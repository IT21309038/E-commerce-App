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
    public class ReviewController : ControllerBase
    {
        public readonly MongoDBContext _context;

        public ReviewController(MongoDBContext context)
        {
            _context = context;
        }

        //Get all reviews using DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewGetDTO>>> Get()
        {
            var reviews = await _context.Reviews.Find(review => true).ToListAsync();

            //Get user name and vendor name
            var userIds = reviews.Select(review => review.UserId).Distinct().ToList();
            var vendorIds = reviews.Select(review => review.VendorId).Distinct().ToList();
            var reviewDTOs = reviews.Select(review => new ReviewGetDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = _context.Users.Find(user => user.Id == review.UserId).FirstOrDefault().Name,
                ReviewNote = review.ReviewNote,
                VendorId = review.VendorId,
                VendorName = _context.Users.Find(user => user.Id == review.VendorId).FirstOrDefault().Name
            });

            return Ok(reviewDTOs);
        }

        //Get review by id using DTO
        [HttpGet("{id:length(24)}", Name = "GetReview")]
        public async Task<ActionResult<ReviewGetDTO>> GetById(string id)
        {
            var review = await _context.Reviews.Find(review => review.Id == id).FirstOrDefaultAsync();
            if (review == null)
            {
                return NotFound();
            }

            return new ReviewGetDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = _context.Users.Find(user => user.Id == review.UserId).FirstOrDefault().Name,
                ReviewNote = review.ReviewNote,
                VendorId = review.VendorId,
                VendorName = _context.Users.Find(user => user.Id == review.VendorId).FirstOrDefault().Name
            };
        }

        //Get review by vendor id using DTO
        [HttpGet("vendor/{id:length(24)}", Name = "GetReviewByVendor")]
        public async Task<ActionResult<IEnumerable<ReviewGetDTO>>> GetByVendorId(string id)
        {
            var reviews = await _context.Reviews.Find(review => review.VendorId == id).ToListAsync();
            if (reviews == null)
            {
                return NotFound();
            }

            //Get user name and vendor name
            var userIds = reviews.Select(review => review.UserId).Distinct().ToList();
            var vendorIds = reviews.Select(review => review.VendorId).Distinct().ToList();
            var reviewDTOs = reviews.Select(review => new ReviewGetDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = _context.Users.Find(user => user.Id == review.UserId).FirstOrDefault().Name,
                ReviewNote = review.ReviewNote,
                VendorId = review.VendorId,
                VendorName = _context.Users.Find(user => user.Id == review.VendorId).FirstOrDefault().Name
            });

            return Ok(reviewDTOs);
        }

        //Get review by user id and vendor id using DTO
        [HttpGet("user/{userId:length(24)}/vendor/{vendorId:length(24)}", Name = "GetReviewByUserAndVendor")]
        public async Task<ActionResult<ReviewGetDTO>> GetByUserAndVendorId(string userId, string vendorId)
        {
            var review = await _context.Reviews.Find(review => review.UserId == userId && review.VendorId == vendorId).FirstOrDefaultAsync();
            if (review == null)
            {
                return NotFound();
            }

            return new ReviewGetDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = _context.Users.Find(user => user.Id == review.UserId).FirstOrDefault().Name,
                ReviewNote = review.ReviewNote,
                VendorId = review.VendorId,
                VendorName = _context.Users.Find(user => user.Id == review.VendorId).FirstOrDefault().Name
            };
        }


        //Add review using DTO
        [HttpPost]
        public async Task<ActionResult<Review>> Add(ReviewAddDTO reviewDTO)
        {
            //only a user can review a vendor once
            var reviewExist = await _context.Reviews.Find(review => review.UserId == reviewDTO.UserId && review.VendorId == reviewDTO.VendorId).FirstOrDefaultAsync();
            
            if (reviewExist != null)
            {
                return BadRequest("You have already reviewed this vendor");
            }

            var review = new Review
            {
                UserId = reviewDTO.UserId,
                ReviewNote = reviewDTO.ReviewNote,
                VendorId = reviewDTO.VendorId
            };

            await _context.Reviews.InsertOneAsync(review);
            return CreatedAtRoute("GetReview", new { id = review.Id.ToString() }, review);
        }

        //Update review using UpdateDTO
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, ReviewUpdateDTO reviewDTO)
        {
            var review = await _context.Reviews.Find(review => review.Id == id).FirstOrDefaultAsync();
            if (review == null)
            {
                return NotFound();
            }

            review.ReviewNote = reviewDTO.ReviewNote;

            await _context.Reviews.ReplaceOneAsync(review => review.Id == id, review);
            return Ok("Review with " + id + " updated sucessfully");
        }
    }
}
