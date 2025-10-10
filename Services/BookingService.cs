/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the BookingService implementation for booking management operations.
 * It implements all business logic, validation rules, and data operations for bookings.
 */

using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Service implementation for booking management operations
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly MongoDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(MongoDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new booking reservation with business rule validation
        /// </summary>
        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto, string createdBy)
        {
            // Validate booking creation
            var validation = await ValidateBookingCreationAsync(createDto);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            // Get station and slot information
            var station = await _context.Stations.Find(s => s.Id == ObjectId.Parse(createDto.StationId)).FirstOrDefaultAsync();
            var slot = await _context.Slots.Find(s => s.Id == ObjectId.Parse(createDto.SlotId)).FirstOrDefaultAsync();

            if (station == null || slot == null)
            {
                throw new ArgumentException("Station or slot not found");
            }

            // Check slot availability
            var isSlotAvailable = await IsSlotAvailableAsync(createDto.SlotId, createDto.ReservationStartTime, createDto.ReservationStartTime.AddMinutes(createDto.DurationMinutes));
            if (!isSlotAvailable)
            {
                throw new InvalidOperationException("Slot is not available for the selected time period");
            }

            // Generate booking number
            var bookingNumber = await GenerateBookingNumberAsync();

            // Calculate end time and total amount
            var endTime = createDto.ReservationStartTime.AddMinutes(createDto.DurationMinutes);
            var totalAmount = (decimal)(createDto.EstimatedKWh * slot.PricePerKWh);

            // Create booking
            var booking = new Booking
            {
                Id = ObjectId.GenerateNewId().ToString(),
                BookingNumber = bookingNumber,
                CustomerNic = createDto.CustomerNic,
                CustomerName = createDto.CustomerName,
                CustomerEmail = createDto.CustomerEmail,
                CustomerPhone = createDto.CustomerPhone,
                StationId = createDto.StationId,
                StationName = station.StationName,
                SlotId = createDto.SlotId,
                SlotCode = slot.SlotCode,
                ReservationStartTime = createDto.ReservationStartTime,
                ReservationEndTime = endTime,
                DurationMinutes = createDto.DurationMinutes,
                Status = BookingStatus.Confirmed,
                TotalAmount = totalAmount,
                PricePerKWh = slot.PricePerKWh,
                EstimatedKWh = createDto.EstimatedKWh,
                QrCode = GenerateQrCode(bookingNumber),
                Notes = createDto.Notes,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            // Start transaction
            using var session = await _context.Bookings.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // Insert booking
                await _context.Bookings.InsertOneAsync(session, booking);

                // Note: We don't update slot status to "Occupied" because slots can be booked
                // for different time periods. Availability is checked based on time conflicts.

                await session.CommitTransactionAsync();

                _logger.LogInformation("Booking {BookingId} created successfully for customer {CustomerName}", booking.Id, booking.CustomerName);

                return MapToDto(booking);
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Failed to create booking for customer {CustomerName}", createDto.CustomerName);
                throw;
            }
        }

        /// <summary>
        /// Gets a booking by its ID with role-based access control
        /// </summary>
        public async Task<BookingDto?> GetBookingByIdAsync(string bookingId, string userId, UserRole userRole)
        {
            if (!ObjectId.TryParse(bookingId, out _))
            {
                return null;
            }

            var booking = await _context.Bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (booking == null)
            {
                return null;
            }

            // Check access permissions
            if (userRole == UserRole.StationOperator)
            {
                // Station operators can only see bookings for their station
                var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return null;
                }

                // For now, we'll assume station operators are associated with stations through a separate mechanism
                // This would need to be implemented based on your user-station relationship model
            }

            return MapToDto(booking);
        }

        /// <summary>
        /// Gets a paginated list of bookings with filters and role-based access control
        /// </summary>
        public async Task<BookingResponseDto> GetBookingsAsync(BookingQueryDto queryDto, string userId, UserRole userRole)
        {
            var filter = Builders<Booking>.Filter.Empty;

            // Apply role-based filtering
            if (userRole == UserRole.StationOperator)
            {
                // Station operators can only see bookings for their assigned station
                // This would need to be implemented based on your user-station relationship model
                // For now, we'll apply no additional filtering
            }

            // Apply filters
            if (!string.IsNullOrEmpty(queryDto.StationId))
            {
                filter &= Builders<Booking>.Filter.Eq(b => b.StationId, queryDto.StationId);
            }

            if (queryDto.Status.HasValue)
            {
                filter &= Builders<Booking>.Filter.Eq(b => b.Status, queryDto.Status.Value);
            }

            if (queryDto.StartDate.HasValue)
            {
                filter &= Builders<Booking>.Filter.Gte(b => b.ReservationStartTime, queryDto.StartDate.Value);
            }

            if (queryDto.EndDate.HasValue)
            {
                filter &= Builders<Booking>.Filter.Lte(b => b.ReservationStartTime, queryDto.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(queryDto.CustomerName))
            {
                filter &= Builders<Booking>.Filter.Regex(b => b.CustomerName, new BsonRegularExpression(queryDto.CustomerName, "i"));
            }

            if (!string.IsNullOrEmpty(queryDto.CustomerNic))
            {
                filter &= Builders<Booking>.Filter.Eq(b => b.CustomerNic, queryDto.CustomerNic);
            }

            // Get total count
            var totalCount = await _context.Bookings.CountDocumentsAsync(filter);

            // Apply sorting
            var sortDefinition = queryDto.SortOrder?.ToLower() == "asc"
                ? Builders<Booking>.Sort.Ascending(queryDto.SortBy)
                : Builders<Booking>.Sort.Descending(queryDto.SortBy);

            // Apply pagination
            var skip = (queryDto.Page - 1) * queryDto.PageSize;
            var bookings = await _context.Bookings
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(skip)
                .Limit(queryDto.PageSize)
                .ToListAsync();

            var bookingDtos = bookings.Select(MapToDto).ToList();

            return new BookingResponseDto
            {
                Bookings = bookingDtos,
                TotalCount = (int)totalCount,
                Page = queryDto.Page,
                PageSize = queryDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / queryDto.PageSize),
                HasNextPage = queryDto.Page < Math.Ceiling((double)totalCount / queryDto.PageSize),
                HasPreviousPage = queryDto.Page > 1
            };
        }

        /// <summary>
        /// Updates an existing booking with business rule validation
        /// </summary>
        public async Task<BookingDto> UpdateBookingAsync(string bookingId, UpdateBookingDto updateDto, string userId, UserRole userRole)
        {
            // Validate booking update
            var validation = await ValidateBookingUpdateAsync(bookingId, updateDto);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            var booking = await _context.Bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (booking == null)
            {
                throw new ArgumentException("Booking not found");
            }

            // Check access permissions
            if (userRole == UserRole.StationOperator)
            {
                // Station operators can only update bookings for their station
                // This would need to be implemented based on your user-station relationship model
            }

            // Calculate new end time and total amount
            var newEndTime = updateDto.ReservationStartTime.AddMinutes(updateDto.DurationMinutes);
            var slot = await _context.Slots.Find(s => s.Id == ObjectId.Parse(booking.SlotId)).FirstOrDefaultAsync();
            var newTotalAmount = (decimal)(updateDto.EstimatedKWh * (slot?.PricePerKWh ?? 0));

            // Update booking
            var filter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId);
            var update = Builders<Booking>.Update
                .Set(b => b.CustomerNic, updateDto.CustomerNic)
                .Set(b => b.CustomerName, updateDto.CustomerName)
                .Set(b => b.CustomerEmail, updateDto.CustomerEmail)
                .Set(b => b.CustomerPhone, updateDto.CustomerPhone)
                .Set(b => b.ReservationStartTime, updateDto.ReservationStartTime)
                .Set(b => b.ReservationEndTime, newEndTime)
                .Set(b => b.DurationMinutes, updateDto.DurationMinutes)
                .Set(b => b.EstimatedKWh, updateDto.EstimatedKWh)
                .Set(b => b.TotalAmount, newTotalAmount)
                .Set(b => b.Notes, updateDto.Notes)
                .Set(b => b.UpdatedAt, DateTime.UtcNow);

            await _context.Bookings.UpdateOneAsync(filter, update);

            _logger.LogInformation("Booking {BookingId} updated successfully", bookingId);

            // Return updated booking
            var updatedBooking = await _context.Bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            return MapToDto(updatedBooking!);
        }

        /// <summary>
        /// Cancels a booking with business rule validation
        /// </summary>
        public async Task<BookingDto> CancelBookingAsync(string bookingId, string? cancellationReason, string userId, UserRole userRole)
        {
            // Validate booking cancellation
            var validation = await ValidateBookingCancellationAsync(bookingId);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            var booking = await _context.Bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (booking == null)
            {
                throw new ArgumentException("Booking not found");
            }

            // Check access permissions
            if (userRole == UserRole.StationOperator)
            {
                // Station operators can only cancel bookings for their station
                // This would need to be implemented based on your user-station relationship model
            }

            // Start transaction
            using var session = await _context.Bookings.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // Update booking status
                var bookingFilter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId);
                var bookingUpdate = Builders<Booking>.Update
                    .Set(b => b.Status, BookingStatus.Cancelled)
                    .Set(b => b.CancelledAt, DateTime.UtcNow)
                    .Set(b => b.CancellationReason, cancellationReason)
                    .Set(b => b.UpdatedAt, DateTime.UtcNow);
                await _context.Bookings.UpdateOneAsync(session, bookingFilter, bookingUpdate);

                // Note: We don't update slot status because availability is checked based on time conflicts
                // The cancelled booking will be excluded from availability checks automatically

                await session.CommitTransactionAsync();

                _logger.LogInformation("Booking {BookingId} cancelled successfully", bookingId);

                // Return updated booking
                var cancelledBooking = await _context.Bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
                return MapToDto(cancelledBooking!);
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Failed to cancel booking {BookingId}", bookingId);
                throw;
            }
        }

        /// <summary>
        /// Gets available slots for a station within a time range
        /// </summary>
        public async Task<List<Slot>> GetAvailableSlotsAsync(string stationId, DateTime startTime, DateTime endTime)
        {
            // Get all slots for the station
            var slots = await _context.Slots.Find(s => s.StationId == ObjectId.Parse(stationId)).ToListAsync();

            // Filter out slots that are occupied during the time range
            var occupiedSlots = await _context.Bookings.Find(b =>
                b.StationId == stationId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Expired &&
                ((b.ReservationStartTime < endTime && b.ReservationEndTime > startTime))
            ).ToListAsync();

            var occupiedSlotIds = occupiedSlots.Select(b => b.SlotId).ToHashSet();
            return slots.Where(s => !occupiedSlotIds.Contains(s.Id.ToString())).ToList();
        }

        /// <summary>
        /// Validates if a booking can be created within business rules
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> ValidateBookingCreationAsync(CreateBookingDto createDto)
        {
            // Check if reservation is within 7 days
            var maxReservationDate = DateTime.UtcNow.AddDays(7);
            if (createDto.ReservationStartTime > maxReservationDate)
            {
                return (false, "Reservation must be within 7 days from today");
            }

            // Check if reservation is in the future
            if (createDto.ReservationStartTime <= DateTime.UtcNow)
            {
                return (false, "Reservation must be in the future");
            }

            // Check if slot exists and is available
            var slot = await _context.Slots.Find(s => s.Id == ObjectId.Parse(createDto.SlotId)).FirstOrDefaultAsync();
            if (slot == null)
            {
                return (false, "Slot not found");
            }

            // Check if slot is available for the time period
            // Note: We don't check slot.SlotStatus because slots can be booked for different time periods
            var isAvailable = await IsSlotAvailableAsync(createDto.SlotId, createDto.ReservationStartTime, createDto.ReservationStartTime.AddMinutes(createDto.DurationMinutes));
            if (!isAvailable)
            {
                return (false, "Slot is not available for the selected time period");
            }

            return (true, null);
        }

        /// <summary>
        /// Validates if a booking can be updated within business rules
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> ValidateBookingUpdateAsync(string bookingId, UpdateBookingDto updateDto)
        {
            var booking = await _context.Bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (booking == null)
            {
                return (false, "Booking not found");
            }

            // Check if booking can be updated (at least 12 hours before start time)
            var minUpdateTime = booking.ReservationStartTime.AddHours(-12);
            if (DateTime.UtcNow > minUpdateTime)
            {
                return (false, "Booking can only be updated at least 12 hours before the reservation start time");
            }

            // Check if new reservation time is within 7 days
            var maxReservationDate = DateTime.UtcNow.AddDays(7);
            if (updateDto.ReservationStartTime > maxReservationDate)
            {
                return (false, "Reservation must be within 7 days from today");
            }

            // Check if new reservation time is in the future
            if (updateDto.ReservationStartTime <= DateTime.UtcNow)
            {
                return (false, "Reservation must be in the future");
            }

            return (true, null);
        }

        /// <summary>
        /// Validates if a booking can be cancelled within business rules
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> ValidateBookingCancellationAsync(string bookingId)
        {
            var booking = await _context.Bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (booking == null)
            {
                return (false, "Booking not found");
            }

            // Check if booking is already cancelled
            if (booking.Status == BookingStatus.Cancelled)
            {
                return (false, "Booking is already cancelled");
            }

            // Check if booking can be cancelled (at least 12 hours before start time)
            var minCancellationTime = booking.ReservationStartTime.AddHours(-12);
            if (DateTime.UtcNow > minCancellationTime)
            {
                return (false, "Booking can only be cancelled at least 12 hours before the reservation start time");
            }

            return (true, null);
        }

        /// <summary>
        /// Checks if a slot is available for a specific time period
        /// </summary>
        private async Task<bool> IsSlotAvailableAsync(string slotId, DateTime startTime, DateTime endTime)
        {
            var conflictingBookings = await _context.Bookings.CountDocumentsAsync(b =>
                b.SlotId == slotId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Expired &&
                ((b.ReservationStartTime < endTime && b.ReservationEndTime > startTime))
            );

            return conflictingBookings == 0;
        }

        /// <summary>
        /// Generates a unique booking number
        /// </summary>
        private async Task<string> GenerateBookingNumberAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.Bookings.CountDocumentsAsync(b => b.BookingNumber.StartsWith($"BK{today}"));
            return $"BK{today}{(count + 1):D4}";
        }

        /// <summary>
        /// Generates a QR code for the booking
        /// </summary>
        private string GenerateQrCode(string bookingNumber)
        {
            // Simple QR code generation - in production, use a proper QR code library
            return $"QR_{bookingNumber}_{DateTime.UtcNow.Ticks}";
        }

        /// <summary>
        /// Debug method to get current station and slot IDs
        /// </summary>
        public async Task<object> GetStationsForDebugAsync()
        {
            var stations = await _context.Stations.Find(_ => true).ToListAsync();
            var result = stations.Select(s => new
            {
                stationId = s.Id.ToString(),
                stationName = s.StationName,
                slots = _context.Slots.Find(sl => sl.StationId == s.Id).ToListAsync().Result.Select(sl => new
                {
                    slotId = sl.Id.ToString(),
                    slotCode = sl.SlotCode,
                    status = sl.SlotStatus.ToString()
                }).ToList()
            }).ToList();

            return new { stations = result };
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
