using Microsoft.AspNetCore.Mvc;
using Ecommerce.Models;
using Ecommerce.Data;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.DTO;
using MongoDB.Bson;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public ProductController(MongoDBContext context)
        {
            _context = context;
        }

        //Get all products search by product name and filter by category
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductGetDTO>>> GetProductsSearch([FromQuery] string? productName = null, [FromQuery] string? categoryId = null)
        {
            var filterBuilder = Builders<Product>.Filter;
            var filter = filterBuilder.Empty; // Start with an empty filter

            if (!string.IsNullOrEmpty(categoryId))
            {
                filter = filter & filterBuilder.Eq("category_id", ObjectId.Parse(categoryId));
            }

            if (!string.IsNullOrEmpty(productName))
            {
                filter = filter & filterBuilder.Regex("product_name", new BsonRegularExpression(productName, "i"));
            }

            var products = await _context.Products.Find(filter).ToListAsync();
            var productDTOs = new List<ProductGetDTO>();

            foreach (var product in products)
            {
                var category = await _context.Categories.Find(c => c.Id == product.CategoryId && c.ActiveStatus == true).FirstOrDefaultAsync();
                var vendor = await _context.Users.Find(u => u.Id == product.VendorId).FirstOrDefaultAsync();

                var ranks = await _context.Ranks.Find(r => r.VendorId == product.VendorId).ToListAsync();
                double totalRank = 0;
                foreach (var rank in ranks)
                {
                    totalRank += rank.Rank;
                }
                double rating = ranks.Count == 0 ? 0 : totalRank / ranks.Count;

                bool lowStock = product.Quantity < Math.Ceiling(0.2 * product.InitialQuantity);
                if (lowStock)
                {
                    var notification = new NotificationLowStock
                    {
                        CreatedTime = DateTime.UtcNow,
                        VendorId = product.VendorId,
                        Message = $"Product {product.ProductName} is low on stock.",
                        MarkRead = false
                    };
                    await _context.NotificationLowStock.InsertOneAsync(notification);
                }

                productDTOs.Add(
                    new ProductGetDTO
                    {
                        Id = product.Id.ToString(),
                        ProductName = product.ProductName,
                        ProductDescription = product.ProductDescription,
                        UnitPrice = product.UnitPrice,
                        Quantity = product.Quantity,
                        InitialQuantity = product.InitialQuantity,
                        Image = product.Image,
                        CategoryName = category?.CategoryName,
                        VendorName = vendor?.Name,
                        Rating = rating,
                        VendorId = product.VendorId,
                        LowStock = lowStock
                    }
                );
            }
            return Ok(productDTOs);
        }

        // GET: api/Product/{id}
        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        public async Task<ActionResult<ProductGetDTO>> GetProduct(string id)
        {
            var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.Find(c => c.Id == product.CategoryId).FirstOrDefaultAsync();
            var vendor = await _context.Users.Find(u => u.Id == product.VendorId).FirstOrDefaultAsync();

            var ranks = await _context.Ranks.Find(r => r.VendorId == product.VendorId).ToListAsync();
            double totalRank = 0;
            foreach (var rank in ranks)
            {
                totalRank += rank.Rank;
            }
            double rating = ranks.Count == 0 ? 0 : totalRank / ranks.Count;

            bool lowStock = product.Quantity < Math.Ceiling(0.2 * product.InitialQuantity);
            if (lowStock)
            {
                var notification = new NotificationLowStock
                {
                    CreatedTime = DateTime.UtcNow,
                    VendorId = product.VendorId,
                    Message = $"Product {product.ProductName} is low on stock.",
                    MarkRead = false
                };
                await _context.NotificationLowStock.InsertOneAsync(notification);
            }

            var productDTO = new ProductGetDTO
            {
                Id = product.Id.ToString(),
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                UnitPrice = product.UnitPrice,
                Quantity = product.Quantity,
                InitialQuantity = product.InitialQuantity,
                Image = product.Image,
                CategoryName = category?.CategoryName,
                VendorName = vendor?.Name,
                Rating = rating,
                VendorId = product.VendorId,
                LowStock = lowStock
            };

            return productDTO;
        }


        //Get product by VendorId
        [HttpGet("vendor/{id:length(24)}")]
        public async Task<ActionResult<IEnumerable<ProductGetDTO>>> GetProductByVendor(string id)
        {
            var products = await _context.Products.Find(p => p.VendorId == id).ToListAsync();
            var productDTOs = new List<ProductGetDTO>();

            foreach (var product in products)
            {
                var category = await _context.Categories.Find(c => c.Id == product.CategoryId && c.ActiveStatus == true).FirstOrDefaultAsync();
                var vendor = await _context.Users.Find(u => u.Id == product.VendorId).FirstOrDefaultAsync();

                var ranks = await _context.Ranks.Find(r => r.VendorId == product.VendorId).ToListAsync();
                double totalRank = 0;
                foreach (var rank in ranks)
                {
                    totalRank += rank.Rank;
                }
                double rating = ranks.Count == 0 ? 0 : totalRank / ranks.Count;

                bool lowStock = product.Quantity < Math.Ceiling(0.2 * product.InitialQuantity);
                if (lowStock)
                {
                    var notification = new NotificationLowStock
                    {
                        CreatedTime = DateTime.UtcNow,
                        VendorId = product.VendorId,
                        Message = $"Product {product.ProductName} is low on stock.",
                        MarkRead = false
                    };
                    await _context.NotificationLowStock.InsertOneAsync(notification);
                }

                productDTOs.Add(
                    new ProductGetDTO
                    {
                        Id = product.Id.ToString(),
                        ProductName = product.ProductName,
                        ProductDescription = product.ProductDescription,
                        UnitPrice = product.UnitPrice,
                        Quantity = product.Quantity,
                        InitialQuantity = product.InitialQuantity,
                        Image = product.Image,
                        CategoryName = category?.CategoryName,
                        VendorName = vendor?.Name,
                        Rating = rating,
                        VendorId = product.VendorId,
                        LowStock = lowStock
                    }
                );
            }
            return Ok(productDTOs);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<Product>> Create(ProductDTO productDTO)
        {
            try
            {
                // Check if category exists
                var category = await _context.Categories.Find(c => c.Id == productDTO.CategoryId).FirstOrDefaultAsync();
                if (category == null)
                {
                    return BadRequest("Invalid category ID.");
                }

                // Check if vendor exists (Optional validation)
                var vendor = await _context.Users.Find(u => u.Id == productDTO.VendorId).FirstOrDefaultAsync();
                if (vendor == null)
                {
                    return BadRequest("Invalid vendor ID.");
                }

                // Create new product
                var product = new Product
                {
                    ProductName = productDTO.ProductName,
                    ProductDescription = productDTO.ProductDescription,
                    UnitPrice = productDTO.UnitPrice,
                    InitialQuantity = productDTO.Quantity,
                    Quantity = productDTO.Quantity,
                    Image = productDTO.Image,
                    CategoryId = productDTO.CategoryId,
                    VendorId = productDTO.VendorId
                };

                // Insert into the database
                await _context.Products.InsertOneAsync(product);

                // Return the newly created product using the "GetProduct" route
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id.ToString() }, product);
            }
            catch (Exception ex)
            {
                // Log the error (consider adding a logging framework)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //update product
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, ProductUpdateDTO productDTO)
        {
            var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            // Check if category exists
            var category = await _context.Categories.Find(c => c.Id == productDTO.CategoryId).FirstOrDefaultAsync();
            if (category == null)
            {
                return BadRequest("Invalid category ID.");
            }

            // Check if vendor exists (Optional validation)
            var vendor = await _context.Users.Find(u => u.Id == productDTO.VendorId).FirstOrDefaultAsync();
            if (vendor == null)
            {
                return BadRequest("Invalid vendor ID.");
            }

            // Update product properties
            product.ProductName = productDTO.ProductName;
            product.ProductDescription = productDTO.ProductDescription;
            product.UnitPrice = productDTO.UnitPrice;
            product.Image = productDTO.Image;
            product.CategoryId = productDTO.CategoryId;
            product.VendorId = productDTO.VendorId;

            // Replace the product in the database
            await _context.Products.ReplaceOneAsync(p => p.Id == id, product);

            return Ok("Product with " + id + " updated successfully");
        }

        //Update Restock product using RestockDTO
        [HttpPut("restock/{id:length(24)}")]
        public async Task<IActionResult> Restock(string id, RestockDTO restockDTO)
        {
            var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            // Update the quantity of the product
            product.Quantity += restockDTO.Quantity;
            product.InitialQuantity = product.Quantity;

            // Replace the product in the database
            await _context.Products.ReplaceOneAsync(p => p.Id == id, product);

            return Ok("Product with " + id + " restocked successfully");
        }

        //Delete products allow onnly if the product is not in any order
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            // Check if the product ID is in any order where the delivery status is "Processing To Deliver" or "Partially Delivered"
            var order = await _context.Orders.Find(o =>
                o.OrderItems.Any(pl => pl.ProductId == id) &&
                (o.OrderStatus == "Processing To Deliver" || o.OrderStatus == "Partially Delivered")
            ).FirstOrDefaultAsync();

            if (order != null)
            {
                return BadRequest("Product cannot be deleted as it is part of an order with a pending delivery status.");
            }

            // Delete the product
            await _context.Products.DeleteOneAsync(p => p.Id == id);

            return Ok("Product with " + id + " deleted successfully");
        }


    }
}
