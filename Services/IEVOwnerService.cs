using EVOwnerManagement.API.Models;
using EVOwnerManagement.API.DTOs;

namespace EVOwnerManagement.API.Services
{
    public interface IEVOwnerService
    {
        Task<List<EVOwnerDto>> GetAllAsync();
        Task<EVOwnerDto?> GetByNICAsync(string nic);
        Task<EVOwnerDto?> GetByIdAsync(string id);
        Task<EVOwnerDto> CreateAsync(CreateEVOwnerDto createDto);
        Task<EVOwnerDto?> UpdateAsync(string nic, UpdateEVOwnerDto updateDto);
        Task<bool> DeleteAsync(string nic);
        Task<bool> ToggleActiveStatusAsync(string nic);
        Task<bool?> DeactivateAsync(string nic, string? performedBy = null);
        Task<List<EVOwnerDto>> SearchAsync(string query);
        Task<EVOwnerDto?> LoginAsync(string nic, string password);
    }
}