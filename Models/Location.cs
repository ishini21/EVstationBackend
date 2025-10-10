/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 7, 2025
 * File Name: Location.cs
 * 
 * This file contains the Location class, which represents the geographical location of an EV station.
 * It includes latitude, longitude, and address fields for mapping and display purposes.
 */

namespace EVOwnerManagement.API.Models
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }
}
