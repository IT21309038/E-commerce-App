using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class CancelledOrders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("order_date")]
        public DateTime OrderDate { get; set; }

        [BsonElement("order_id")]
        public string OrderId { get; set; }

        [BsonElement("customer_id")]
        public string CustomerId { get; set; }

        [BsonElement("cancel_note")]
        public string CancelNote { get; set; }
    }
}
