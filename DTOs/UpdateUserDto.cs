/************************************************************************************************
* Filename:         UpdateUserDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) for updating an existing system user's
* information. All properties are nullable to allow for partial updates, meaning the client
* only needs to send the fields they intend to change.
************************************************************************************************/

using System.ComponentModel.DataAnnotations;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// DTO for updating a user's information. All fields are optional.
    /// </summary>
    public class UpdateUserDto
    {
        // Property: The user's updated first name. Must be between 2 and 50 characters if provided.
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string? FirstName { get; set; }

        // Property: The user's updated last name. Must be between 2 and 50 characters if provided.
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string? LastName { get; set; }

        // Property: The user's updated email address. Must be in a valid email format if provided.
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        // Property: The user's updated phone number. Must be between 10 and 20 characters if provided.
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        public string? PhoneNumber { get; set; }

        // Property: The user's updated physical address. Cannot exceed 200 characters if provided.
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        // Property: The user's new password. This is optional and will only be updated if a value is provided.
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        // Property: The user's updated role (e.g., Backoffice or StationOperator).
        public UserRole? Role { get; set; }

        // Property: The URL for the user's updated profile image. Cannot exceed 500 characters.
        [StringLength(500, ErrorMessage = "Profile image URL cannot exceed 500 characters")]
        public string? ProfileImage { get; set; }
    }
}
