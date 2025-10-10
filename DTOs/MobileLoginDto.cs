/************************************************************************************************
* Filename:         MobileLoginDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) for the unified mobile login endpoint.
* It allows both Station Operators (using email) and EV Owners (using NIC) to authenticate
* through a single interface, simplifying the mobile app's login logic.
************************************************************************************************/

using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Mobile login DTO that accepts both email and NIC for unified login.
    /// </summary>
    public class MobileLoginDto
    {
        // Property: A unique identifier for the user, which can be either an email
        // (for StationOperators) or an NIC (for EVOwners). This field is required.
        [Required(ErrorMessage = "Email or NIC is required")]
        public string Identifier { get; set; } = string.Empty;

        // Property: The user's password for authentication. This field is required.
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
