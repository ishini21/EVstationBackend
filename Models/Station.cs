/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 7, 2025
 * File Name: Station.cs
 * 
 * This file contains the Station class, which represents an EV charging station entity.
 * It defines properties for station details, location, operating hours, status, assigned operators,
 * and timestamps for creation and updates. Used for MongoDB persistence and business logic.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace EVOwnerManagement.API.Models
{
    public class Station
    {
        [BsonId]
        public ObjectId Id { get; set; }
        
        public string StationName { get; set; }
        public string StationCode { get; set; }
        public Location Location { get; set; }
        public string StationType { get; set; }
        public int NoOfSlots { get; set; }
        public string PhoneNumber { get; set; }
        public OperatingHours OperatingHours { get; set; }
        public string Status { get; set; }

        // list of assigned operator IDs (references to User collection)
        [BsonElement("operatorIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> OperatorIds { get; set; } = new List<string>();

        // Timestamps
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
