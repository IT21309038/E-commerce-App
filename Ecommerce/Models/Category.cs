using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("category_name")]
        public string CategoryName { get; set; }

        [BsonElement("active_status")]
        public bool ActiveStatus { get; set; }
    }
}
