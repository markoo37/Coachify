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

        // GET: api/trainingplans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetTrainingPlans()
        {
            int userId = User.GetUserId();

            // Only plans for athletes in teams coached by current user
            var plans = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.TeamMemberships)
                        .ThenInclude(tm => tm.Team)
                            .ThenInclude(t => t.Coach)
                .Where(tp => tp.Athlete.TeamMemberships
                    .Any(tm => tm.Team.Coach.UserId == userId))
                .ToListAsync();

            var dtoList = plans.Select(tp => tp.ToDto()).ToList();
            return Ok(dtoList);
        }

        // GET: api/trainingplans/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingPlanDto>> GetTrainingPlan(int id)
        {
            int userId = User.GetUserId();

            var plan = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.TeamMemberships)
                        .ThenInclude(tm => tm.Team)
                            .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(tp => tp.Id == id &&
                    tp.Athlete.TeamMemberships
                        .Any(tm => tm.Team.Coach.UserId == userId));

            if (plan == null)
                return NotFound();

            return Ok(plan.ToDto());
        }

        // POST: api/trainingplans
        [HttpPost]
        public async Task<ActionResult<TrainingPlanDto>> PostTrainingPlan(CreateTrainingPlanDto dto)
        {
            int userId = User.GetUserId();

            // Verify athlete belongs to this coach
            var athlete = await _context.Athletes
                .Include(a => a.TeamMemberships)
                    .ThenInclude(tm => tm.Team)
                        .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(a => a.Id == dto.AthleteId &&
                    a.TeamMemberships.Any(tm => tm.Team.Coach.UserId == userId));

            if (athlete == null)
                return BadRequest("Invalid athlete for current user.");

            var plan = new TrainingPlan
            {
                Name       = dto.Name,
                Description= dto.Description,
                StartDate  = dto.StartDate,
                EndDate    = dto.EndDate,
                AthleteId  = athlete.Id,
                TeamId     = null // optional: remove TeamId from plan or handle via TeamMembership
            };

            _context.TrainingPlans.Add(plan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrainingPlan), new { id = plan.Id }, plan.ToDto());
        }

        // PUT: api/trainingplans/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTrainingPlan(int id, CreateTrainingPlanDto dto)
        {
            int userId = User.GetUserId();

            var plan = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.TeamMemberships)
                        .ThenInclude(tm => tm.Team)
                            .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(tp => tp.Id == id &&
                    tp.Athlete.TeamMemberships
                        .Any(tm => tm.Team.Coach.UserId == userId));

            if (plan == null)
                return NotFound();

            plan.Name        = dto.Name;
            plan.Description = dto.Description;
            plan.StartDate   = dto.StartDate;
            plan.EndDate     = dto.EndDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/trainingplans/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainingPlan(int id)
        {
            int userId = User.GetUserId();

            var plan = await _context.TrainingPlans
                .Include(tp => tp.Athlete)
                    .ThenInclude(a => a.TeamMemberships)
                        .ThenInclude(tm => tm.Team)
                            .ThenInclude(t => t.Coach)
                .FirstOrDefaultAsync(tp => tp.Id == id &&
                    tp.Athlete.TeamMemberships
                        .Any(tm => tm.Team.Coach.UserId == userId));

            if (plan == null)
                return NotFound();

            _context.TrainingPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
