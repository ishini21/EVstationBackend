using System.ComponentModel.DataAnnotations;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; } // Optional - only updated if provided

        public UserRole? Role { get; set; }

        [StringLength(500, ErrorMessage = "Profile image URL cannot exceed 500 characters")]
        public string? ProfileImage { get; set; }
    }
}

