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

            return Ok(order);
        }

        //Get by CustomerId
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
            }

            return Ok(orders);
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

            var order = new Order
            {
                OrderDate = DateTime.Now,
                OrderStatus = "Processing To Deliver",
                TotalAmount = (decimal)orderItems.Sum(item => item.Price),
                CustomerId = orderAddDTO.CustomerId,
                OrderItems = orderItems // Full product listings are added here
            };

            // Insert the order into the Orders collection
            await _context.Orders.InsertOneAsync(order);

            // Update the orderId field in each ProductListing
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id; // Set the orderId field
                await _context.ProductListings.ReplaceOneAsync(p => p.Id == item.Id, item);
            }

            return CreatedAtRoute("GetOrder", new { id = order.Id.ToString() }, order);
        }

        [HttpPut("{id:length(24)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, OrderUpdateDTO orderUpdateDTO)
        {
            // Fetch the order
            var order = await _context.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound(); // Return 404 if the order is not found
            }

            // Fetch current product listings in the order
            var currentOrderItems = order.OrderItems;

            // Fetch updated product listings from the database using the provided IDs
            var updatedOrderItems = await _context.ProductListings
                                                  .Find(listing => orderUpdateDTO.OrderItemIds.Contains(listing.Id))
                                                  .ToListAsync();

            if (updatedOrderItems.Count != orderUpdateDTO.OrderItemIds.Count)
            {
                return BadRequest("Some product listings could not be found."); // Return 400 if some product listings are missing
            }

            // Check if any product's ReadyStatus is true and block the update if so
            if (updatedOrderItems.Any(item => item.ReadyStatus == true))
            {
                return BadRequest("Cannot update the order because one or more products have ReadyStatus set to true.");
            }

            // Find products that were removed in the update (products that are in the current order but not in the updated list)
            var removedOrderItems = currentOrderItems.Where(item => !orderUpdateDTO.OrderItemIds.Contains(item.Id)).ToList();

            // Set the OrderId of removed product listings to null
            foreach (var removedItem in removedOrderItems)
            {
                removedItem.OrderId = null;
                await _context.ProductListings.ReplaceOneAsync(p => p.Id == removedItem.Id, removedItem);
            }

            // Update the OrderId field for new or existing product listings in the updated order
            foreach (var updatedItem in updatedOrderItems)
            {
                updatedItem.OrderId = order.Id; // Set the orderId field
                await _context.ProductListings.ReplaceOneAsync(p => p.Id == updatedItem.Id, updatedItem);
            }

            // Update the order with the new product listings
            order.OrderItems = updatedOrderItems;

            // Calculate the new total amount
            order.TotalAmount = (decimal)updatedOrderItems.Sum(item => item.Price);

            // Save the updated order
            await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

            return Ok("Order with " + id + " updated successfully");
        }

        [HttpDelete("{id:length(24)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(string id)
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

            // Check if any product has ReadyStatus set to true, and block the deletion if so
            if (orderItems.Any(item => item.ReadyStatus == true))
            {
                return BadRequest("Cannot delete the order because one or more products have ReadyStatus set to true.");
            }

            // Set the OrderId of each product listing in the order to null
            foreach (var item in orderItems)
            {
                item.OrderId = null;
                await _context.ProductListings.ReplaceOneAsync(p => p.Id == item.Id, item);
            }

            // Delete the order from the Orders collection
            await _context.Orders.DeleteOneAsync(o => o.Id == id);

            return Ok("Order with " + id + " deleted successfully");
        }




    }
}
