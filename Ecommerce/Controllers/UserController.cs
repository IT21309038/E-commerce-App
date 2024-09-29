using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Models;
using Ecommerce.Data;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.DTO;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public UserController(MongoDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            return await _context.Users.Find(user => true).ToListAsync();
        }

        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var user = await _context.Users.Find<User>(user => user.Id == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(User user)
        {
            _context.Users.InsertOne(user);
            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, User userIn)
        {
            var user = _context.Users.Find<User>(user => user.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.ReplaceOne(user => user.Id == id, userIn);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = _context.Users.Find<User>(user => user.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.DeleteOne(user => user.Id == id);
            return NoContent();
        }

        //only username and password is provided in request body
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users.Find<User>(user => user.Email == loginRequest.Email && user.Password == loginRequest.Password).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
    }
}
