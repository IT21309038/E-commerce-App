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
    public class RankController : ControllerBase
    {
        private readonly MongoDBContext _context;

        public RankController(MongoDBContext context)
        {
            _context = context;
        }

        //Get all ranks using DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RankGetDTO>>> Get()
        {
            var ranks = await _context.Ranks.Find(rank => true).ToListAsync();

            //Get user name and vendor name
            var userIds = ranks.Select(rank => rank.UserId).Distinct().ToList();
            var vendorIds = ranks.Select(rank => rank.VendorId).Distinct().ToList();
            var rankDTOs = ranks.Select(rank => new RankGetDTO
            {
                Id = rank.Id,
                UserId = rank.UserId,
                UserName = _context.Users.Find(user => user.Id == rank.UserId).FirstOrDefault().Name,
                Rank = rank.Rank,
                VendorId = rank.VendorId,
                VendorName = _context.Users.Find(user => user.Id == rank.VendorId).FirstOrDefault().Name
            });

            return Ok(rankDTOs);
        }

        //Get rank by id using DTO
        [HttpGet("{id:length(24)}", Name = "GetRank")]
        public async Task<ActionResult<RankGetDTO>> GetById(string id)
        {
            var rank = await _context.Ranks.Find(rank => rank.Id == id).FirstOrDefaultAsync();
            if (rank == null)
            {
                return NotFound();
            }

            return new RankGetDTO
            {
                Id = rank.Id,
                UserId = rank.UserId,
                UserName = _context.Users.Find(user => user.Id == rank.UserId).FirstOrDefault().Name,
                Rank = rank.Rank,
                VendorId = rank.VendorId,
                VendorName = _context.Users.Find(user => user.Id == rank.VendorId).FirstOrDefault().Name
            };
        }

        //Get rank by vendor id using DTO
        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<IEnumerable<RankGetDTO>>> GetByVendorId(string vendorId)
        {
            var ranks = await _context.Ranks.Find(rank => rank.VendorId == vendorId).ToListAsync();
            if (ranks == null)
            {
                return NotFound();
            }

            var rankDTOs = ranks.Select(rank => new RankGetDTO
            {
                Id = rank.Id,
                UserId = rank.UserId,
                UserName = _context.Users.Find(user => user.Id == rank.UserId).FirstOrDefault().Name,
                Rank = rank.Rank,
                VendorId = rank.VendorId,
                VendorName = _context.Users.Find(user => user.Id == rank.VendorId).FirstOrDefault().Name
            });

            return Ok(rankDTOs);
        }

        //Get average rank by vendor id using RankAvgDTO
        [HttpGet("vendor/{vendorId}/average")]
        public async Task<ActionResult<RankAvgGetDTO>> GetAverageRankByVendorId(string vendorId)
        {
            var ranks = await _context.Ranks.Find(rank => rank.VendorId == vendorId).ToListAsync();
            if (ranks == null)
            {
                return NotFound();
            }

            var rankAvgDTO = new RankAvgGetDTO
            {
                VendorId = vendorId,
                AvgRank = ranks.Average(rank => rank.Rank)
            };

            return Ok(rankAvgDTO);
        }


        [HttpPost]
        public async Task<ActionResult<Ranking>> Create(RankAddDTO rankAddDTO)
        {
            var existingRank = await _context.Ranks.Find<Ranking>(r => r.UserId == rankAddDTO.UserId && r.VendorId == rankAddDTO.VendorId).FirstOrDefaultAsync();
            if (existingRank != null)
            {
                return BadRequest("Rank already exists");
            }

            var rank = new Ranking
            {
                UserId = rankAddDTO.UserId,
                Rank = rankAddDTO.Rank,
                VendorId = rankAddDTO.VendorId
            };

            await _context.Ranks.InsertOneAsync(rank);
            return CreatedAtRoute("GetRank", new { id = rank.Id.ToString() }, rank);
        }
    }
}
