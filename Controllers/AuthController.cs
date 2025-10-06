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
        private readonly IEVOwnerService _evOwnerService;
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IEVOwnerService evOwnerService, MongoDbContext context, IConfiguration configuration)
        {
            _authService = authService;
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
        public async Task<ActionResult<UserDto>> VerifyToken()
        {
            try
            {
                // Get user ID from JWT token claims
                var userId = User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Get user from database
                var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if user is active
                if (user.Status != UserStatus.Active)
                {
                    return Unauthorized(new { message = "User account is inactive" });
                }

                // Return simplified user information (same as login response)
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
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Login endpoint for all users (Backoffice and StationOperator)
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.LoginAsync(loginDto);
                
                if (response == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
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
        public async Task<ActionResult> EVOwnerLogin([FromBody] EVOwnerLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _evOwnerService.LoginAsync(loginDto.NIC, loginDto.Password);
                
                if (owner == null)
                {
                    return Unauthorized(new { message = "Invalid NIC or password" });
                }

                // Generate JWT token for EV Owner
                var token = GenerateEVOwnerJwtToken(owner.Id, owner.NIC);
                var expiresAt = DateTime.UtcNow.AddHours(
                    double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
                );

                var response = new
                {
                    token = token,
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
                    expiresAt = expiresAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        private string GenerateEVOwnerJwtToken(string ownerId, string nic)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var issuer = jwtSettings["Issuer"] ?? "EVStationBackend";
            var audience = jwtSettings["Audience"] ?? "EVStationFrontend";
            var expirationHours = double.Parse(jwtSettings["ExpirationHours"] ?? "24");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, ownerId),
                new Claim(JwtRegisteredClaimNames.UniqueName, nic),
                new Claim(ClaimTypes.Role, "EVOwner"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

