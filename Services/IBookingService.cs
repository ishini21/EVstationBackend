/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the IBookingService interface for booking management operations.
 * It defines the contract for booking-related business logic and data operations.
 */

using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Interface for booking management service operations
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// Creates a new booking reservation
        /// </summary>
        /// <param name="createDto">Booking creation data</param>
        /// <param name="createdBy">ID of the user creating the booking</param>
        /// <returns>Created booking DTO</returns>
        Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto, string createdBy);

        /// <summary>
        /// Gets a booking by its ID
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <param name="userId">User ID requesting the booking</param>
        /// <param name="userRole">Role of the requesting user</param>
        /// <returns>Booking DTO if found and accessible</returns>
        Task<BookingDto?> GetBookingByIdAsync(string bookingId, string userId, UserRole userRole);

        /// <summary>
        /// Gets a paginated list of bookings with filters
        /// </summary>
        /// <param name="queryDto">Query parameters and filters</param>
        /// <param name="userId">User ID requesting the bookings</param>
        /// <param name="userRole">Role of the requesting user</param>
        /// <returns>Paginated booking response</returns>
        Task<BookingResponseDto> GetBookingsAsync(BookingQueryDto queryDto, string userId, UserRole userRole);

        /// <summary>
        /// Updates an existing booking
        /// </summary>
        /// <param name="bookingId">Booking ID to update</param>
        /// <param name="updateDto">Updated booking data</param>
        /// <param name="userId">User ID performing the update</param>
        /// <param name="userRole">Role of the user performing the update</param>
        /// <returns>Updated booking DTO</returns>
        Task<BookingDto> UpdateBookingAsync(string bookingId, UpdateBookingDto updateDto, string userId, UserRole userRole);

        /// <summary>
        /// Cancels a booking
        /// </summary>
        /// <param name="bookingId">Booking ID to cancel</param>
        /// <param name="cancellationReason">Reason for cancellation</param>
        /// <param name="userId">User ID performing the cancellation</param>
        /// <param name="userRole">Role of the user performing the cancellation</param>
        /// <returns>Updated booking DTO</returns>
        Task<BookingDto> CancelBookingAsync(string bookingId, string? cancellationReason, string userId, UserRole userRole);

        /// <summary>
        /// Gets available slots for a station within a time range
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="startTime">Start time for availability check</param>
        /// <param name="endTime">End time for availability check</param>
        /// <returns>List of available slots</returns>
        Task<List<Slot>> GetAvailableSlotsAsync(string stationId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Validates if a booking can be created within business rules
        /// </summary>
        /// <param name="createDto">Booking creation data</param>
        /// <returns>Validation result with error message if invalid</returns>
        Task<(bool IsValid, string? ErrorMessage)> ValidateBookingCreationAsync(CreateBookingDto createDto);

        /// <summary>
        /// Validates if a booking can be updated within business rules
        /// </summary>
        /// <param name="bookingId">Booking ID to update</param>
        /// <param name="updateDto">Updated booking data</param>
        /// <returns>Validation result with error message if invalid</returns>
        Task<(bool IsValid, string? ErrorMessage)> ValidateBookingUpdateAsync(string bookingId, UpdateBookingDto updateDto);

        /// <summary>
        /// Validates if a booking can be cancelled within business rules
        /// </summary>
        /// <param name="bookingId">Booking ID to cancel</param>
        /// <returns>Validation result with error message if invalid</returns>
        Task<(bool IsValid, string? ErrorMessage)> ValidateBookingCancellationAsync(string bookingId);

        /// <summary>
        /// Validates a QR code and returns booking details if valid
        /// </summary>
        /// <param name="qrData">QR code validation data</param>
        /// <returns>Validation result with booking details</returns>
        Task<QRValidationResponseDto> ValidateQRCodeAsync(QRValidationRequestDto qrData);

        /// <summary>
        /// Debug method to get current station and slot IDs
        /// </summary>
        /// <returns>Current station and slot IDs for testing</returns>
        Task<object> GetStationsForDebugAsync();
    }
}
