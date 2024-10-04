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
    public class OrderController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public OrderController(MongoDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> Get()
        {
            // Fetch all orders
            var orders = await _context.Orders.Find(order => true).ToListAsync();

            // Fetch and update the order items with fresh product listings
            foreach (var order in orders)
            {
                var updatedOrderItems = await _context.ProductListings
                                                      .Find(listing => order.OrderItems.Select(i => i.Id).Contains(listing.Id))
                                                      .ToListAsync();
                order.OrderItems = updatedOrderItems; // Update order with fresh product listings

                // Update OrderStatus and EditableStatus based on DeliveredStatus and ActiveStatus
                UpdateOrderStatus(order, updatedOrderItems);

                // Save changes to the order
                await _context.Orders.ReplaceOneAsync(o => o.Id == order.Id, order);
            }

            return Ok(orders);
        }

        [HttpGet("{id:length(24)}", Name = "GetOrder")]
        public async Task<ActionResult<Order>> Get(string id)
        {
            // Fetch the order by ID
            var order = await _context.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(); // Return 404 if the order is not found
            }

            // Fetch fresh product listings for this order
            var updatedOrderItems = await _context.ProductListings
                                                  .Find(listing => order.OrderItems.Select(i => i.Id).Contains(listing.Id))
                                                  .ToListAsync();
            order.OrderItems = updatedOrderItems; // Update order with fresh product listings

            // Update OrderStatus and EditableStatus based on DeliveredStatus and ActiveStatus
            UpdateOrderStatus(order, updatedOrderItems);

            // Save changes to the order
            await _context.Orders.ReplaceOneAsync(o => o.Id == order.Id, order);

            return Ok(order);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetByCustomerId(string customerId)
        {
            // Fetch all orders by CustomerId
            var orders = await _context.Orders.Find(order => order.CustomerId == customerId).ToListAsync();

            // Fetch and update the order items with fresh product listings
            foreach (var order in orders)
            {
                var updatedOrderItems = await _context.ProductListings
                                                      .Find(listing => order.OrderItems.Select(i => i.Id).Contains(listing.Id))
                                                      .ToListAsync();
                order.OrderItems = updatedOrderItems; // Update order with fresh product listings

                // Update OrderStatus and EditableStatus based on DeliveredStatus and ActiveStatus
                UpdateOrderStatus(order, updatedOrderItems);

                // Save changes to the order
                await _context.Orders.ReplaceOneAsync(o => o.Id == order.Id, order);
            }

            return Ok(orders);
        }


        private void UpdateOrderStatus(Order order, List<ProductListing> orderItems)
        {
            // Determine the OrderStatus based on DeliveredStatus
            if (orderItems.All(item => item.DeliveredStatus == true))
            {
                order.OrderStatus = "Delivered";
            }
            else if (orderItems.Any(item => item.DeliveredStatus == true))
            {
                order.OrderStatus = "Partially Delivered";
            }
            else
            {
                order.OrderStatus = "Processing To Deliver";
            }

            // Determine the EditableStatus based on ActiveStatus
            if (orderItems.Any(item => item.ReadyStatus == true))
            {
                order.EditableStatus = false;
            }
            else
            {
                order.EditableStatus = true;
            }
        }

        [HttpPost]
        public async Task<ActionResult<Order>> Create(OrderAddDTO orderAddDTO)
        {
            // Fetch product listings from the database using the provided IDs
            var orderItems = await _context.ProductListings
                                           .Find(listing => orderAddDTO.OrderItemIds.Contains(listing.Id))
                                           .ToListAsync();

            if (orderItems.Count != orderAddDTO.OrderItemIds.Count)
            {
                return BadRequest("Some product listings could not be found.");
            }

            // Fetch products to check quantities
            var insufficientProducts = new List<string>();

            foreach (var item in orderItems)
            {
                var product = await _context.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                if (product == null || product.Quantity < item.Quantity)
                {
                    insufficientProducts.Add(item.ProductId);
                }
            }

            if (insufficientProducts.Any())
            {
                return BadRequest($"Insufficient quantity for products: {string.Join(", ", insufficientProducts)}");
            }

            // Reduce the product quantities
            foreach (var item in orderItems)
            {
                var product = await _context.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                product.Quantity -= item.Quantity;
                await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                OrderStatus = "Processing To Deliver",
                EditableStatus = true,
                TotalAmount = (decimal)orderItems.Sum(item => item.Price),
                CustomerId = orderAddDTO.CustomerId,
                OrderItems = orderItems
            };

            // Insert the order into the Orders collection
            await _context.Orders.InsertOneAsync(order);

            // Update the orderId field in each ProductListing
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                await _context.ProductListings.ReplaceOneAsync(p => p.Id == item.Id, item);
            }

            return CreatedAtRoute("GetOrder", new { id = order.Id.ToString() }, order);
        }


        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, OrderUpdateDTO orderUpdateDTO)
        {
            var order = await _context.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound();
            }

            var currentOrderItems = order.OrderItems;
            var updatedOrderItems = await _context.ProductListings
                                                  .Find(listing => orderUpdateDTO.OrderItemIds.Contains(listing.Id))
                                                  .ToListAsync();

            if (updatedOrderItems.Count != orderUpdateDTO.OrderItemIds.Count)
            {
                return BadRequest("Some product listings could not be found.");
            }

            if (updatedOrderItems.Any(item => item.ReadyStatus == true))
            {
                return BadRequest("Cannot update the order because one or more products have ReadyStatus set to true.");
            }

            // Check for sufficient quantity in newly added products
            var newItems = updatedOrderItems.Where(item => !currentOrderItems.Any(c => c.Id == item.Id)).ToList();
            var insufficientProducts = new List<string>();

            foreach (var item in newItems)
            {
                var product = await _context.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                if (product == null || product.Quantity < item.Quantity)
                {
                    insufficientProducts.Add(item.ProductId);
                }
            }

            if (insufficientProducts.Any())
            {
                return BadRequest($"Insufficient quantity for products: {string.Join(", ", insufficientProducts)}");
            }

            // Restore product quantity for removed items
            var removedItems = currentOrderItems.Where(item => !updatedOrderItems.Any(u => u.Id == item.Id)).ToList();
            foreach (var item in removedItems)
            {
                var product = await _context.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                product.Quantity += item.Quantity;
                await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
            }

            // Reduce product quantity for newly added items
            foreach (var item in newItems)
            {
                var product = await _context.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                product.Quantity -= item.Quantity;
                await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
            }

            // Update the order and save changes
            order.OrderItems = updatedOrderItems;
            order.TotalAmount = (decimal)updatedOrderItems.Sum(item => item.Price);
            await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

            return Ok("Order updated successfully");
        }


        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _context.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound();
            }

            var orderItems = await _context.ProductListings
                                           .Find(listing => order.OrderItems.Select(i => i.Id).Contains(listing.Id))
                                           .ToListAsync();

            if (orderItems.Any(item => item.ReadyStatus == true))
            {
                return BadRequest("Cannot delete the order because one or more products have ReadyStatus set to true.");
            }

            // Restore product quantities before deleting the order
            foreach (var item in orderItems)
            {
                var product = await _context.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                product.Quantity += item.Quantity;
                await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
            }

            // Set the OrderId of each product listing in the order to null
            foreach (var item in orderItems)
            {
                item.OrderId = null;
                await _context.ProductListings.ReplaceOneAsync(p => p.Id == item.Id, item);
            }

            await _context.Orders.DeleteOneAsync(o => o.Id == id);

            return Ok("Order deleted successfully");
        }


        //put method to update order status to Delivered and all product listings to DeliveredStatus to true and ReadyStatus to true
        [HttpPut("deliver/{id:length(24)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeliverOrder(string id)
        {
            // Fetch the order by its ID
            var order = await _context.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound(); // Return 404 if the order is not found
            }

            // Fetch fresh product listings associated with this order
            var orderItems = await _context.ProductListings
                                           .Find(listing => order.OrderItems.Select(i => i.Id).Contains(listing.Id))
                                           .ToListAsync();

            // Update the DeliveredStatus and ReadyStatus of each product listing in the order
            foreach (var item in orderItems)
            {
                item.DeliveredStatus = true;
                item.ReadyStatus = true;
                await _context.ProductListings.ReplaceOneAsync(p => p.Id == item.Id, item);
            }

            // Update the OrderStatus and EditableStatus of the order
            order.OrderStatus = "Delivered";
            order.EditableStatus = false;

            // Save the updated order
            await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

            return Ok("Order with " + id + " delivered successfully");
        }

    }
}
