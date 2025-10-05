using Microsoft.AspNetCore.Mvc;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Services;
using EVOwnerManagement.API.Models;
using MongoDB.Driver;
using EVOwnerManagement.API.Data;

namespace EVOwnerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public SetupController(MongoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create the first admin user (only works if no users exist in the database)
        /// </summary>
        /// <param name="createDto">Admin user creation data</param>
        /// <returns>Created admin user information</returns>
        [HttpPost("create-admin")]
        public async Task<ActionResult<UserDto>> CreateFirstAdmin([FromBody] CreateUserDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if any users already exist
                var userCount = await _context.Users.CountDocumentsAsync(_ => true);
                
                if (userCount > 0)
                {
                    return Conflict(new { message = "Admin user already exists. Cannot create another admin user." });
                }

                // Ensure the role is Backoffice for admin
                createDto.Role = UserRole.Backoffice;

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password);

                var user = new User
                {
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    Email = createDto.Email,
                    PhoneNumber = createDto.PhoneNumber,
                    Address = createDto.Address,
                    PasswordHash = passwordHash,
                    Role = createDto.Role,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    ProfileImage = null
                };

                await _context.Users.InsertOneAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = user.Role.ToString(),
                    Status = user.Status.ToString(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    ProfileImage = user.ProfileImage
                };

                return CreatedAtAction(nameof(CreateFirstAdmin), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Check if the system is already set up (has users)
        /// </summary>
        /// <returns>Setup status</returns>
        [HttpGet("status")]
        public async Task<ActionResult<object>> GetSetupStatus()
        {
            try
            {
                var userCount = await _context.Users.CountDocumentsAsync(_ => true);
                return Ok(new { 
                    isSetup = userCount > 0, 
                    userCount = userCount,
                    message = userCount > 0 ? "System is already set up" : "System needs initial setup"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
