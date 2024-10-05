using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("order_date")]
        public DateTime OrderDate { get; set; }

        [BsonElement("order_status")]
        public string OrderStatus { get; set; }

        [BsonElement("editable_status")]
        public bool EditableStatus { get; set; }

        [BsonElement("cancel_status")]
        public bool CancelStatus { get; set; }

        [BsonElement("total_amount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("customer_id")]
        public string CustomerId { get; set; }

        [BsonElement("order_items")]
        public List<ProductListing> OrderItems { get; set; }
    }
}
