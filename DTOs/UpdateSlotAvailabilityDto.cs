/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 7, 2025
 * File Name: UpdateSlotAvailabilityDto.cs
 * 
 * This file contains the UpdateSlotAvailabilityDto class, which is used for data transfer
 * when updating the availability status of a charging slot. It defines the structure and
 * validation rules for modifying the slot's status (e.g., Available, Occupied, Maintenance).
 */

using EVOwnerManagement.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    public class UpdateSlotAvailabilityDto
    {
        [Required]
        public SlotStatus SlotStatus { get; set; }
    }
}
