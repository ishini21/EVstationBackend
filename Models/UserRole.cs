namespace EVOwnerManagement.API.Models
{
    /// <summary>
    /// User roles in the EV Station Management System
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Full administrative access - can manage all system resources
        /// </summary>
        Backoffice = 0,

        /// <summary>
        /// Station operator - limited access for managing station operations
        /// </summary>
        StationOperator = 1
    }
}

