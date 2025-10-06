using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EVOwnerManagement.API.Services;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Controllers
{
    /// <summary>
    /// EV Owner Management Controller - BACKOFFICE ONLY
    /// Handles EV owner CRUD operations, only accessible by Backoffice users
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Backoffice")]
    public class EVOwnersController : ControllerBase
    {
        private readonly IEVOwnerService _evOwnerService;

        public EVOwnersController(IEVOwnerService evOwnerService)
        {
            _evOwnerService = evOwnerService;
        }

        [HttpGet]
        [Authorize(Roles = "Backoffice")]
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

        [HttpGet("search")]
        [Authorize(Roles = "Backoffice")]
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

        [HttpGet("{nic}")]
        [Authorize(Roles = "Backoffice")]
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

        [HttpPost]
        [Authorize(Roles = "Backoffice")]
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

        [HttpPut("{nic}")]
        [Authorize(Roles = "Backoffice")]
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

        [HttpDelete("{nic}")]
        [Authorize(Roles = "Backoffice")]
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

        [HttpPatch("{nic}/toggle-active")]
        [Authorize(Roles = "Backoffice")]
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