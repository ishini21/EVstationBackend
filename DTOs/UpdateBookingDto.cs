/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the UpdateBookingDto for updating existing booking reservations.
 * It includes validation attributes and data transfer object structure.
 */

using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating an existing booking reservation
    /// </summary>
    public class UpdateBookingDto
    {
        [Required(ErrorMessage = "Customer NIC is required")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "NIC must be between 10 and 12 characters")]
        public string CustomerNic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reservation start time is required")]
        public DateTime ReservationStartTime { get; set; }

        [Required(ErrorMessage = "Duration in minutes is required")]
        [Range(30, 480, ErrorMessage = "Duration must be between 30 minutes and 8 hours")]
        public int DurationMinutes { get; set; }

        [Range(0, 1000, ErrorMessage = "Estimated kWh must be between 0 and 1000")]
        public double EstimatedKWh { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
