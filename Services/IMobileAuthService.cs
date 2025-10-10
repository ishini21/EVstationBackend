/************************************************************************************************
* Filename:         IMobileAuthService.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the interface for the mobile authentication service. It specifies
* the contract for a unified login process for the mobile application, which needs to
* handle authentication for both Station Operators (by email) and EV Owners (by NIC).
************************************************************************************************/

using EVOwnerManagement.API.DTOs;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Defines the contract for the mobile authentication service, providing a unified login endpoint.
    /// </summary>
    public interface IMobileAuthService
    {
        /// <summary>
        /// Asynchronously authenticates a user via the mobile application. This single method
        /// is designed to handle logins for both Station Operators (using their email) and
        /// EV Owners (using their NIC).
        /// </summary>
        /// <param name="mobileLoginDto">A DTO containing the identifier (email or NIC) and password.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a 
        /// <see cref="MobileLoginResponseDto"/> with user and role information if authentication is successful; 
        /// otherwise, null.
        /// </returns>
        Task<MobileLoginResponseDto?> MobileLoginAsync(MobileLoginDto mobileLoginDto);
    }
}
