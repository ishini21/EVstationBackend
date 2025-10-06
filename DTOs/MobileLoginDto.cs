using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Mobile login DTO that accepts both email and NIC for unified login
    /// </summary>
    public class MobileLoginDto
    {
        /// <summary>
        /// Either email (for StationOperator) or NIC (for EVOwner)
        /// </summary>
        [Required(ErrorMessage = "Email or NIC is required")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Password for authentication
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
