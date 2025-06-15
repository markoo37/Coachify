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

        var athletes = await _context.Athletes
            .Include(a => a.Team)
            .Where(a => a.Team.Coach.UserId == userId)
            .ToListAsync();

        var dtoList = athletes.Select(a => a.ToDto()).ToList();

        return Ok(dtoList);
    }

    // GET: api/athletes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AthleteDto>> GetAthlete(int id)
    {
        int userId = User.GetUserId();

        var athlete = await _context.Athletes
            .Include(a => a.Team)
            .ThenInclude(t => t.Coach)
            .FirstOrDefaultAsync(a => a.Id == id && a.Team.Coach.UserId == userId);

        if (athlete == null)
        {
            return NotFound();
        }

        return Ok(athlete.ToDto());
    }

    // POST: api/athletes
    [HttpPost]
    public async Task<ActionResult<AthleteDto>> PostAthlete(CreateAthleteDto dto)
    {
        int userId = User.GetUserId();

        // Ellenőrizzük, hogy a megadott team a bejelentkezett userhez tartozik-e
        var team = await _context.Teams
            .Include(t => t.Coach)
            .FirstOrDefaultAsync(t => t.Id == dto.TeamId && t.Coach.UserId == userId);

        if (team == null)
        {
            return BadRequest("Invalid team for current user.");
        }

        var athlete = new Athlete
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            BirthDate = dto.BirthDate,
            Weight = dto.Weight,
            Height = dto.Height,
            TeamId = team.Id
        };

        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAthlete), new { id = athlete.Id }, athlete.ToDto());
    }

    // PUT: api/athletes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAthlete(int id, CreateAthleteDto dto)
    {
        int userId = User.GetUserId();

        var athlete = await _context.Athletes
            .Include(a => a.Team)
            .ThenInclude(t => t.Coach)
            .FirstOrDefaultAsync(a => a.Id == id && a.Team.Coach.UserId == userId);

        if (athlete == null)
        {
            return NotFound();
        }

        athlete.FirstName = dto.FirstName;
        athlete.LastName = dto.LastName;
        athlete.BirthDate = dto.BirthDate;
        athlete.Weight = dto.Weight;
        athlete.Height = dto.Height;
        athlete.TeamId = dto.TeamId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/athletes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAthlete(int id)
    {
        int userId = User.GetUserId();

        var athlete = await _context.Athletes
            .Include(a => a.Team)
            .ThenInclude(t => t.Coach)
            .FirstOrDefaultAsync(a => a.Id == id && a.Team.Coach.UserId == userId);

        if (athlete == null)
        {
            return NotFound();
        }

        _context.Athletes.Remove(athlete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
