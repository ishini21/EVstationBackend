using BCrypt.Net; // for password hashing
using EVOwnerManagement.API.Data;   // for MongoDbContext
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;
using EVOwnerManagement.API.Utils; // for SlotValidator
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EVOwnerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IMongoCollection<Station> _stations;
        private readonly IMongoCollection<Slot> _slots;
        private readonly IMongoCollection<Operator> _operators;

        public StationsController(MongoDbContext context)
        {
            _stations = context.Stations;
            _slots = context.Slots;
            _operators = context.Operators;
        }

        //  POST - Create a new station
        [HttpPost]
        public async Task<IActionResult> CreateStation([FromBody] CreateStationDto dto)
        {
            // Validate Operators
            if (dto.Operators == null || dto.Operators.Count < 1)
                return BadRequest("At least one operator must be created for this station.");

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

            // Create station object
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

            // Create operator accounts
            var operators = dto.Operators.Select(op => new Operator
            {
                Id = ObjectId.GenerateNewId(),
                StationId = station.Id,
                Name = op.Name,
                Email = op.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(op.Password)
            }).ToList();

            await _operators.InsertManyAsync(operators);

            return Ok(new
            {
                message = "Station created successfully",
                stationId = station.Id.ToString(),
                slotsCreated = slots.Count,
                operatorsCreated = operators.Count
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
            var operators = await _operators.Find(op => op.StationId == objectId).ToListAsync();

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
                Slots = slots.Select(sl => new
                {
                    id = sl.Id.ToString(),
                    sl.SlotCode,
                    ConnectorType = sl.ConnectorType.ToString(),
                    sl.PowerRating,
                    sl.PricePerKWh,
                    Status = sl.SlotStatus.ToString()
                }),
                Operators = operators.Select(op => new
                {
                    id = op.Id.ToString(),
                    op.Name,
                    op.Email
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
            await _operators.DeleteManyAsync(op => op.StationId == stationId);
            await _stations.DeleteOneAsync(s => s.Id == stationId);

            return Ok(new
            {
                message = "Station and all associated slots and operators deleted successfully."
            });
        }
    }
}
