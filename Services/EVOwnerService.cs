using MongoDB.Driver;
using MongoDB.Driver.Linq;
using EVOwnerManagement.API.Models;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Data;

namespace EVOwnerManagement.API.Services
{
    public class EVOwnerService : IEVOwnerService
    {
        private readonly MongoDbContext _context;

        public EVOwnerService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<EVOwnerDto>> GetAllAsync()
        {
            var owners = await _context.EVOwners
                .Find(_ => true)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();

            return owners.Select(MapToDto).ToList();
        }

        public async Task<EVOwnerDto?> GetByNICAsync(string nic)
        {
            var owner = await _context.EVOwners
                .Find(o => o.NIC == nic)
                .FirstOrDefaultAsync();

            return owner == null ? null : MapToDto(owner);
        }

        public async Task<EVOwnerDto?> GetByIdAsync(string id)
        {
            var owner = await _context.EVOwners
                .Find(o => o.Id == id)
                .FirstOrDefaultAsync();

            return owner == null ? null : MapToDto(owner);
        }

        public async Task<EVOwnerDto> CreateAsync(CreateEVOwnerDto createDto)
        {
            // Check if NIC already exists
            var existingOwner = await GetByNICAsync(createDto.NIC);
            if (existingOwner != null)
            {
                throw new InvalidOperationException($"EV Owner with NIC {createDto.NIC} already exists.");
            }

            var owner = new EVOwner
            {
                NIC = createDto.NIC,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                Phone = createDto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password),
                Role = "EVOwner", // Always set to EVOwner for EV owners
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.EVOwners.InsertOneAsync(owner);
            return MapToDto(owner);
        }

        public async Task<EVOwnerDto?> UpdateAsync(string nic, UpdateEVOwnerDto updateDto)
        {
            var filter = Builders<EVOwner>.Filter.Eq(o => o.NIC, nic);
            var update = Builders<EVOwner>.Update
                .Set(o => o.FirstName, updateDto.FirstName)
                .Set(o => o.LastName, updateDto.LastName)
                .Set(o => o.Email, updateDto.Email)
                .Set(o => o.Phone, updateDto.Phone)
                .Set(o => o.Role, "EVOwner") // Always keep as EVOwner
                .Set(o => o.IsActive, updateDto.IsActive)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            // Only update password if provided
            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                update = update.Set(o => o.PasswordHash, BCrypt.Net.BCrypt.HashPassword(updateDto.Password));
            }

            var options = new FindOneAndUpdateOptions<EVOwner>
            {
                ReturnDocument = ReturnDocument.After
            };

            var owner = await _context.EVOwners.FindOneAndUpdateAsync(filter, update, options);
            return owner == null ? null : MapToDto(owner);
        }

        public async Task<bool> DeleteAsync(string nic)
        {
            var result = await _context.EVOwners.DeleteOneAsync(o => o.NIC == nic);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ToggleActiveStatusAsync(string nic)
        {
            var owner = await _context.EVOwners
                .Find(o => o.NIC == nic)
                .FirstOrDefaultAsync();

            if (owner == null) return false;

            var update = Builders<EVOwner>.Update
                .Set(o => o.IsActive, !owner.IsActive)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _context.EVOwners.UpdateOneAsync(
                o => o.NIC == nic, update);

            return result.ModifiedCount > 0;
        }

        public async Task<List<EVOwnerDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }

            var searchQuery = query.ToLower().Trim();

            var filter = Builders<EVOwner>.Filter.Or(
                Builders<EVOwner>.Filter.Regex(o => o.NIC, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i")),
                Builders<EVOwner>.Filter.Regex(o => o.FirstName, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i")),
                Builders<EVOwner>.Filter.Regex(o => o.LastName, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i")),
                Builders<EVOwner>.Filter.Regex(o => o.Email, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i"))
            );

            var owners = await _context.EVOwners
                .Find(filter)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();

            return owners.Select(MapToDto).ToList();
        }

        public async Task<EVOwnerDto?> LoginAsync(string nic, string password)
        {
            var owner = await _context.EVOwners
                .Find(o => o.NIC == nic)
                .FirstOrDefaultAsync();

            if (owner == null || !owner.IsActive)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, owner.PasswordHash))
            {
                return null;
            }

            return MapToDto(owner);
        }

        public async Task<bool?> DeactivateAsync(string nic, string? performedBy = null)
        {
            var owner = await _context.EVOwners
                .Find(o => o.NIC == nic)
                .FirstOrDefaultAsync();

            if (owner == null)
                return null;

            if (!owner.IsActive)
                return false; 

            var update = Builders<EVOwner>.Update
                .Set(o => o.IsActive, false)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _context.EVOwners.UpdateOneAsync(
                o => o.NIC == nic, update);

            if (result.ModifiedCount == 0)
                return false;
            return true;
        }

        private static EVOwnerDto MapToDto(EVOwner owner)
        {
            return new EVOwnerDto
            {
                Id = owner.Id,
                NIC = owner.NIC,
                FirstName = owner.FirstName,
                LastName = owner.LastName,
                Email = owner.Email,
                Phone = owner.Phone,
                Role = owner.Role,
                IsActive = owner.IsActive,
                CreatedAt = owner.CreatedAt,
                UpdatedAt = owner.UpdatedAt
            };
        }
    }
}