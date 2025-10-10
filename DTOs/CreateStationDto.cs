/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 7, 2025
 * File Name: CreateStationDto.cs
 * 
 * This file contains the CreateStationDto and SlotGroupDto classes used for data transfer
 * when creating a new EV station. It defines the structure and validation rules for
 * station creation requests, including station details, slot groups, and operator assignments.
 */

using EVOwnerManagement.API.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    public class CreateStationDto
    {
        [Required]
        public string StationName { get; set; }

        [Required]
        public string StationCode { get; set; }

        [Required]
        public Location Location { get; set; }

        [Required]
        public string StationType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of slots must be at least 1.")]
        public int NoOfSlots { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public OperatingHours OperatingHours { get; set; }

        [Required]
        public string Status { get; set; }

        //  Slot groups (for creating slots automatically)
        [Required]
        public List<SlotGroupDto> SlotGroups { get; set; }

        //  Select existing operators (by their IDs)
        [Required]
        [MinLength(1, ErrorMessage = "At least one operator must be selected for this station.")]
        public List<string> OperatorIds { get; set; }
    }

    //  Used for grouped slot input
    public class SlotGroupDto
    {
        [Required]
        public ConnectorType ConnectorType { get; set; }

        [Required]
        public PowerRating PowerRating { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per kWh must be positive.")]
        public double PricePerKWh { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Slot count must be at least 1.")]
        public int Count { get; set; }
    }

}
