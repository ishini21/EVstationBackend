/* 
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M 
 * Created On : October 7, 2025
 * File Name: StationsController.cs
 * 
 * This file contains the StationsController for handling HTTP requests related to station management.
 * It provides RESTful API endpoints for creating, retrieving, updating, and deleting stations,
 * as well as managing associated slots and station operators.
 */

using BCrypt.Net; // for password hashing
using EVOwnerManagement.API.Data;   // for MongoDbContext
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;
using EVOwnerManagement.API.Utils; // for SlotValidator
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EVOwnerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IMongoCollection<Station> _stations;
        private readonly IMongoCollection<Slot> _slots;
        private readonly IMongoCollection<User> _users;

        public StationsController(MongoDbContext context)
        {
            _stations = context.Stations;
            _slots = context.Slots;
            _users= context.Users;
        }

        //  POST - Create a new station
        [HttpPost]
        public async Task<IActionResult> CreateStation([FromBody] CreateStationDto dto)
        {
            // Validate Operators
            if (dto.OperatorIds == null || dto.OperatorIds.Count < 1)
                return BadRequest("At least one operator must be selected for this station.");

            // Validate Slot Groups
            if (dto.SlotGroups == null || dto.SlotGroups.Count < 1)
                return BadRequest("At least one slot group must be defined.");

            // Validate total slots
            int totalSlots = dto.SlotGroups.Sum(g => g.Count);
            if (totalSlots != dto.NoOfSlots)
                return BadRequest($"Slot groups total ({totalSlots}) does not match noOfSlots ({dto.NoOfSlots}).");

            // Validate power–connector combination
            foreach (var group in dto.SlotGroups)
            {
                if (!SlotValidator.IsValidPowerCombination(group.ConnectorType, group.PowerRating))
                {
                    return BadRequest(new
                    {
                        message = $"Invalid power rating {group.PowerRating} for connector type {group.ConnectorType}."
                    });
                }
            }

            // Validate operator IDs exist and are active station operators
            var validOperators = await _users
                .Find(u => dto.OperatorIds.Contains(u.Id) &&
                           u.Role == UserRole.StationOperator &&
                           u.Status == UserStatus.Active)
                .ToListAsync();

            if (validOperators.Count != dto.OperatorIds.Count)
                return BadRequest("Some operator IDs are invalid or not active station operators.");


            // Create the station document
            var station = new Station
            {
                Id = ObjectId.GenerateNewId(),
                StationName = dto.StationName,
                StationCode = dto.StationCode,
                Location = dto.Location,
                StationType = dto.StationType,
                NoOfSlots = dto.NoOfSlots,
                PhoneNumber = dto.PhoneNumber,
                OperatingHours = dto.OperatingHours,
                Status = dto.Status,
                OperatorIds = dto.OperatorIds,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _stations.InsertOneAsync(station);

            // Generate slot documents
            int slotCounter = 1;
            var slots = new List<Slot>();

            foreach (var group in dto.SlotGroups)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    var slot = new Slot
                    {
                        Id = ObjectId.GenerateNewId(),
                        StationId = station.Id,
                        SlotCode = $"{station.StationCode}-S{slotCounter}",
                        ConnectorType = group.ConnectorType,
                        PowerRating = group.PowerRating,
                        PricePerKWh = group.PricePerKWh,
                        SlotStatus = SlotStatus.Available
                    };

                    slots.Add(slot);
                    slotCounter++;
                }
            }

            await _slots.InsertManyAsync(slots);


            return Ok(new
            {
                message = "Station created successfully",
                stationId = station.Id.ToString(),
                slotsCreated = slots.Count,
                operatorCount = validOperators.Count
            });
        }

        //  GET - Get all stations
        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            var stations = await _stations.Find(_ => true).ToListAsync();

            var result = stations.Select(s => new
            {
                id = s.Id.ToString(),
                s.StationName,
                s.StationCode,
                s.StationType,
                s.Status,
                s.NoOfSlots,
                s.PhoneNumber,
                Location = new { s.Location.Latitude, s.Location.Longitude, s.Location.Address },
                s.OperatingHours
            });

            return Ok(result);
        }

        //  GET - Get a station by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid station ID format.");

            var station = await _stations.Find(s => s.Id == objectId).FirstOrDefaultAsync();
            if (station == null)
                return NotFound("Station not found.");

            var slots = await _slots.Find(sl => sl.StationId == objectId).ToListAsync();

            // Fetch full operator details
            var operatorDetails = await _users
                .Find(u => station.OperatorIds.Contains(u.Id))
                .Project(u => new
                {
                    id = u.Id,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    u.Status,
                    u.ProfileImage
                })
                .ToListAsync();

            var response = new
            {
                id = station.Id.ToString(),
                station.StationName,
                station.StationCode,
                station.StationType,
                station.NoOfSlots,
                station.Status,
                station.PhoneNumber,
                Location = new { station.Location.Latitude, station.Location.Longitude, station.Location.Address },
                station.OperatingHours,
                Operators = operatorDetails,
                Slots = slots.Select(sl => new
                {
                    id = sl.Id.ToString(),
                    sl.SlotCode,
                    ConnectorType = sl.ConnectorType.ToString(),
                    sl.PowerRating,
                    sl.PricePerKWh,
                    Status = sl.SlotStatus.ToString()
                })
            };

            return Ok(response);
        }

        //  DELETE - Delete a station by ID (only if no occupied slots)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStation(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId stationId))
                return BadRequest("Invalid station ID format.");

            // Check if station exists
            var station = await _stations.Find(s => s.Id == stationId).FirstOrDefaultAsync();
            if (station == null)
                return NotFound("Station not found.");

            // Check for occupied slots
            var occupiedCount = await _slots.CountDocumentsAsync(sl => sl.StationId == stationId && sl.SlotStatus == SlotStatus.Occupied);
            if (occupiedCount > 0)
            {
                return BadRequest(new
                {
                    message = "Cannot delete station. One or more slots are currently occupied with bookings."
                });
            }

            // Delete all slots, operators, then station
            await _slots.DeleteManyAsync(sl => sl.StationId == stationId);
            //await _operators.DeleteManyAsync(op => op.StationId == stationId);
            await _stations.DeleteOneAsync(s => s.Id == stationId);

            return Ok(new
            {
                message = "Station and all associated slots deleted successfully."
            });
        }

        //  GET - Get all Station Operators
        [HttpGet("station-operators")]
        public async Task<IActionResult> GetStationOperators()
        {
            // Fetch only active Station Operators
            var operators = await _users
                .Find(u => u.Role == UserRole.StationOperator && u.Status == UserStatus.Active)
                .ToListAsync();

            // Project directly to return separate names
            var result = operators.Select(u => new
            {
                id = u.Id,
                firstName = u.FirstName,
                lastName = u.LastName,
                u.Email,
                u.PhoneNumber,
                u.Address,
                u.Status,
                u.CreatedAt,
                u.LastLogin,
                u.ProfileImage
            });

            return Ok(result);
        }


        // PUT - Update a station by ID (only selected fields)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStation(string id, [FromBody] UpdateStationDto dto)
        {
            if (!ObjectId.TryParse(id, out ObjectId stationId))
                return BadRequest("Invalid station ID format.");

            var existingStation = await _stations.Find(s => s.Id == stationId).FirstOrDefaultAsync();
            if (existingStation == null)
                return NotFound("Station not found.");

            var update = Builders<Station>.Update
                .Set(s => s.StationName, dto.StationName)
                .Set(s => s.Location, new Location
                {
                    Latitude = dto.Location.Latitude,
                    Longitude = dto.Location.Longitude,
                    Address = dto.Location.Address
                })
                .Set(s => s.StationType, dto.StationType)
                .Set(s => s.PhoneNumber, dto.PhoneNumber)
                .Set(s => s.OperatingHours, dto.OperatingHours)
                .Set(s => s.Status, dto.Status)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);

            var result = await _stations.UpdateOneAsync(s => s.Id == stationId, update);

            if (result.ModifiedCount == 0)
                return BadRequest("Station update failed or no changes detected.");

            return Ok(new
            {
                message = "Station updated successfully.",
                updatedAt = DateTime.UtcNow
            });
        }


    }
}
