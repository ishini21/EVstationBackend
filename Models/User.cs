/************************************************************************************************
* Filename:         User.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file defines the main database model for a system user (Backoffice or StationOperator).
* It maps directly to the 'users' collection in the MongoDB database and uses BSON attributes
* to control serialization.
************************************************************************************************/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVOwnerManagement.API.Models
{
    /// <summary>
    /// Represents a system user in the database.
    /// </summary>
    public class User
    {
        // Property: The unique identifier for the user document, stored as an ObjectId in MongoDB.
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // Property: The user's first name. Maps to the 'firstName' field in MongoDB.
        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        // Property: The user's last name. Maps to the 'lastName' field in MongoDB.
        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        // Property: The user's email address. Maps to the 'email' field in MongoDB.
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        // Property: The user's contact phone number. Maps to the 'phoneNumber' field in MongoDB.
        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Property: The user's physical address (optional). Maps to the 'address' field in MongoDB.
        [BsonElement("address")]
        public string? Address { get; set; }

        // Property: The hashed password for user authentication. Maps to the 'passwordHash' field in MongoDB.
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        // Property: The user's role, stored as a string in MongoDB (e.g., "Backoffice").
        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; } = UserRole.StationOperator;

        // Property: The user's account status, stored as a string in MongoDB (e.g., "Active").
        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public UserStatus Status { get; set; } = UserStatus.Active;

        // Property: The ID of the station the user is assigned to. Null if not a StationOperator or unassigned.
        [BsonElement("stationId")]
        public string? StationId { get; set; }

        // Property: The UTC date and time the user was created. Maps to the 'createdAt' field.
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Property: The UTC date and time the user was last updated (optional). Maps to 'updatedAt'.
        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Property: The URL for the user's profile image (optional). Maps to 'profileImage'.
        [BsonElement("profileImage")]
        public string? ProfileImage { get; set; }

        // Property: The UTC date and time of the user's last login (optional). Maps to 'lastLogin'.
        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }
    }
}
