/************************************************************************************************
* Filename:         EVOwnersController.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Akmeemana I S-IT22136060
* Date:             10-Oct-2025
*************************************************************************************************/


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EVOwnerManagement.API.Services;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class EVOwnersController : ControllerBase
    {
        private readonly IEVOwnerService _evOwnerService;

        public EVOwnersController(IEVOwnerService evOwnerService)
        {
            _evOwnerService = evOwnerService;
        }
        //get owner list
        [HttpGet]
        public async Task<ActionResult<List<EVOwnerDto>>> GetAll()
        {
            try
            {
                var owners = await _evOwnerService.GetAllAsync();
                return Ok(owners);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //search owner
        [HttpGet("search")]
        public async Task<ActionResult<List<EVOwnerDto>>> Search([FromQuery] string query)
        {
            try
            {
                var owners = await _evOwnerService.SearchAsync(query);
                return Ok(owners);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //get owner by id
        [HttpGet("{nic}")]
        public async Task<ActionResult<EVOwnerDto>> GetByNIC(string nic)
        {
            try
            {
                var owner = await _evOwnerService.GetByNICAsync(nic);
                if (owner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found.");
                }
                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //add evowner
        [HttpPost]
        public async Task<ActionResult<EVOwnerDto>> Create([FromBody] CreateEVOwnerDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _evOwnerService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetByNIC), new { nic = owner.NIC }, owner);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //update evowner details
        [HttpPut("{nic}")]
        public async Task<ActionResult<EVOwnerDto>> Update(string nic, [FromBody] UpdateEVOwnerDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _evOwnerService.UpdateAsync(nic, updateDto);
                if (owner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found.");
                }
                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //delete evowner profile
        [HttpDelete("{nic}")]
        public async Task<ActionResult> Delete(string nic)
        {
            try
            {
                var result = await _evOwnerService.DeleteAsync(nic);
                if (!result)
                {
                    return NotFound($"EV Owner with NIC {nic} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //evowner mobile register
        [HttpPost("self-register")]
        [AllowAnonymous]
        public async Task<ActionResult<EVOwnerDto>> SelfRegister([FromBody] CreateEVOwnerDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _evOwnerService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetByNIC), new { nic = owner.NIC }, owner);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //evowner profile get by id
        [HttpGet("profile/{nic}")]
        [Authorize] 
        public async Task<ActionResult<EVOwnerDto>> GetProfile(string nic)
        {
            try
            {
                
                var currentUserNic = User.FindFirst("nic")?.Value ??
                                   User.FindFirst("sub")?.Value ??
                                   User.Identity?.Name;

                
                if (!string.IsNullOrEmpty(currentUserNic) && currentUserNic != nic)
                {
                    return Forbid("You can only access your own profile.");
                }

                var owner = await _evOwnerService.GetByNICAsync(nic);
                if (owner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found.");
                }

                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //update profile by  nic 
        [HttpPut("profile/{nic}")]
        [Authorize] 
        public async Task<ActionResult<EVOwnerDto>> UpdateProfile(string nic, [FromBody] UpdateEVOwnerDto updateDto)
        {
            try
            {
                
                var currentUserNic = User.FindFirst("nic")?.Value ??
                                   User.FindFirst("sub")?.Value ??
                                   User.Identity?.Name;

                
                if (!string.IsNullOrEmpty(currentUserNic) && currentUserNic != nic)
                {
                    return Forbid("You can only update your own profile.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _evOwnerService.UpdateAsync(nic, updateDto);
                if (owner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found.");
                }

                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
       //deactivate evowner account
        [HttpPatch("profile/{nic}/deactivate")]
        [Authorize]
        public async Task<ActionResult> DeactivateAccount(string nic)
        {
            try
            {
                
                var currentUserNic = User.FindFirst("nic")?.Value ??
                                   User.FindFirst("sub")?.Value ??
                                   User.Identity?.Name;

               
                if (!string.IsNullOrEmpty(currentUserNic) && currentUserNic != nic)
                {
                    return Forbid("You can only deactivate your own account.");
                }

                var result = await _evOwnerService.DeactivateAsync(nic, currentUserNic);
                if (result == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found.");
                }
                if (result == false)
                {
                    return BadRequest("Account is already deactivated.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //toggle active-inactive

        [HttpPatch("{nic}/toggle-active")]
        public async Task<ActionResult> ToggleActive(string nic)
        {
            try
            {
                var result = await _evOwnerService.ToggleActiveStatusAsync(nic);
                if (!result)
                {
                    return NotFound($"EV Owner with NIC {nic} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}