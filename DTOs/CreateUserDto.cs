/************************************************************************************************
* Filename:         CreateUserDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) for creating a new system user.
* It includes all the necessary properties and data validation attributes required to
* create a Backoffice or StationOperator user.
************************************************************************************************/

using System.ComponentModel.DataAnnotations;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.DTOs
{
    public class CreateUserDto
    {
        // Property: User's first name. It is required and must be between 2 and 50 characters.
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        // Property: User's last name. It is required and must be between 2 and 50 characters.
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; } = string.Empty;

        // Property: User's email address. It is required and must be a valid email format.
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        // Property: User's phone number. It is required and must be a valid phone number format.
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Property: User's physical address. This field is optional.
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        // Property: User's password. It is required and must be at least 6 characters long.
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        // Property: User's role in the system (Backoffice or StationOperator). This is required.
        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }
    }
}
