using Microsoft.AspNetCore.Mvc;
using Ecommerce.Models;
using Ecommerce.Data;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.DTO;

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

        // GET: api/Product/{id}
        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        public async Task<ActionResult<ProductGetDTO>> GetProduct(string id)
        {
            var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            // Fetch related category and vendor information
            var category = await _context.Categories.Find(c => c.Id == product.CategoryId).FirstOrDefaultAsync();
            var vendor = await _context.Users.Find(u => u.Id == product.VendorId).FirstOrDefaultAsync();

            // Construct the ProductGetDTO to return
            var productDTO = new ProductGetDTO
            {
                Id = product.Id.ToString(),
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                UnitPrice = product.UnitPrice,
                Quantity = product.Quantity,
                Image = product.Image,
                CategoryName = category?.CategoryName,  // Null check in case category is missing
                VendorName = vendor?.Name,               // Null check in case vendor is missing
                VendorId = product.VendorId
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
                // Fetch the category and ensure ActiveStatus == true
                var category = await _context.Categories.Find(c => c.Id == product.CategoryId && c.ActiveStatus == true).FirstOrDefaultAsync();
                var vendor = await _context.Users.Find(u => u.Id == product.VendorId).FirstOrDefaultAsync();
                productDTOs.Add(
                    new ProductGetDTO
                    {
                        Id = product.Id.ToString(),
                        ProductName = product.ProductName,
                        ProductDescription = product.ProductDescription,
                        UnitPrice = product.UnitPrice,
                        Quantity = product.Quantity,
                        Image = product.Image,
                        CategoryName = category.CategoryName,
                        VendorName = vendor?.Name,
                        VendorId = product.VendorId
                    }
                );
            }
            return Ok( productDTOs );
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductGetDTO>>> Get()
        {
            var products = await _context.Products.Find(p => true).ToListAsync();
            var productDTOs = new List<ProductGetDTO>();

            foreach (var product in products)
            {
                // Fetch the category and ensure ActiveStatus == true
                var category = await _context.Categories.Find(c => c.Id == product.CategoryId && c.ActiveStatus == true).FirstOrDefaultAsync();

                // If the category is not active or doesn't exist, skip the product
                if (category == null)
                {
                    continue;
                }

                // Fetch the vendor information (optional null check)
                var vendor = await _context.Users.Find(u => u.Id == product.VendorId).FirstOrDefaultAsync();

                // Add product to the DTO list
                productDTOs.Add(new ProductGetDTO
                {
                    Id = product.Id.ToString(),
                    ProductName = product.ProductName,
                    ProductDescription = product.ProductDescription,
                    UnitPrice = product.UnitPrice,
                    Quantity = product.Quantity,
                    Image = product.Image,
                    CategoryName = category.CategoryName, // Category is guaranteed to be active here
                    VendorName = vendor?.Name,             // Null check in case vendor is missing
                    VendorId = product.VendorId
                });
            }

            return productDTOs;
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
        public async Task<IActionResult> Update(string id, ProductDTO productDTO)
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
            product.Quantity = productDTO.Quantity;
            product.Image = productDTO.Image;
            product.CategoryId = productDTO.CategoryId;
            product.VendorId = productDTO.VendorId;

            // Replace the product in the database
            await _context.Products.ReplaceOneAsync(p => p.Id == id, product);

            return Ok("Product with " + id + " updated successfully");
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            await _context.Products.DeleteOneAsync(p => p.Id == id);

            return Ok("product with id " + id + " deleted successfully");
        }

    }
}
