using EVOwnerManagement.API.DTOs;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Mobile authentication service interface for unified login
    /// </summary>
    public interface IMobileAuthService
    {
        /// <summary>
        /// Authenticate user via mobile login (supports both email and NIC)
        /// </summary>
        /// <param name="mobileLoginDto">Mobile login credentials</param>
        /// <returns>Mobile login response with role information</returns>
        Task<MobileLoginResponseDto?> MobileLoginAsync(MobileLoginDto mobileLoginDto);
    }
}
