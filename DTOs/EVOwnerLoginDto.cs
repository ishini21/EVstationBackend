using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// EV Owner login DTO for mobile authentication
    /// </summary>
    public class EVOwnerLoginDto
    {
        /// <summary>
        /// NIC number for EV Owner authentication
        /// </summary>
        [Required(ErrorMessage = "NIC is required")]
        public string NIC { get; set; } = string.Empty;

        /// <summary>
        /// Password for authentication
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}