namespace Ecommerce.DTO
{
    public class ListingGetDTO
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string OrderId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public bool ReadyStatus { get; set; }
        public bool DeliveredStatus { get; set; }
    }
}
