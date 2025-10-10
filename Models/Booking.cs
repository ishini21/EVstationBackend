/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the Booking model for the EV Station Management System.
 * It defines the structure for booking reservations including customer information,
 * slot details, timing constraints, and status tracking.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVOwnerManagement.API.Models
{
    /// <summary>
    /// Represents a booking reservation for an EV charging slot
    /// </summary>
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("bookingNumber")]
        public string BookingNumber { get; set; } = string.Empty;

        [BsonElement("customerNic")]
        public string CustomerNic { get; set; } = string.Empty;

        [BsonElement("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [BsonElement("customerEmail")]
        public string CustomerEmail { get; set; } = string.Empty;

        [BsonElement("customerPhone")]
        public string CustomerPhone { get; set; } = string.Empty;

        [BsonElement("evOwnerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? EvOwnerId { get; set; }

        [BsonElement("stationId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StationId { get; set; } = string.Empty;

        [BsonElement("stationName")]
        public string StationName { get; set; } = string.Empty;

        [BsonElement("slotId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SlotId { get; set; } = string.Empty;

        [BsonElement("slotCode")]
        public string SlotCode { get; set; } = string.Empty;

        [BsonElement("reservationDate")]
        public DateTime? ReservationDate { get; set; }

        [BsonElement("reservationStartTime")]
        public DateTime ReservationStartTime { get; set; }

        [BsonElement("reservationEndTime")]
        public DateTime ReservationEndTime { get; set; }

        [BsonElement("durationMinutes")]
        public int DurationMinutes { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("pricePerKWh")]
        public double PricePerKWh { get; set; }

        [BsonElement("estimatedKWh")]
        public double EstimatedKWh { get; set; }

        [BsonElement("qrCode")]
        public string? QrCode { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("createdBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("cancelledAt")]
        public DateTime? CancelledAt { get; set; }

        [BsonElement("cancellationReason")]
        public string? CancellationReason { get; set; }
    }

    /// <summary>
    /// Enumeration of possible booking statuses
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Booking is pending approval
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Booking is confirmed and active
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Booking is currently in progress
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Booking has been completed
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Booking has been cancelled
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Booking has expired without being used
        /// </summary>
        Expired = 5
    }
}
