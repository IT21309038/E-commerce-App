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
    public class NotificationOrderController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public NotificationOrderController(MongoDBContext context)
        {
            _context = context;
        }

        //Get notification by user id
        [HttpGet("{id:length(24)}", Name = "GetNotification")]
        public async Task<ActionResult<NotificationOrderCancel>> GetNotification(string id)
        {
            var notification = await _context.NotificationOrderCancel.Find(n => n.UserId == id).FirstOrDefaultAsync();

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        //Get all notifications by user id
        [HttpGet("all/{id:length(24)}")]
        public async Task<ActionResult<List<NotificationOrderCancel>>> GetAllNotifications(string id)
        {
            var notifications = await _context.NotificationOrderCancel.Find(n => n.UserId == id).ToListAsync();

            if (notifications == null)
            {
                return NotFound();
            }

            return notifications;
        }

        //Mark notification as read using notification id
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> MarkAsRead(string id, NotificationOrderCancel notification)
        {
            var notificationToUpdate = await _context.NotificationOrderCancel.Find(n => n.Id == id).FirstOrDefaultAsync();

            if (notificationToUpdate == null)
            {
                return NotFound();
            }

            notificationToUpdate.MarkRead = true;

            await _context.NotificationOrderCancel.ReplaceOneAsync(n => n.Id == id, notificationToUpdate);

            return Ok("Notification with id " + id + " marked as read");
        }
    }
}
