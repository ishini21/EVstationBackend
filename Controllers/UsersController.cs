/************************************************************************************************
* Filename:         UsersController.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file contains the UsersController for handling HTTP requests related to system user
* management (Backoffice and StationOperator). It provides RESTful API endpoints for CRUD
* operations, accessible only by users with the Backoffice role.
************************************************************************************************/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Services;

namespace EVOwnerManagement.API.Controllers
{
    /// <summary>
    /// User Management Controller - BACKOFFICE ONLY
    /// Handles user CRUD operations, only accessible by Backoffice users
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Backoffice")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        // Constructor: Injects the IUserService dependency to handle business logic.
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users (Backoffice only)
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        // Method: Retrieves a list of all system users.
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get user by ID (Backoffice only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information</returns>
        [HttpGet("{id}")]
        // Method: Retrieves a single system user by their unique ID.
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create a new user (Backoffice only)
        /// </summary>
        /// <param name="createDto">User creation data</param>
        /// <returns>Created user information</returns>
        [HttpPost]
        // Method: Creates a new system user based on the provided data.
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update user basic information (Backoffice only) - Name, Email, Phone, Role
        /// Password is optional - only updated if provided
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateDto">User update data (password optional)</param>
        /// <returns>Updated user information</returns>
        [HttpPut("{id}")]
        // Method: Updates an existing system user's information.
        public async Task<ActionResult<UserDto>> Update(string id, [FromBody] UpdateUserDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.UpdateAsync(id, updateDto);
                
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Reset user password (Backoffice only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="resetPasswordDto">New password data</param>
        /// <returns>Success message</returns>
        [HttpPatch("{id}/reset-password")]
        // Method: Resets the password for a specific system user.
        public async Task<ActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.ResetPasswordAsync(id, resetPasswordDto.NewPassword);
                
                if (!result)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Deactivate a user (Backoffice only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        [HttpPatch("{id}/deactivate")]
        // Method: Deactivates a user's account, setting their status to Inactive.
        public async Task<ActionResult> Deactivate(string id)
        {
            try
            {
                var result = await _userService.DeactivateAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Reactivate a user (Backoffice only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        [HttpPatch("{id}/reactivate")]
        // Method: Reactivates a user's account, setting their status to Active.
        public async Task<ActionResult> Reactivate(string id)
        {
            try
            {
                var result = await _userService.ReactivateAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
