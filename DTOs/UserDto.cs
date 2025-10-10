/************************************************************************************************
* Filename:         UserDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) that represents a system user when their
* data is sent to the client. It provides a safe, structured way to expose user information
* without revealing sensitive data like the password hash.
************************************************************************************************/

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// DTO representing a system user for client-side display.
    /// </summary>
    public class UserDto
    {
        // Property: The unique identifier for the user.
        public string Id { get; set; } = string.Empty;

        // Property: The user's first name.
        public string FirstName { get; set; } = string.Empty;

        // Property: The user's last name.
        public string LastName { get; set; } = string.Empty;

        // Property: The user's email address, used for login and communication.
        public string Email { get; set; } = string.Empty;

        // Property: The user's contact phone number.
        public string PhoneNumber { get; set; } = string.Empty;

        // Property: The user's physical address (optional).
        public string? Address { get; set; }

        // Property: The user's assigned role as a string (e.g., "Backoffice", "StationOperator").
        public string Role { get; set; } = string.Empty;

        // Property: The user's account status as a string (e.g., "Active", "Inactive").
        public string Status { get; set; } = string.Empty;

        // Property: The date and time when the user account was created.
        public DateTime CreatedAt { get; set; }

        // Property: The date and time when the user account was last updated (optional).
        public DateTime? UpdatedAt { get; set; }

        // Property: The URL for the user's profile image (optional).
        public string? ProfileImage { get; set; }

        // Property: The date and time of the user's last login (optional).
        public DateTime? LastLogin { get; set; }

        // Property: The ID of the station the user is assigned to (only for StationOperators).
        public string? StationId { get; set; }
    }
}
