using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class NotificationOrderCancel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("created_time")]
        public DateTime CreatedTime { get; set; }

        [BsonElement("order_id")]
        public string OrderId { get; set; }

        [BsonElement("user_id")]
        public string UserId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("mark_read")]
        public bool MarkRead { get; set; }
    }
}
