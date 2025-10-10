/************************************************************************************************
* Filename:         UserRole.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the enumeration for the different system user roles. Using an enum
* provides type safety and makes the code more readable and maintainable by avoiding the
* use of "magic strings" or numbers for roles.
************************************************************************************************/

namespace EVOwnerManagement.API.Models
{
    /// <summary>
    /// Defines the roles for users within the EV Station Management System.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Represents a user with full administrative access. Backoffice users can manage
        /// all system resources, including other users, stations, and EV owners.
        /// </summary>
        Backoffice = 0,

        /// <summary>
        /// Represents a user with limited access, responsible for managing the operations
        /// of a specific charging station.
        /// </summary>
        StationOperator = 1
    }
}
