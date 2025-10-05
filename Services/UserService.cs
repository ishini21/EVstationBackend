using MongoDB.Driver;
using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Services
{
    public class UserService : IUserService
    {
        private readonly MongoDbContext _context;

        public UserService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createDto)
        {
            // Check if email already exists
            var existingEmail = await _context.Users
                .Find(u => u.Email == createDto.Email)
                .FirstOrDefaultAsync();

            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Email '{createDto.Email}' already exists.");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password);

            var user = new User
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber,
                Address = createDto.Address,
                PasswordHash = passwordHash,
                Role = createDto.Role,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(user);
            return MapToDto(user);
        }

        public async Task<UserDto?> UpdateAsync(string id, UpdateUserDto updateDto)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            var updateDefinitions = new List<UpdateDefinition<User>>();

            if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.FirstName, updateDto.FirstName));
            }

            if (!string.IsNullOrWhiteSpace(updateDto.LastName))
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.LastName, updateDto.LastName));
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Email))
            {
                // Check if new email already exists for a different user
                var existingEmail = await _context.Users
                    .Find(u => u.Email == updateDto.Email && u.Id != id)
                    .FirstOrDefaultAsync();

                if (existingEmail != null)
                {
                    throw new InvalidOperationException($"Email '{updateDto.Email}' already exists.");
                }

                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Email, updateDto.Email));
            }

            if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.PhoneNumber, updateDto.PhoneNumber));
            }

            if (updateDto.Address != null)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Address, updateDto.Address));
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Password))
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.PasswordHash, passwordHash));
            }

            if (updateDto.Role.HasValue)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Role, updateDto.Role.Value));
            }

            if (updateDto.ProfileImage != null)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ProfileImage, updateDto.ProfileImage));
            }

            if (updateDefinitions.Count == 0)
            {
                return MapToDto(user); // No updates needed
            }

            // Add updated timestamp
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.UpdatedAt, DateTime.UtcNow));

            var combinedUpdate = Builders<User>.Update.Combine(updateDefinitions);
            var result = await _context.Users.FindOneAndUpdateAsync<User>(
                u => u.Id == id,
                combinedUpdate,
                new FindOneAndUpdateOptions<User, User> { ReturnDocument = ReturnDocument.After }
            );

            return result == null ? null : MapToDto(result);
        }

        public async Task<bool> DeactivateAsync(string id)
        {
            var update = Builders<User>.Update
                .Set(u => u.Status, UserStatus.Inactive)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ReactivateAsync(string id)
        {
            var update = Builders<User>.Update
                .Set(u => u.Status, UserStatus.Active)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                ProfileImage = user.ProfileImage
            };
        }
    }
}

