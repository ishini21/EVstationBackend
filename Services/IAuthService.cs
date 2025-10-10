/************************************************************************************************
* Filename:         IAuthService.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the interface for the authentication service. It specifies the contract
* that any class implementing this interface must adhere to, ensuring a consistent approach
* to user authentication and token generation for the web application.
************************************************************************************************/

using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Defines the contract for authentication-related operations for web users.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Asynchronously attempts to log in a user with the provided credentials.
        /// </summary>
        /// <param name="loginDto">The data transfer object containing the user's email and password.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a 
        /// <see cref="LoginResponseDto"/> if authentication is successful; otherwise, null.
        /// </returns>
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Generates a JSON Web Token (JWT) for an authenticated user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="identifier">The user's primary identifier (e.g., email).</param>
        /// <param name="role">The role of the user (e.g., Backoffice, StationOperator).</param>
        /// <returns>A string representing the generated JWT.</returns>
        string GenerateJwtToken(string userId, string identifier, UserRole role);
    }
}
