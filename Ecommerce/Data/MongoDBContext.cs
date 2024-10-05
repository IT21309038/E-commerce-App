using Ecommerce.Models;
using MongoDB.Driver;

namespace Ecommerce.Data
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase? _database;

        public MongoDBContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
        public IMongoCollection<ProductListing> ProductListings => _database.GetCollection<ProductListing>("ProductListings");
        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
        public IMongoCollection<Ranking> Ranks => _database.GetCollection<Ranking>("Ranks");
        public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("Reviews");
        public IMongoCollection<CancelledOrders> CancelledOrders => _database.GetCollection<CancelledOrders>("CancelledOrders");
    }
}
