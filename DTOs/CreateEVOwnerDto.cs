/************************************************************************************************
* Filename:         CreateEVOwnerDto.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Akmeemana I S-IT22136060
* Date:             10-Oct-2025
*************************************************************************************************/




namespace EVOwnerManagement.API.DTOs
{
    public class CreateEVOwnerDto
    {
        public string NIC { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}