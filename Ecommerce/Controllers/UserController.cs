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

        //Get all users with role as Customer
        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<User>>> GetCustomers()
        {
            return await _context.Users.Find<User>(user => user.Role == "Customer").ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(RegisterDTO registerDTO)
        {
            var existingUser = await _context.Users.Find<User>(u => u.Email == registerDTO.Email).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return BadRequest("User already exists");
            }

            if (registerDTO.Role == "Admin" || registerDTO.Role == "CSR" || registerDTO.Role == "Vendor")
            {
                var user = new User
                {
                    Name = registerDTO.Name,
                    Email = registerDTO.Email,
                    Password = registerDTO.Password,
                    Role = registerDTO.Role,
                    Active_Status = true
                };

                await _context.Users.InsertOneAsync(user);
                return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
            }

            if (registerDTO.Role == "Customer")
            {
                var user = new User
                {
                    Name = registerDTO.Name,
                    Email = registerDTO.Email,
                    Password = registerDTO.Password,
                    Role = registerDTO.Role,
                    Active_Status = false
                };

                await _context.Users.InsertOneAsync(user);
                return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
            }

            return BadRequest("Invalid Role");
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

        //change active and inactive status for Customers
        [HttpPut("status/{id:length(24)}")]
        public async Task<IActionResult> ChangeStatus(string id)
        {
            var user = _context.Users.Find<User>(user => user.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role != "Customer")
            {
                return BadRequest("User is not a customer");
            }

            if (user.Active_Status == true)
            {
                user.Active_Status = false;
                _context.Users.ReplaceOne(user => user.Id == id, user);
                return Ok("User who is a customer with id " + id + " account deactivated");
            }

            if (user.Active_Status == false)
            {
                user.Active_Status = true;
                _context.Users.ReplaceOne(user => user.Id == id, user);
                return Ok("User who is a customer with id " + id + " account activated");
            }

            return BadRequest("Invalid Request");
        }

        ////change active status to active for customers
        //[HttpPut("activate/{id:length(24)}")]
        //public async Task<IActionResult> Activate(string id)
        //{
        //    var user = _context.Users.Find<User>(user => user.Id == id).FirstOrDefault();

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    if (user.Role != "Customer")
        //    {
        //        return BadRequest("User is not a customer");
        //    }

        //    if (user.Active_Status == true)
        //    {
        //        return BadRequest("User is already active");
        //    }

        //    user.Active_Status = true;
        //    _context.Users.ReplaceOne(user => user.Id == id, user);
        //    return Ok("User who is a customer with id " + id + " account activated");
        //}

        ////change active status to inactive for customers
        //[HttpPut("deactivate/{id:length(24)}")]
        //public async Task<IActionResult> Deactivate(string id)
        //{
        //    var user = _context.Users.Find<User>(user => user.Id == id).FirstOrDefault();

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    if (user.Role != "Customer")
        //    {
        //        return BadRequest("User is not a customer");
        //    }

        //    if (user.Active_Status == false)
        //    {
        //        return BadRequest("User is already inactive");
        //    }

        //    user.Active_Status = false;
        //    _context.Users.ReplaceOne(user => user.Id == id, user);
        //    return Ok("User who is a customer with id " + id + " account deactivated");
        //}

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

            if (user.Active_Status == false)
            {
                return BadRequest("User is not active");
            }

            var loginResponse = new LoginResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Role = user.Role,
            };

            return Ok(loginResponse);
        }
    }
}
