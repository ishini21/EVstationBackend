/*
 * Course: SE4040 - Software Engineering
 * Assignment: EV Station Management System - Booking Management
 * Student: [IT Number]
 * Date: [Current Date]
 * 
 * This file contains the BookingQueryDto for filtering and paginating booking queries.
 * It provides query parameters for the booking list endpoint.
 */

using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for querying bookings with filters and pagination
    /// </summary>
    public class BookingQueryDto
    {
        public string? StationId { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNic { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }
}
