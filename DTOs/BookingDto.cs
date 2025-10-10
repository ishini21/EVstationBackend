/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the BookingDto for returning booking information in API responses.
 * It provides a clean data structure for client consumption.
 */

using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for returning booking information in API responses
    /// </summary>
    public class BookingDto
    {
        public string Id { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty;
        public string CustomerNic { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
        public DateTime ReservationStartTime { get; set; }
        public DateTime ReservationEndTime { get; set; }
        public int DurationMinutes { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public double PricePerKWh { get; set; }
        public double EstimatedKWh { get; set; }
        public string? QrCode { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}
