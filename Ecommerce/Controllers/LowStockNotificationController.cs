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
    public class LowStockNotificationController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public LowStockNotificationController(MongoDBContext context)
        {
            _context = context;
        }

        //Get notification by vendor id
        [HttpGet("{id:length(24)}", Name = "GetOrderNotification")]
        public async Task<ActionResult<NotificationLowStock>> GetNotification(string id)
        {
            var notification = await _context.NotificationLowStock.Find(n => n.VendorId == id).FirstOrDefaultAsync();

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        //Get all notifications by vendor id
        [HttpGet("all/{id:length(24)}")]
        public async Task<ActionResult<List<NotificationLowStock>>> GetAllNotifications(string id)
        {
            var notifications = await _context.NotificationLowStock.Find(n => n.VendorId == id).ToListAsync();

            if (notifications == null)
            {
                return NotFound();
            }

            return notifications;
        }

        //Mark notification as read using notification id
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> MarkAsRead(string id, NotificationLowStock notification)
        {
            var notificationToUpdate = await _context.NotificationLowStock.Find(n => n.Id == id).FirstOrDefaultAsync();

            if (notificationToUpdate == null)
            {
                return NotFound();
            }

            notificationToUpdate.MarkRead = true;

            await _context.NotificationLowStock.ReplaceOneAsync(n => n.Id == id, notificationToUpdate);

            return Ok("Notification with id " + id + " marked as read");
        }
    }
}
