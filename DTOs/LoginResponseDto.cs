/************************************************************************************************
* Filename:         LoginResponseDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) that is sent back to the client
* after a successful login. It contains the essential user information and the JWT
* token needed for session management.
************************************************************************************************/

namespace EVOwnerManagement.API.DTOs
{
    public class LoginResponseDto
    {
        // Property: The unique identifier for the logged-in user.
        public string UserId { get; set; } = string.Empty;

        // Property: The JSON Web Token (JWT) used for authenticating subsequent API requests.
        public string Token { get; set; } = string.Empty;

        // Property: The first name of the logged-in user.
        public string FirstName { get; set; } = string.Empty;

        // Property: The last name of the logged-in user.
        public string LastName { get; set; } = string.Empty;

        // Property: The email address of the logged-in user.
        public string Email { get; set; } = string.Empty;

        // Property: The role of the user (e.g., "Backoffice", "StationOperator").
        public string Role { get; set; } = string.Empty;

        // Property: The exact date and time when the JWT token will expire.
        public DateTime ExpiresAt { get; set; }
    }
}
