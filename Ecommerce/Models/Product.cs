using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("product_name")]
        public string ProductName { get; set; }

        [BsonElement("product_description")]
        public string ProductDescription { get; set; }

        [BsonElement("unit_price")]
        public decimal UnitPrice { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("initial_quantity")]
        public int InitialQuantity { get; set; }

        [BsonElement("image")]
        public string Image { get; set; }

        [BsonElement("category_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; }

        [BsonElement("vendor_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VendorId { get; set; }

        
    }
}
