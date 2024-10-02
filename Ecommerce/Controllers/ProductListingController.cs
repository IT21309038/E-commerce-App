using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Models;
using Ecommerce.Data;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.DTO;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductListingController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public ProductListingController(MongoDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListingGetDTO>>> Get()
        {
            var productListings = await _context.ProductListings.Find(productListing => true).ToListAsync();

            // Fetch product details for the ProductId
            var productIds = productListings.Select(pl => pl.ProductId).Distinct().ToList();
            var products = await _context.Products.Find(product => productIds.Contains(product.Id)).ToListAsync();

            // Fetch vendor IDs based on the products
            var vendorIds = products.Select(p => p.VendorId).Distinct().ToList();
            var vendors = await _context.Users.Find(vendor => vendorIds.Contains(vendor.Id)).ToListAsync();

            var productListingsWithDetails = productListings.Select(pl =>
            {
                // Find the product for this product listing
                var product = products.FirstOrDefault(p => p.Id == pl.ProductId);

                // Find the vendor using the VendorId from the product
                var vendor = product != null ? vendors.FirstOrDefault(v => v.Id == product.VendorId) : null;

                return new ListingGetDTO
                {
                    Id = pl.Id,
                    ProductId = pl.ProductId,
                    UserId = pl.UserId,
                    ProductName = product?.ProductName,  // Fetch the product name
                    VendorId = product?.VendorId,
                    VendorName = vendor?.Name,     // Fetch the vendor name using the VendorId
                    OrderId = pl.OrderId,
                    Quantity = pl.Quantity,
                    Price = pl.Price,
                    ReadyStatus = pl.ReadyStatus,
                    DeliveredStatus = pl.DeliveredStatus
                };
            }).ToList();

            return Ok(productListingsWithDetails);
        }


        [HttpGet("{id:length(24)}", Name = "GetProductListing")]
        public async Task<ActionResult<ListingGetDTO>> Get(string id)
        {
            // Find the product listing by ID
            var productListing = await _context.ProductListings.Find<ProductListing>(pl => pl.Id == id).FirstOrDefaultAsync();

            if (productListing == null)
            {
                return NotFound();
            }

            // Fetch the product associated with the ProductId
            var product = await _context.Products.Find(p => p.Id == productListing.ProductId).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Product not found");
            }

            // Fetch the vendor associated with the VendorId from the product
            var vendor = await _context.Users.Find(v => v.Id == product.VendorId).FirstOrDefaultAsync();

            // Create the DTO with product and vendor details
            var listingGetDTO = new ListingGetDTO
            {
                Id = productListing.Id,
                ProductId = productListing.ProductId,
                UserId = productListing.UserId,
                ProductName = product.ProductName,  // Product name from the Products collection
                VendorId = product.VendorId,
                VendorName = vendor?.Name,    // Vendor name from the Vendors collection
                OrderId = productListing.OrderId,
                Quantity = productListing.Quantity,
                Price = productListing.Price,
                ReadyStatus = productListing.ReadyStatus,
                DeliveredStatus = productListing.DeliveredStatus
            };

            return Ok(listingGetDTO);
        }

        //Get product listings by vendor id where order id is not null
        [HttpGet("vendor/{vendorId:length(24)}")]
        public async Task<ActionResult<IEnumerable<ListingGetDTO>>> GetByVendorId(string vendorId)
        {
            var productListings = await _context.ProductListings.Find(productListing => productListing.OrderId != null).ToListAsync();

            // Fetch product details for the ProductId
            var productIds = productListings.Select(pl => pl.ProductId).Distinct().ToList();
            var products = await _context.Products.Find(product => productIds.Contains(product.Id)).ToListAsync();

            // Fetch vendor IDs based on the products
            var vendorIds = products.Select(p => p.VendorId).Distinct().ToList();
            var vendors = await _context.Users.Find(vendor => vendorIds.Contains(vendor.Id)).ToListAsync();

            var productListingsWithDetails = productListings.Select(pl =>
            {
                // Find the product for this product listing
                var product = products.FirstOrDefault(p => p.Id == pl.ProductId);

                // Find the vendor using the VendorId from the product
                var vendor = product != null ? vendors.FirstOrDefault(v => v.Id == product.VendorId) : null;

                return new ListingGetDTO
                {
                    Id = pl.Id,
                    ProductId = pl.ProductId,
                    UserId = pl.UserId,
                    ProductName = product?.ProductName,  // Fetch the product name
                    VendorId = product?.VendorId,
                    VendorName = vendor?.Name,     // Fetch the vendor name using the VendorId
                    OrderId = pl.OrderId,
                    Quantity = pl.Quantity,
                    Price = pl.Price,
                    ReadyStatus = pl.ReadyStatus,
                    DeliveredStatus = pl.DeliveredStatus
                };
            }).ToList();

            return Ok(productListingsWithDetails);
        }

        //Get product listing by user id
        [HttpGet("user/{userId:length(24)}")]
        public async Task<ActionResult<IEnumerable<ListingGetDTO>>> GetByUserId(string userId)
        {
            var productListings = await _context.ProductListings.Find(productListing => productListing.UserId == userId).ToListAsync();

            // Fetch product details for the ProductId
            var productIds = productListings.Select(pl => pl.ProductId).Distinct().ToList();
            var products = await _context.Products.Find(product => productIds.Contains(product.Id)).ToListAsync();

            // Fetch vendor IDs based on the products
            var vendorIds = products.Select(p => p.VendorId).Distinct().ToList();
            var vendors = await _context.Users.Find(vendor => vendorIds.Contains(vendor.Id)).ToListAsync();

            var productListingsWithDetails = productListings.Select(pl =>
            {
                // Find the product for this product listing
                var product = products.FirstOrDefault(p => p.Id == pl.ProductId);

                // Find the vendor using the VendorId from the product
                var vendor = product != null ? vendors.FirstOrDefault(v => v.Id == product.VendorId) : null;

                return new ListingGetDTO
                {
                    Id = pl.Id,
                    ProductId = pl.ProductId,
                    UserId = pl.UserId,
                    ProductName = product?.ProductName,  // Fetch the product name
                    VendorId = product?.VendorId,
                    VendorName = vendor?.Name,     // Fetch the vendor name using the VendorId
                    OrderId = pl.OrderId,
                    Quantity = pl.Quantity,
                    Price = pl.Price,
                    ReadyStatus = pl.ReadyStatus,
                    DeliveredStatus = pl.DeliveredStatus
                };
            }).ToList();

            return Ok(productListingsWithDetails);
        }


        [HttpPost]
        public async Task<ActionResult<ProductListing>> Create(ListingAddDTO productListingAddDTO)
        {
            var existingProduct = await _context.Products.Find<Product>(product => product.Id == productListingAddDTO.ProductId).FirstOrDefaultAsync();

            if (existingProduct == null)
            {
                return BadRequest("Product does not exist");
            }

            // Calculate the total price by multiplying the unit price by the quantity
            var totalPrice = existingProduct.UnitPrice * productListingAddDTO.Quantity;

            var productListing = new ProductListing
            {
                ProductId = productListingAddDTO.ProductId,
                UserId = productListingAddDTO.UserId,
                Quantity = productListingAddDTO.Quantity,
                Price = (double)totalPrice, // Set the calculated price
                OrderId = null,
                ReadyStatus = false,
                DeliveredStatus = false
            };

            await _context.ProductListings.InsertOneAsync(productListing);
            return CreatedAtRoute("GetProductListing", new { id = productListing.Id.ToString() }, productListing);
        }


        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, ListingUpdateDTO productListingUpdateDTO)
        {
            // Find the existing product listing
            var productListing = await _context.ProductListings.Find<ProductListing>(listing => listing.Id == id).FirstOrDefaultAsync();

            if (productListing == null)
            {
                return NotFound();
            }

            // Fetch the product associated with the listing to get the unit price
            var existingProduct = await _context.Products.Find<Product>(product => product.Id == productListing.ProductId).FirstOrDefaultAsync();

            if (existingProduct == null)
            {
                return BadRequest("Product does not exist");
            }

            var totalPrice = existingProduct.UnitPrice * productListingUpdateDTO.Quantity;

            // Update the quantity and calculate the new price based on the product's unit price
            productListing.Quantity = productListingUpdateDTO.Quantity;
            productListing.Price = (double)totalPrice;

            // Save the updated product listing
            await _context.ProductListings.ReplaceOneAsync(listing => listing.Id == id, productListing);
            return Ok("Updated product listing with id " + id + " successfully");
        }


        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var productListing = await _context.ProductListings.Find<ProductListing>(productListing => productListing.Id == id).FirstOrDefaultAsync();

            if (productListing == null)
            {
                return NotFound();
            }

            await _context.ProductListings.DeleteOneAsync(productListing => productListing.Id == id);
            return Ok("Product with id " + id + " deleted sucessfully");
        }

        [HttpPut("{id:length(24)}/ready")]
        public async Task<IActionResult> SetReadyStatus(string id)
        {
            var productListing = await _context.ProductListings.Find<ProductListing>(productListing => productListing.Id == id).FirstOrDefaultAsync();

            if (productListing == null)
            {
                return NotFound();
            }

            if (productListing.ReadyStatus == true)
            {
                return BadRequest("Product listing is already ready");
            }

            productListing.ReadyStatus = true;

            await _context.ProductListings.ReplaceOneAsync(productListing => productListing.Id == id, productListing);
            return Ok("Product listing with id " + id + " is now ready");
        }

        [HttpPut("{id:length(24)}/delivered")]
        public async Task<IActionResult> SetDeliveredStatus(string id)
        {
            var productListing = await _context.ProductListings.Find<ProductListing>(productListing => productListing.Id == id).FirstOrDefaultAsync();

            if (productListing == null)
            {
                return NotFound();
            }

            if (productListing.DeliveredStatus == true)
            {
                return BadRequest("Product listing is already delivered");
            }

            productListing.DeliveredStatus = true;

            await _context.ProductListings.ReplaceOneAsync(productListing => productListing.Id == id, productListing);
            return Ok("Product listing with id " + id + " is now delivered");
        }
    }
}
