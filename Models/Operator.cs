using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EVOwnerManagement.API.Models
{
    public class Operator
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId StationId { get; set; }  // Reference to station

        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }  // store hashed password, not plain text

        //  Timestamps
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
