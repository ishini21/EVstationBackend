/************************************************************************************************
* Filename:         UpdateEVOwnerDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Akmeemana I S-IT22136060
* Date:             10-Oct-2025
*************************************************************************************************/




namespace EVOwnerManagement.API.DTOs
{
    public class UpdateEVOwnerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool IsActive { get; set; }
    }
}