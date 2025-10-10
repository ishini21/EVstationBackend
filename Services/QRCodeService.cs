/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - QR Code Service Implementation
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the implementation of QR code generation and validation services.
 * It handles QR code creation, encoding, and validation for the booking system.
 */

using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using QRCoder;
using System.Text.Json;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Service implementation for QR code generation and validation operations
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        private readonly MongoDbContext _context;
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(MongoDbContext context, ILogger<QRCodeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Generates a QR code for a booking with encoded booking details
        /// </summary>
        public async Task<string> GenerateQRCodeAsync(Booking booking)
        {
            try
            {
                // Encode booking data into QR code structure
                var qrData = EncodeBookingData(booking);
                
                // Serialize the data to JSON
                var jsonData = JsonSerializer.Serialize(qrData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Generate QR code using QRCoder
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new Base64QRCode(qrCodeData);
                
                // Generate base64 encoded QR code image
                var qrCodeImageAsBase64 = qrCode.GetGraphic(20);
                
                _logger.LogInformation("QR code generated successfully for booking {BookingId}", booking.Id);
                
                return qrCodeImageAsBase64;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for booking {BookingId}", booking.Id);
                throw new InvalidOperationException("Failed to generate QR code", ex);
            }
        }

        /// <summary>
        /// Validates a QR code and returns booking details if valid
        /// </summary>
        public async Task<QRValidationResponseDto> ValidateQRCodeAsync(QRValidationRequestDto qrData)
        {
            try
            {
                // Find the booking by ID
                var booking = await _context.Bookings.Find(b => b.Id == qrData.BookingId).FirstOrDefaultAsync();
                
                if (booking == null)
                {
                    return new QRValidationResponseDto
                    {
                        IsValid = false,
                        Message = "Booking not found",
                        ErrorCode = "BOOKING_NOT_FOUND"
                    };
                }

                // Validate booking details match QR code data
                if (booking.CustomerNic != qrData.EvOwnerNIC)
                {
                    return new QRValidationResponseDto
                    {
                        IsValid = false,
                        Message = "Invalid customer information",
                        ErrorCode = "INVALID_CUSTOMER"
                    };
                }

                if (booking.StationId != qrData.StationId)
                {
                    return new QRValidationResponseDto
                    {
                        IsValid = false,
                        Message = "Invalid station information",
                        ErrorCode = "INVALID_STATION"
                    };
                }

                // Check if booking is in a valid state for charging
                if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.InProgress)
                {
                    return new QRValidationResponseDto
                    {
                        IsValid = false,
                        Message = $"Booking is not in a valid state for charging. Current status: {booking.Status}",
                        ErrorCode = "INVALID_STATUS"
                    };
                }

                // Check if booking is not expired
                if (booking.ReservationStartTime > DateTime.UtcNow.AddHours(1)) // Allow 1 hour buffer
                {
                    return new QRValidationResponseDto
                    {
                        IsValid = false,
                        Message = "Booking is not yet active",
                        ErrorCode = "BOOKING_NOT_ACTIVE"
                    };
                }

                if (booking.ReservationEndTime < DateTime.UtcNow)
                {
                    return new QRValidationResponseDto
                    {
                        IsValid = false,
                        Message = "Booking has expired",
                        ErrorCode = "BOOKING_EXPIRED"
                    };
                }

                // If all validations pass, return success with booking details
                var bookingDto = MapToDto(booking);
                
                _logger.LogInformation("QR code validation successful for booking {BookingId}", booking.Id);
                
                return new QRValidationResponseDto
                {
                    IsValid = true,
                    Message = "QR code is valid",
                    BookingDetails = bookingDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating QR code for booking {BookingId}", qrData.BookingId);
                return new QRValidationResponseDto
                {
                    IsValid = false,
                    Message = "An error occurred during validation",
                    ErrorCode = "VALIDATION_ERROR"
                };
            }
        }

        /// <summary>
        /// Encodes booking details into QR code data structure
        /// </summary>
        public QRCodeDataDto EncodeBookingData(Booking booking)
        {
            return new QRCodeDataDto
            {
                BookingId = booking.Id,
                EvOwnerNIC = booking.CustomerNic,
                StationId = booking.StationId,
                ReservationDate = booking.ReservationStartTime
            };
        }

        /// <summary>
        /// Maps a Booking entity to BookingDto
        /// </summary>
        private static BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                CustomerNic = booking.CustomerNic,
                CustomerName = booking.CustomerName,
                CustomerEmail = booking.CustomerEmail,
                CustomerPhone = booking.CustomerPhone,
                StationId = booking.StationId,
                StationName = booking.StationName,
                SlotId = booking.SlotId,
                SlotCode = booking.SlotCode,
                ReservationStartTime = booking.ReservationStartTime,
                ReservationEndTime = booking.ReservationEndTime,
                DurationMinutes = booking.DurationMinutes,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                PricePerKWh = booking.PricePerKWh,
                EstimatedKWh = booking.EstimatedKWh,
                QrCode = booking.QrCode,
                Notes = booking.Notes,
                CreatedBy = booking.CreatedBy,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                CancelledAt = booking.CancelledAt,
                CancellationReason = booking.CancellationReason
            };
        }
    }
}
