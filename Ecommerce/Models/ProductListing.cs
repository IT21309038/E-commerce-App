using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class ProductListing
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("ProductId")]
        public string ProductId { get; set; }
        [BsonElement("OrderId")]
        public string OrderId { get; set; }
        [BsonElement("Quantity")]
        public int Quantity { get; set; }
        [BsonElement("Price")]
        public double Price { get; set; }
        [BsonElement("ReadyStatus")]
        public bool ReadyStatus { get; set; }
        [BsonElement("DeliveredStatus")]
        public bool DeliveredStatus { get; set; }
    }
}
