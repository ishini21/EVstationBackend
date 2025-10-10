/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 9, 2025
 * File Name: UpdateStationDto.cs
 * 
 * This file contains the UpdateStationDto class used for data transfer
 * when updating an existing EV station. It defines the structure and validation
 * rules for station update requests, allowing modification of selected station fields.
 */

using EVOwnerManagement.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    public class UpdateStationDto
    {
        [Required]
        public string StationName { get; set; }

        [Required]
        public Location Location { get; set; }

        [Required]
        public string StationType { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public OperatingHours OperatingHours { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
