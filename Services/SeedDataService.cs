using MongoDB.Driver;
using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Services
{
    public class SeedDataService
    {
        private readonly MongoDbContext _context;

        public SeedDataService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task SeedAdminUserAsync()
        {
            // Check if any users exist
            var userCount = await _context.Users.CountDocumentsAsync(_ => true);
            
            if (userCount == 0)
            {
                // Create default admin user
                var adminUser = new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@evstation.com",
                    PhoneNumber = "+1234567890",
                    Address = "System Administration",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    Role = UserRole.Backoffice,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    ProfileImage = null
                };

                await _context.Users.InsertOneAsync(adminUser);
                Console.WriteLine("✅ Default admin user created:");
                Console.WriteLine($"   Email: admin@evstation.com");
                Console.WriteLine($"   Password: Admin123!");
                Console.WriteLine($"   Role: Backoffice");
            }
        }
    }
}
