using Microsoft.AspNetCore.Mvc;

namespace EVOwnerManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EVOwnersController : ControllerBase
    {
        // GET: api/evowners
        [HttpGet]
        public IActionResult Get()
        {
            // Temporary hardcoded data
            var owners = new[]
            {
                new { NIC = "123456789V", FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "1234567890", IsActive = true },
                new { NIC = "987654321V", FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Phone = "0987654321", IsActive = false }
            };

            return Ok(owners);
        }
    }
}