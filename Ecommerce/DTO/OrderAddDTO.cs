namespace Ecommerce.DTO
{
    public class OrderAddDTO
    {
        public string CustomerId { get; set; }
        public List<string> OrderItemIds { get; set; }
    }
}
