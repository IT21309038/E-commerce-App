using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class NotificationLowStock
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("created_time")]
        public DateTime CreatedTime { get; set; }

        [BsonElement("vendor_id")]
        public string VendorId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("mark_read")]
        public bool MarkRead { get; set; }
    }
}
