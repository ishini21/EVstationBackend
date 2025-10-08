using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EVOwnerManagement.API.Models
{
    public class Slot
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId StationId { get; set; }   // Reference to station

        public string SlotCode { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ConnectorType ConnectorType { get; set; }  // Enum type stored as string

        [BsonRepresentation(BsonType.String)]
        public PowerRating PowerRating { get; set; }      // Enum stored as string (safe)
        public double PricePerKWh { get; set; }

        [BsonRepresentation(BsonType.String)]
        public SlotStatus SlotStatus { get; set; }  // Available, Occupied, Maintenance

        //  Timestamps
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    //  Connector types
    public enum ConnectorType
    {
        CHAdeMO_CCS2_DualPort,
        CCS2_SinglePort,
        CHAdeMO_SinglePort,
        Type2
    }

    //  Slot status options
    public enum SlotStatus
    {
        Available,
        Occupied,
        Maintenance
    }

    //  Power rating options
    public enum PowerRating
    {
        kW22 = 22,
        kW30 = 30,
        kW50 = 50,
        kW100 = 100
    }
}
