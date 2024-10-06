namespace Ecommerce.DTO
{
    public class OrderGetDTO
    {
        public string Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public bool EditableStatus { get; set; }
        public bool CancelStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerId { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }
}
