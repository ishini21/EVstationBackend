/************************************************************************************************
* Filename:         MobileLoginResponseDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) sent to the mobile client after a
* successful unified login. It contains all necessary user information, the JWT token,
* and a 'UserType' field to help the mobile app route to the correct dashboard
* (either for a Station Operator or an EV Owner).
************************************************************************************************/

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Mobile login response DTO with role information for UI routing.
    /// </summary>
    public class MobileLoginResponseDto
    {
        // Property: The unique identifier for the logged-in user or EV owner.
        public string UserId { get; set; } = string.Empty;

        // Property: The JSON Web Token (JWT) for authenticating subsequent API requests.
        public string Token { get; set; } = string.Empty;

        // Property: The first name of the logged-in user.
        public string FirstName { get; set; } = string.Empty;

        // Property: The last name of the logged-in user.
        public string LastName { get; set; } = string.Empty;

        // Property: The email address of the logged-in user (will be empty for EV Owners).
        public string Email { get; set; } = string.Empty;

        // Property: The National Identity Card (NIC) number (will be empty for Station Operators).
        public string NIC { get; set; } = string.Empty;

        // Property: The specific role of the user (e.g., "StationOperator", "EVOwner").
        public string Role { get; set; } = string.Empty;

        // Property: A clear identifier for the type of user, used for client-side routing.
        public string UserType { get; set; } = string.Empty; // "StationOperator" or "EVOwner"
        
        // Property: The exact date and time when the JWT token will expire.
        public DateTime ExpiresAt { get; set; }
    }
}
