/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - QR Code Service Interface
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the interface for QR code generation and validation services.
 * It defines the contract for QR code operations in the booking system.
 */

using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Interface for QR code generation and validation operations
    /// </summary>
    public interface IQRCodeService
    {
        /// <summary>
        /// Generates a QR code for a booking with encoded booking details
        /// </summary>
        /// <param name="booking">The booking to generate QR code for</param>
        /// <returns>Base64 encoded QR code image</returns>
        Task<string> GenerateQRCodeAsync(Booking booking);

        /// <summary>
        /// Validates a QR code and returns booking details if valid
        /// </summary>
        /// <param name="qrData">The decoded QR code data</param>
        /// <returns>Validation result with booking details</returns>
        Task<QRValidationResponseDto> ValidateQRCodeAsync(QRValidationRequestDto qrData);

        /// <summary>
        /// Encodes booking details into QR code data structure
        /// </summary>
        /// <param name="booking">The booking to encode</param>
        /// <returns>QR code data structure</returns>
        QRCodeDataDto EncodeBookingData(Booking booking);
    }
}
