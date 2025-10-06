using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// DTO for resetting user password
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// New password for the user
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
