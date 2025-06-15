using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachCRM.Data;
using CoachCRM.Models;
using CoachCRM.Extensions;
using CoachCRM.Dtos;

namespace CoachCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingPlansController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TrainingPlansController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingPlan>>> GetTrainingPlans()
        {
            return await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingPlan>> GetTrainingPlan(int id)
        {
            var plan = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                .FirstOrDefaultAsync(tp => tp.Id == id);

            if (plan == null)
            {
                return NotFound();
            }

            return plan;
        }

        [HttpPost]
        public async Task<ActionResult<TrainingPlanDto>> PostTrainingPlan(CreateTrainingPlanDto dto)
        {
            var plan = new TrainingPlan
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                AthleteId = dto.AthleteId,
                TeamId = dto.TeamId
            };

            _context.TrainingPlans.Add(plan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrainingPlan), new { id = plan.Id }, plan.ToDto());
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutTrainingPlan(int id, TrainingPlan plan)
        {
            if (id != plan.Id)
            {
                return BadRequest();
            }

            _context.Entry(plan).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainingPlan(int id)
        {
            var plan = await _context.TrainingPlans.FindAsync(id);
            if (plan == null)
            {
                return NotFound();
            }

            _context.TrainingPlans.Remove(plan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
