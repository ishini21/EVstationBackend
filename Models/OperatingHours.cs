/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 7, 2025
 * File Name: OperatingHours.cs
 * 
 * This file contains the OperatingHours class, which specifies the opening and closing times
 * for an EV station, including support for 24-hour operation.
 */

namespace EVOwnerManagement.API.Models
{
    public class OperatingHours
    {
        public string OpenTime { get; set; }  // e.g. "06:00"
        public string CloseTime { get; set; } // e.g. "22:00"
        public bool Is24Hours { get; set; } = false; // 24/7 operation flag
    }
}
