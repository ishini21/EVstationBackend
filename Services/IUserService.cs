using EVOwnerManagement.API.DTOs;

namespace EVOwnerManagement.API.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(string id);
        Task<UserDto> CreateAsync(CreateUserDto createDto);
        Task<UserDto?> UpdateAsync(string id, UpdateUserDto updateDto);
        Task<bool> DeactivateAsync(string id);
        Task<bool> ReactivateAsync(string id);
        Task<bool> ResetPasswordAsync(string id, string newPassword);
    }
}

