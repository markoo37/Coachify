using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachCRM.Data;
using CoachCRM.Models;
using CoachCRM.Extensions;
using CoachCRM.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace CoachCRM.Controllers
{
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetTrainingPlans()
        {
            int userId = User.GetUserId();

            var plans = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.Team)
                    .ThenInclude(t => t.Coach)
                .Where(tp => tp.Athlete.Team.Coach.UserId == userId)
                .ToListAsync();

            var dtoList = plans.Select(tp => tp.ToDto()).ToList();

            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingPlanDto>> GetTrainingPlan(int id)
        {
            int userId = User.GetUserId();

            var plan = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.Team)
                    .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(tp => tp.Id == id && tp.Athlete.Team.Coach.UserId == userId);

            if (plan == null)
            {
                return NotFound();
            }

            return Ok(plan.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<TrainingPlanDto>> PostTrainingPlan(CreateTrainingPlanDto dto)
        {
            int userId = User.GetUserId();

            // Ellenőrizzük, hogy az Athlete hozzá tartozik-e a bejelentkezett coachhoz
            var athlete = await _context.Athletes
                .Include(a => a.Team)
                    .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(a => a.Id == dto.AthleteId && a.Team.Coach.UserId == userId);

            if (athlete == null)
            {
                return BadRequest("Invalid athlete for current user.");
            }

            var plan = new TrainingPlan
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                AthleteId = athlete.Id,
                TeamId = athlete.TeamId
            };

            _context.TrainingPlans.Add(plan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrainingPlan), new { id = plan.Id }, plan.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTrainingPlan(int id, CreateTrainingPlanDto dto)
        {
            int userId = User.GetUserId();

            var plan = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.Team)
                    .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(tp => tp.Id == id && tp.Athlete.Team.Coach.UserId == userId);

            if (plan == null)
            {
                return NotFound();
            }

            plan.Name = dto.Name;
            plan.Description = dto.Description;
            plan.StartDate = dto.StartDate;
            plan.EndDate = dto.EndDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainingPlan(int id)
        {
            int userId = User.GetUserId();

            var plan = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.Team)
                    .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(tp => tp.Id == id && tp.Athlete.Team.Coach.UserId == userId);

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
