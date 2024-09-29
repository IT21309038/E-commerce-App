﻿using Ecommerce.Models;
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
    }
}