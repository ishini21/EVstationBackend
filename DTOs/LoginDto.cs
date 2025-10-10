/************************************************************************************************
* Filename:         LoginDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) for handling login requests from
* web users (Backoffice and StationOperator). It specifies the required credentials
* (email and password) and their validation rules.
************************************************************************************************/

using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    public class LoginDto
    {
        // Property: The user's email address. It is required and must be a valid email format.
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        // Property: The user's password. It is required.
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
