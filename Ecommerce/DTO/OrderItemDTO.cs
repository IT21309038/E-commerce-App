namespace Ecommerce.DTO
{
    public class OrderItemDTO
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public bool ReadyStatus { get; set; }
        public bool DeliveredStatus { get; set; }
    }
}
