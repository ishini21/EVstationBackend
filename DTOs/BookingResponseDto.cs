/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the BookingResponseDto for paginated booking list responses.
 * It provides a structured response with pagination metadata.
 */

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for paginated booking list responses
    /// </summary>
    public class BookingResponseDto
    {
        public List<BookingDto> Bookings { get; set; } = new List<BookingDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
