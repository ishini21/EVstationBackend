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
