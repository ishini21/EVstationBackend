namespace EVOwnerManagement.API.Models
{
    /// <summary>
    /// User account status
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// User account is active and can login
        /// </summary>
        Active = 0,

        /// <summary>
        /// User account is inactive and cannot login
        /// </summary>
        Inactive = 1
    }
}

