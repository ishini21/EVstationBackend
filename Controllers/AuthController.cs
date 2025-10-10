/************************************************************************************************
* Filename:         AuthController.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file contains the AuthController for handling all authentication-related HTTP requests.
* It provides API endpoints for logging in web and mobile users, verifying JWT tokens,
* and handling distinct login flows for different user roles.
************************************************************************************************/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Services;
using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.Models;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EVOwnerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMobileAuthService _mobileAuthService;
        private readonly IEVOwnerService _evOwnerService;
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor: Injects required services using Dependency Injection.
        public AuthController(IAuthService authService, IEVOwnerService evOwnerService, MongoDbContext context, IConfiguration configuration, IMobileAuthService mobileAuthService)
        {
            _authService = authService;
            _mobileAuthService = mobileAuthService;
            _evOwnerService = evOwnerService;
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Verify JWT token and return user information
        /// </summary>
        /// <returns>User information if token is valid</returns>
        [HttpGet("verify")]
        [Authorize]
        // Method: Verifies the authenticity of the JWT token sent in the request header.
        public async Task<ActionResult<UserDto>> VerifyToken()
        {
            try
            {
                // Get user ID from the 'sub' (subject) claim in the JWT token.
                var userId = User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token: User ID is missing." });
                }

                // Retrieve the user from the database to ensure they still exist.
                var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Security Check: Ensure the user's account is currently active.
                if (user.Status != UserStatus.Active)
                {
                    return Unauthorized(new { message = "User account is inactive." });
                }

                // Return a simplified user profile to the frontend.
                var response = new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    role = user.Role.ToString(),
                    profileImage = user.ProfileImage
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any unexpected server errors.
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Login endpoint for web users (Backoffice and StationOperator)
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        // Method: Handles login requests from the web application for system users.
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Validate the incoming request body against the DTO's rules.
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Delegate the core login logic to the authentication service.
                var response = await _authService.LoginAsync(loginDto);

                if (response == null)
                {
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Catch specific exceptions from the service layer, like account status issues.
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Mobile login endpoint for both Station Operators and EV Owners
        /// </summary>
        /// <param name="mobileLoginDto">Mobile login credentials (email or NIC + password)</param>
        /// <returns>JWT token and user information with role for UI routing</returns>
        [HttpPost("mobile-login")]
        // Method: Provides a single login endpoint for the mobile application.
        public async Task<ActionResult<MobileLoginResponseDto>> MobileLogin([FromBody] MobileLoginDto mobileLoginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Delegate to the mobile-specific authentication service.
                var response = await _mobileAuthService.MobileLoginAsync(mobileLoginDto);

                if (response == null)
                {
                    return Unauthorized(new { message = "Invalid credentials. Please check your email/NIC and password." });
                }

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Login endpoint for EV Owners (Mobile App)
        /// </summary>
        /// <param name="loginDto">EV Owner login credentials (NIC and Password)</param>
        /// <returns>JWT token and EV Owner information</returns>
        [HttpPost("evowner-login")]
        // Method: Handles login requests specifically from EV Owners on the mobile app.
        public async Task<ActionResult> EVOwnerLogin([FromBody] EVOwnerLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Use the EVOwner service to validate credentials.
                var owner = await _evOwnerService.LoginAsync(loginDto.NIC, loginDto.Password);

                if (owner == null)
                {
                    return Unauthorized(new { message = "Invalid NIC or password." });
                }

                // Generate a JWT token specifically for the EV Owner.
                var token = GenerateEVOwnerJwtToken(owner.Id, owner.NIC);
                var expiresAt = DateTime.UtcNow.AddHours(
                    double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
                );

                var response = new
                {
                    userId = owner.Id,
                    token,
                    owner = new
                    {
                        id = owner.Id,
                        nic = owner.NIC,
                        firstName = owner.FirstName,
                        lastName = owner.LastName,
                        email = owner.Email,
                        phone = owner.Phone,
                        isActive = owner.IsActive
                    },
                    expiresAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Method: A private helper to generate a JWT for an EV Owner after successful login.
        private string GenerateEVOwnerJwtToken(string ownerId, string nic)
        {
            // Load JWT settings from appsettings.json.
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var issuer = jwtSettings["Issuer"] ?? "EVStationBackend";
            var audience = jwtSettings["Audience"] ?? "EVStationFrontend";
            var expirationHours = double.Parse(jwtSettings["ExpirationHours"] ?? "24");

            // Create security key and signing credentials.
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define the claims to be included in the token.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, ownerId),
                new Claim(JwtRegisteredClaimNames.UniqueName, nic),
                new Claim(ClaimTypes.Role, "EVOwner"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            // Create the JWT token object.
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            // Serialize the token into a string.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
