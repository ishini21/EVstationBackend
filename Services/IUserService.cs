/************************************************************************************************
* Filename:         IUserService.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the interface for the user management service. It outlines the contract
* for all business logic related to the CRUD (Create, Read, Update, Delete) operations
* for system users (Backoffice and StationOperator).
************************************************************************************************/

using EVOwnerManagement.API.DTOs;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Defines the contract for the user management service.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all system users.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="UserDto"/>.</returns>
        Task<List<UserDto>> GetAllAsync();

        /// <summary>
        /// Asynchronously retrieves a single system user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the user to retrieve.</param>
        /// <returns>A task containing the <see cref="UserDto"/> if found; otherwise, null.</returns>
        Task<UserDto?> GetByIdAsync(string id);

        /// <summary>
        /// Asynchronously creates a new system user based on the provided data.
        /// </summary>
        /// <param name="createDto">The DTO containing the data for the new user.</param>
        /// <returns>A task containing the newly created <see cref="UserDto"/>.</returns>
        Task<UserDto> CreateAsync(CreateUserDto createDto);

        /// <summary>
        /// Asynchronously updates an existing system user's information.
        /// </summary>
        /// <param name="id">The unique ID of the user to update.</param>
        /// <param name="updateDto">The DTO containing the updated user data.</param>
        /// <returns>A task containing the updated <see cref="UserDto"/> if the user was found and updated; otherwise, null.</returns>
        Task<UserDto?> UpdateAsync(string id, UpdateUserDto updateDto);

        /// <summary>
        /// Asynchronously deactivates a system user's account.
        /// </summary>
        /// <param name="id">The unique ID of the user to deactivate.</param>
        /// <returns>A task containing a boolean value indicating whether the operation was successful.</returns>
        Task<bool> DeactivateAsync(string id);

        /// <summary>
        /// Asynchronously reactivates a system user's account.
        /// </summary>
        /// <param name="id">The unique ID of the user to reactivate.</param>
        /// <returns>A task containing a boolean value indicating whether the operation was successful.</returns>
        Task<bool> ReactivateAsync(string id);

        /// <summary>
        /// Asynchronously resets a system user's password.
        /// </summary>
        /// <param name="id">The unique ID of the user whose password will be reset.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <returns>A task containing a boolean value indicating whether the password reset was successful.</returns>
        Task<bool> ResetPasswordAsync(string id, string newPassword);
    }
}
