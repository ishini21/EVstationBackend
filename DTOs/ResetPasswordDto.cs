/************************************************************************************************
* Filename:         ResetPasswordDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the Data Transfer Object (DTO) used when a Backoffice user resets
* the password for another system user. It contains the new password with validation
* rules to ensure a minimum level of security.
************************************************************************************************/

using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// DTO for resetting a user's password.
    /// </summary>
    public class ResetPasswordDto
    {
        // Property: The new password for the user. It is required and must be at least 6 characters long.
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
