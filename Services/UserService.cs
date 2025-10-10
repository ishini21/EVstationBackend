/************************************************************************************************
* Filename:         UserService.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file provides the concrete implementation of the IUserService interface. It contains
* all the business logic for managing system users (Backoffice and StationOperator),
* including creating, retrieving, updating, and deactivating user accounts.
************************************************************************************************/

using MongoDB.Driver;
using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Services
{
    /// <summary>
    /// Implements the IUserService to handle business logic for system users.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly MongoDbContext _context;

        // Method: Constructor for the UserService.
        // Injects the database context to be used for data operations.
        public UserService(MongoDbContext context)
        {
            _context = context;
        }

        // Method: Retrieves a list of all system users from the database.
        // Maps the User models to UserDto objects for client-side consumption.
        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            return users.Select(MapToDto).ToList();
        }

        // Method: Retrieves a single system user by their unique identifier.
        // Returns null if no user is found with the specified ID.
        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            return user == null ? null : MapToDto(user);
        }

        // Method: Creates a new system user.
        // It validates that the email is unique and hashes the password before storing it.
        public async Task<UserDto> CreateAsync(CreateUserDto createDto)
        {
            // Check if a user with the same email already exists to prevent duplicates.
            var existingEmail = await _context.Users
                .Find(u => u.Email == createDto.Email)
                .FirstOrDefaultAsync();

            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Email '{createDto.Email}' already exists.");
            }

            // Hash the user's password using BCrypt for secure storage.
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password);

            // Create a new User entity from the DTO.
            var user = new User
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber,
                Address = createDto.Address,
                PasswordHash = passwordHash,
                Role = createDto.Role,
                Status = UserStatus.Active, // New users are active by default.
                CreatedAt = DateTime.UtcNow
            };

            // Insert the new user into the database and return the mapped DTO.
            await _context.Users.InsertOneAsync(user);
            return MapToDto(user);
        }

        // Method: Updates an existing system user's information.
        // It dynamically builds an update operation based on the fields provided in the DTO.
        public async Task<UserDto?> UpdateAsync(string id, UpdateUserDto updateDto)
        {
            // Find the user to be updated.
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return null; // User not found.
            }

            // Create a list to hold all the update definitions.
            var updateDefinitions = new List<UpdateDefinition<User>>();

            // Conditionally add updates for each field if a new value is provided.
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
                // Ensure the new email doesn't already belong to another user.
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
                // Hash the new password if it's provided.
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.PasswordHash, passwordHash));
            }
            if (updateDto.Role.HasValue)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Role, updateDto.Role.Value));
                
                // If role is changed to Backoffice, ensure StationId is cleared.
                if (updateDto.Role.Value == UserRole.Backoffice)
                {
                    updateDefinitions.Add(Builders<User>.Update.Set(u => u.StationId, (string?)null));
                }
            }
            if (updateDto.ProfileImage != null)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ProfileImage, updateDto.ProfileImage));
            }

            // If no fields were provided for update, return the existing user data.
            if (updateDefinitions.Count == 0)
            {
                return MapToDto(user);
            }

            // Always update the 'UpdatedAt' timestamp.
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.UpdatedAt, DateTime.UtcNow));

            // Combine all updates and execute them in a single database operation.
            var combinedUpdate = Builders<User>.Update.Combine(updateDefinitions);
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var result = await _context.Users.FindOneAndUpdateAsync<User>(
                filter,
                combinedUpdate,
                new FindOneAndUpdateOptions<User, User> { ReturnDocument = ReturnDocument.After }
            );

            return result == null ? null : MapToDto(result);
        }

        // Method: Deactivates a user's account by setting their status to Inactive.
        public async Task<bool> DeactivateAsync(string id)
        {
            var update = Builders<User>.Update
                .Set(u => u.Status, UserStatus.Inactive)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // Method: Reactivates a user's account by setting their status to Active.
        public async Task<bool> ReactivateAsync(string id)
        {
            var update = Builders<User>.Update
                .Set(u => u.Status, UserStatus.Active)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // Method: Resets a user's password to a new value.
        // Hashes the new password before updating it in the database.
        public async Task<bool> ResetPasswordAsync(string id, string newPassword)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, passwordHash)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Method: A private helper to map a User model object to a UserDto.
        // This is used to control the data that is sent back to the client.
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
                ProfileImage = user.ProfileImage,
                LastLogin = user.LastLogin,
                StationId = user.StationId
            };
        }
    }
}
