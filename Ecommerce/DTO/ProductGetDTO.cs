﻿namespace Ecommerce.DTO
{
    public class ProductGetDTO
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int InitialQuantity { get; set; }
        public string Image { get; set; }
        public string CategoryName { get; set; }
        public string CategoryId { get; set; }
        public string VendorName { get; set; }
        public string VendorId { get; set; }
        public double Rating { get; set; }
        public bool LowStock { get; set; }
    }
}
