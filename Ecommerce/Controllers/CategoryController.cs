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
    public class CategoryController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public CategoryController(MongoDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> Get()
        {
            return await _context.Categories.Find(category => true).ToListAsync();
        }

        [HttpGet("{id:length(24)}", Name = "GetCategory")]
        public async Task<ActionResult<Category>> Get(string id)
        {
            var category = await _context.Categories.Find<Category>(category => category.Id == id).FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create(CategoryDTO categoryDTO)
        {
            var existingCategory = await _context.Categories.Find<Category>(c => c.CategoryName == categoryDTO.CategoryName).FirstOrDefaultAsync();
            if (existingCategory != null)
            {
                return BadRequest("Category already exists");
            }

            var category = new Category
            {
                CategoryName = categoryDTO.CategoryName,
                ActiveStatus = true
            };

            await _context.Categories.InsertOneAsync(category);
            return CreatedAtRoute("GetCategory", new { id = category.Id.ToString() }, category);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, CategoryDTO categoryDTO)
        {
            var category = _context.Categories.Find<Category>(category => category.Id == id).FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = categoryDTO.CategoryName;

            await _context.Categories.ReplaceOneAsync(c => c.Id == id, category);
            return Ok();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = _context.Categories.Find<Category>(category => category.Id == id).FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.DeleteOne(category => category.Id == id);
            return NoContent();
        }

        //api to disable active status
        [HttpPut("disable/{id:length(24)}")]
        public async Task<IActionResult> Disable(string id)
        {
            var category = _context.Categories.Find<Category>(category => category.Id == id).FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            if (category.ActiveStatus == false)
            {
                return BadRequest("Category with " + id + " already inactive");
            };

            category.ActiveStatus = false;

            await _context.Categories.ReplaceOneAsync(c => c.Id == id, category);
            return Ok("Category with " + id + " diabled");
        }

        //api to enable active status
        [HttpPut("enable/{id:length(24)}")]
        public async Task<IActionResult> Enable(string id)
        {
            var category = _context.Categories.Find<Category>(category => category.Id == id).FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            if (category.ActiveStatus == true)
            {
                return BadRequest("Category with " + id + " already active");
            };

            category.ActiveStatus = true;

            await _context.Categories.ReplaceOneAsync(c => c.Id == id, category);
            return Ok("Category with " + id + " enabled");
        }
    }
}
