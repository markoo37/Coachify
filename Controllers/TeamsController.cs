using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachCRM.Data;
using CoachCRM.Models;
using CoachCRM.Dtos;
using CoachCRM.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace CoachCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeamsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
        {
            int userId = User.GetUserId();

            var coachId = await _context.Coaches
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            var teams = await _context.Teams
                .Where(t => t.CoachId == coachId)
                .ToListAsync();

            var dtoList = teams.Select(t => t.ToDto()).ToList();

            return Ok(dtoList);
        }

        [HttpPost]
        public async Task<ActionResult<TeamDto>> PostTeam(CreateTeamDto dto)
        {
            int userId = User.GetUserId();

            var coach = await _context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (coach == null)
            {
                return BadRequest("Coach not found.");
            }

            var team = new Team
            {
                Name = dto.Name,
                CoachId = coach.Id
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeams), new { id = team.Id }, team.ToDto());
        }
    }
}