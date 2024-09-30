namespace Ecommerce.DTO
{
    public class ProductDTO
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
        public string CategoryId { get; set; }
        public string VendorId { get; set; }
    }
}
