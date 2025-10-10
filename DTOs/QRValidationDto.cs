/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - QR Code Validation
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains DTOs for QR code validation functionality.
 * It defines the structure for QR code data encoding and validation responses.
 */

using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for QR code data encoding
    /// Contains the essential booking information encoded in QR codes
    /// </summary>
    public class QRCodeDataDto
    {
        public string BookingId { get; set; } = string.Empty;
        public string EvOwnerNIC { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for QR code validation request
    /// Contains the decoded QR code data for validation
    /// </summary>
    public class QRValidationRequestDto
    {
        public string BookingId { get; set; } = string.Empty;
        public string EvOwnerNIC { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for QR code validation response
    /// Contains the validation result and booking details if valid
    /// </summary>
    public class QRValidationResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public BookingDto? BookingDetails { get; set; }
        public string? ErrorCode { get; set; }
    }
}
