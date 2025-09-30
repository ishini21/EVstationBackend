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
            _database = client.GetDatabase("EVOwnerManagementDB");
        }

        public IMongoCollection<EVOwner> EVOwners => _database.GetCollection<EVOwner>("EVOwners");
    }
}