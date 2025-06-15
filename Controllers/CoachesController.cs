using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachCRM.Data;
using CoachCRM.Models;
using CoachCRM.Dtos;
using CoachCRM.Extensions;

namespace CoachCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoachesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoachesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Coach>>> GetCoaches()
        {
            return await _context.Coaches
                .Include(c => c.Teams)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Coach>> GetCoach(int id)
        {
            var coach = await _context.Coaches
                .Include(c => c.Teams)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }

            return coach;
        }

        [HttpPost]
        public async Task<ActionResult<CoachDto>> PostCoach(CreateCoachDto dto)
        {
            var coach = new Coach
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };

            _context.Coaches.Add(coach);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCoach), new { id = coach.Id }, coach.ToDto());
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutCoach(int id, Coach coach)
        {
            if (id != coach.Id)
            {
                return BadRequest();
            }

            _context.Entry(coach).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoach(int id)
        {
            var coach = await _context.Coaches.FindAsync(id);
            if (coach == null)
            {
                return NotFound();
            }

            _context.Coaches.Remove(coach);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
