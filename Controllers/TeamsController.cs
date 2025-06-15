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
            var teams = await _context.Teams.ToListAsync();
            return teams.Select(t => t.ToDto()).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<TeamDto>> PostTeam(CreateTeamDto dto)
        {
            var team = new Team
            {
                Name = dto.Name,
                CoachId = dto.CoachId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeams), new { id = team.Id }, team.ToDto());
        }
    }
}