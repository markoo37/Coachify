using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachCRM.Models;
using CoachCRM.Data;
using CoachCRM.Extensions;
using CoachCRM.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace CoachCRM.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AthletesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AthletesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/athletes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AthleteDto>>> GetAthletes()
    {
        int userId = User.GetUserId();

        var memberships = await _context.TeamMemberships
            .Include(tm => tm.Athlete)
                .ThenInclude(a => a.User)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Coach)
            .Where(tm => tm.Team.Coach.UserId == userId)
            .ToListAsync();

        var athletes = memberships
            .Select(tm => tm.Athlete)
            .Distinct();

        var dtoList = athletes
            .Select(a => a.ToDto())
            .ToList();

        return Ok(dtoList);
    }

    // GET: api/athletes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AthleteDto>> GetAthlete(int id)
    {
        int userId = User.GetUserId();

        var membership = await _context.TeamMemberships
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Coach)
            .Include(tm => tm.Athlete)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(tm => tm.Athlete.Id == id && tm.Team.Coach.UserId == userId);

        if (membership == null)
        {
            return NotFound();
        }

        return Ok(membership.Athlete.ToDto());
    }

    // POST: api/athletes
    // CoachCRM/Controllers/AthletesController.cs

    [HttpPost]
    public async Task<ActionResult<AthleteDto>> PostAthlete(CreateAthleteDto dto)
    {
        int userId = User.GetUserId();
        var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
        if (coach == null) return Unauthorized();

        var team = await _context.Teams
            .FirstOrDefaultAsync(t => t.Id == dto.TeamId && t.CoachId == coach.Id);
        if (team == null) return BadRequest("Invalid team.");

        var athlete = new Athlete
        {
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            BirthDate = dto.BirthDate,
            Weight    = dto.Weight,
            Height    = dto.Height,
            Email     = dto.Email   // ide kerüljön az email
        };
        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync();

        var membership = new TeamMembership
        {
            AthleteId = athlete.Id,
            TeamId    = team.Id
        };
        _context.TeamMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAthlete), new { id = athlete.Id }, athlete.ToDto());
    }


    // PUT: api/athletes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAthlete(int id, CreateAthleteDto dto)
    {
        int userId = User.GetUserId();

        var membership = await _context.TeamMemberships
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Coach)
            .Include(tm => tm.Athlete)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(tm => tm.Athlete.Id == id && tm.Team.Coach.UserId == userId);

        if (membership == null)
        {
            return NotFound();
        }

        var athlete = membership.Athlete;
        athlete.FirstName = dto.FirstName;
        athlete.LastName  = dto.LastName;
        athlete.BirthDate = dto.BirthDate;
        athlete.Weight    = dto.Weight;
        athlete.Height    = dto.Height;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/athletes/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAthlete(int id)
    {
        int userId = User.GetUserId();

        var athlete = await _context.Athletes
            .Include(a => a.TeamMemberships)
                .ThenInclude(tm => tm.Team)
                    .ThenInclude(t => t.Coach)
            .FirstOrDefaultAsync(a => a.Id == id && a.TeamMemberships.Any(tm => tm.Team.Coach.UserId == userId));

        if (athlete == null)
        {
            return NotFound();
        }

        _context.Athletes.Remove(athlete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
