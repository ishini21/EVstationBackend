using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Services;
using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.Models;
using MongoDB.Driver;

namespace EVOwnerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly MongoDbContext _context;

        public AuthController(IAuthService authService, MongoDbContext context)
        {
            _authService = authService;
            _context = context;
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
    }
}

