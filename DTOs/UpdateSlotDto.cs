/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 10, 2025
 * File Name: UpdateSlotDto.cs
 * 
 * This file contains the UpdateSlotDto class, which is used for data transfer when updating
 * the details of an existing charging slot. It defines the structure and validation rules
 * for updating connector type, power rating, price per kWh, and slot status.
 */

using EVOwnerManagement.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    public class UpdateSlotDto
    {
        // SlotCode is excluded because user is NOT allowed to update it
        [Required]
        public ConnectorType ConnectorType { get; set; }

        [Required]
        public PowerRating PowerRating { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per kWh must be positive.")]
        public double PricePerKWh { get; set; }

        [Required]
        public SlotStatus SlotStatus { get; set; }
    }
}
