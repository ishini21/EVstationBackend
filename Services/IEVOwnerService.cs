/************************************************************************************************
* Filename:         IEVOwnerService.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Akmeemana I S-IT22136060
* Date:             10-Oct-2025
*************************************************************************************************/



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