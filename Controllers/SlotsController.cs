/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 10, 2025
 * File Name: SlotsController.cs
 * 
 * This file contains the SlotsController for handling HTTP requests related to slot management.
 * It provides RESTful API endpoints for creating, updating, and managing the availability
 * of charging slots within EV stations.
 */

using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;
using EVOwnerManagement.API.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EVOwnerManagement.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SlotsController : ControllerBase
	{
		private readonly IMongoCollection<Slot> _slots;
		private readonly IMongoCollection<Station> _stations;

		public SlotsController(MongoDbContext context)
		{
			_slots = context.Slots;
			_stations = context.Stations;
		}

		// POST - Create a new slot for a specific station
		[HttpPost("{stationId}")]
		public async Task<IActionResult> CreateSlot(string stationId, [FromBody] CreateSlotDto dto)
		{
			if (!ObjectId.TryParse(stationId, out ObjectId stationObjectId))
				return BadRequest("Invalid station ID format.");

			// Check station exists
			var station = await _stations.Find(s => s.Id == stationObjectId).FirstOrDefaultAsync();
			if (station == null)
				return NotFound("Station not found.");

			// Validate power–connector combination
			if (!SlotValidator.IsValidPowerCombination(dto.ConnectorType, dto.PowerRating))
			{
				return BadRequest(new
				{
					message = $"Invalid power rating {dto.PowerRating} for connector type {dto.ConnectorType}."
				});
			}

			// Generate slot code
			var slotCount = await _slots.CountDocumentsAsync(s => s.StationId == stationObjectId);
			var slotCode = $"{station.StationCode}-S{slotCount + 1}";

			// Create slot document
			var newSlot = new Slot
			{
				Id = ObjectId.GenerateNewId(),
				StationId = stationObjectId,
				SlotCode = slotCode,
				ConnectorType = dto.ConnectorType,
				PowerRating = dto.PowerRating,
				PricePerKWh = dto.PricePerKWh,
				SlotStatus = dto.SlotStatus,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _slots.InsertOneAsync(newSlot);

			return Ok(new
			{
				message = "Slot created successfully.",
				slotId = newSlot.Id.ToString(),
				slotCode = newSlot.SlotCode
			});
		}

		// PUT - Update slot details by ID (cannot update SlotCode or StationId)
		[HttpPut("{slotId}")]
		public async Task<IActionResult> UpdateSlot(string slotId, [FromBody] UpdateSlotDto dto)
		{
			if (!ObjectId.TryParse(slotId, out ObjectId slotObjectId))
				return BadRequest("Invalid slot ID format.");

			var existingSlot = await _slots.Find(s => s.Id == slotObjectId).FirstOrDefaultAsync();
			if (existingSlot == null)
				return NotFound("Slot not found.");

			// Validate power–connector combination
			if (!SlotValidator.IsValidPowerCombination(dto.ConnectorType, dto.PowerRating))
			{
				return BadRequest(new
				{
					message = $"Invalid power rating {dto.PowerRating} for connector type {dto.ConnectorType}."
				});
			}

			var updateDef = Builders<Slot>.Update
				.Set(s => s.ConnectorType, dto.ConnectorType)
				.Set(s => s.PowerRating, dto.PowerRating)
				.Set(s => s.PricePerKWh, dto.PricePerKWh)
				.Set(s => s.SlotStatus, dto.SlotStatus)
				.Set(s => s.UpdatedAt, DateTime.UtcNow);

			await _slots.UpdateOneAsync(s => s.Id == slotObjectId, updateDef);

			return Ok(new { message = "Slot updated successfully." });
		}

		// PATCH - Update slot availability (status only)
		[HttpPatch("{slotId}/availability")]
		public async Task<IActionResult> UpdateSlotAvailability(string slotId, [FromBody] UpdateSlotAvailabilityDto dto)
		{
			if (!ObjectId.TryParse(slotId, out ObjectId slotObjectId))
				return BadRequest("Invalid slot ID format.");

			var existingSlot = await _slots.Find(s => s.Id == slotObjectId).FirstOrDefaultAsync();
			if (existingSlot == null)
				return NotFound("Slot not found.");

			var updateDef = Builders<Slot>.Update
				.Set(s => s.SlotStatus, dto.SlotStatus)
				.Set(s => s.UpdatedAt, DateTime.UtcNow);

			await _slots.UpdateOneAsync(s => s.Id == slotObjectId, updateDef);

			return Ok(new { message = $"Slot status updated to {dto.SlotStatus}." });
		}
	}
}
