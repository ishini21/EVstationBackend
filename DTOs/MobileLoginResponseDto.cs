namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Mobile login response DTO with role information for UI routing
    /// </summary>
    public class MobileLoginResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NIC { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty; // "StationOperator" or "EVOwner"
        public DateTime ExpiresAt { get; set; }
    }
}
