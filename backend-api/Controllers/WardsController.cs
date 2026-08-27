using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Data;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/wards
        [HttpGet]
        public async Task<IActionResult> GetWards()
        {
            var wards = await _context.Wards.ToListAsync();
            return Ok(wards);
        }
    }
}