/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the BookingsController for handling HTTP requests related to booking management.
 * It provides RESTful API endpoints for CRUD operations on bookings with proper authorization.
 */

using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;
using EVOwnerManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVOwnerManagement.API.Controllers
{
    /// <summary>
    /// Controller for managing booking reservations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new booking reservation
        /// </summary>
        /// <param name="createDto">Booking creation data</param>
        /// <returns>Created booking information</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto createDto)
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var booking = await _bookingService.CreateBookingAsync(createDto, userId);
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid booking creation attempt");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in booking creation");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { message = "An error occurred while creating the booking" });
            }
        }

        /// <summary>
        /// Gets a booking by its ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking information</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(string id)
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)?.Value ?? "StationOperator");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var booking = await _bookingService.GetBookingByIdAsync(id, userId, userRole);
                if (booking == null)
                {
                    return NotFound("Booking not found");
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the booking" });
            }
        }

        /// <summary>
        /// Gets a paginated list of bookings with optional filters
        /// </summary>
        /// <param name="queryDto">Query parameters and filters</param>
        /// <returns>Paginated list of bookings</returns>
        [HttpGet]
        public async Task<IActionResult> GetBookings([FromQuery] BookingQueryDto queryDto)
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)?.Value ?? "StationOperator");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var result = await _bookingService.GetBookingsAsync(queryDto, userId, userRole);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings");
                return StatusCode(500, new { message = "An error occurred while retrieving bookings" });
            }
        }

        /// <summary>
        /// Updates an existing booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="updateDto">Updated booking data</param>
        /// <returns>Updated booking information</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(string id, [FromBody] UpdateBookingDto updateDto)
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)?.Value ?? "StationOperator");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var booking = await _bookingService.UpdateBookingAsync(id, updateDto, userId, userRole);
                return Ok(booking);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid booking update attempt for booking {BookingId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in booking update for booking {BookingId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the booking" });
            }
        }

        /// <summary>
        /// Cancels a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="cancellationDto">Cancellation reason</param>
        /// <returns>Updated booking information</returns>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(string id, [FromBody] CancelBookingDto? cancellationDto = null)
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)?.Value ?? "StationOperator");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var cancellationReason = cancellationDto?.Reason;
                var booking = await _bookingService.CancelBookingAsync(id, cancellationReason, userId, userRole);
                return Ok(booking);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid booking cancellation attempt for booking {BookingId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in booking cancellation for booking {BookingId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while cancelling the booking" });
            }
        }

        /// <summary>
        /// Gets available slots for a station within a time range
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="startTime">Start time for availability check</param>
        /// <param name="endTime">End time for availability check</param>
        /// <returns>List of available slots</returns>
        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] string stationId, [FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)?.Value ?? "StationOperator");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var slots = await _bookingService.GetAvailableSlotsAsync(stationId, startTime, endTime);

                // Map to response objects with string IDs to avoid ObjectId JSON shape on the client
                var response = slots.Select(sl => new
                {
                    id = sl.Id.ToString(),
                    stationId = sl.StationId.ToString(),
                    slotCode = sl.SlotCode,
                    connectorType = sl.ConnectorType.ToString(),
                    powerRating = (int)sl.PowerRating,
                    pricePerKWh = sl.PricePerKWh,
                    slotStatus = sl.SlotStatus.ToString(),
                    createdAt = sl.CreatedAt,
                    updatedAt = sl.UpdatedAt
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available slots for station {StationId}", stationId);
                return StatusCode(500, new { message = "An error occurred while retrieving available slots" });
            }
        }

        /// <summary>
        /// Validates if a booking can be created
        /// </summary>
        /// <param name="createDto">Booking creation data</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateBooking([FromBody] CreateBookingDto createDto)
        {
            try
            {
                var validation = await _bookingService.ValidateBookingCreationAsync(createDto);
                return Ok(new { isValid = validation.IsValid, errorMessage = validation.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating booking");
                return StatusCode(500, new { message = "An error occurred while validating the booking" });
            }
        }

        /// <summary>
        /// Validates a QR code and returns booking details if valid
        /// </summary>
        /// <param name="qrData">QR code validation data</param>
        /// <returns>Validation result with booking details</returns>
        [HttpPost("validateQR")]
        public async Task<IActionResult> ValidateQRCode([FromBody] QRValidationRequestDto qrData)
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)?.Value ?? "StationOperator");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                // Only station operators can validate QR codes
                if (userRole != UserRole.StationOperator)
                {
                    return Forbid("Only station operators can validate QR codes");
                }

                var result = await _bookingService.ValidateQRCodeAsync(qrData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating QR code");
                return StatusCode(500, new { message = "An error occurred while validating the QR code" });
            }
        }

        /// <summary>
        /// Debug endpoint to get current station and slot IDs
        /// </summary>
        /// <returns>Current station and slot IDs for testing</returns>
        [HttpGet("debug/ids")]
        public async Task<IActionResult> GetDebugIds()
        {
            try
            {
                var stations = await _bookingService.GetStationsForDebugAsync();
                return Ok(stations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting debug IDs");
                return StatusCode(500, new { message = "An error occurred while getting debug IDs" });
            }
        }
    }

    /// <summary>
    /// Data Transfer Object for booking cancellation
    /// </summary>
    public class CancelBookingDto
    {
        public string? Reason { get; set; }
    }
}
