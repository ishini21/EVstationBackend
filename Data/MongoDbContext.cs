using MongoDB.Driver;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IConfiguration _configuration;

        public MongoDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectionString = _configuration.GetConnectionString("MongoDB");
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("EVManagmentDB");
        }

        public IMongoCollection<EVOwner> EVOwners => _database.GetCollection<EVOwner>("EVOwners");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        // Collections for Station Management
        public IMongoCollection<Station> Stations => _database.GetCollection<Station>("Stations");
        public IMongoCollection<Slot> Slots => _database.GetCollection<Slot>("Slots");
        public IMongoCollection<Operator> Operators => _database.GetCollection<Operator>("Operators");
        
        // Collections for Booking Management
        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
    }
}