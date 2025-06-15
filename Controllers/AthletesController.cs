using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachCRM.Models;
using CoachCRM.Data;
using CoachCRM.Extensions;
using CoachCRM.Dtos;

namespace CoachCRM.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AthletesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AthletesController(AppDbContext context)
    {
        _context = context;
    }
    
    //GET api/athletes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Athlete>>> GetAthletes()
    {
        return await _context.Athletes.ToListAsync();
    }
    
    //GET api/athletes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Athlete>> GetAthlete(int id)
    {
        var athlete = await _context.Athletes.FindAsync(id);
        if (athlete == null)
        {
            return NotFound();
        }
        return athlete;
    }
    
    //POST api/athletes
    [HttpPost]
    public async Task<ActionResult<AthleteDto>> PostAthlete(CreateAthleteDto dto)
    {
        var athlete = new Athlete
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            BirthDate = dto.BirthDate,
            Weight = dto.Weight,
            Height = dto.Height,
            TeamId = dto.TeamId
        };

        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAthlete), new { id = athlete.Id }, athlete.ToDto());
    }

    
    // PUT: api/athletes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAthlete(int id, Athlete athlete)
    {
        if (id != athlete.Id)
        {
            return BadRequest();
        }

        _context.Entry(athlete).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/athletes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAthlete(int id)
    {
        var athlete = await _context.Athletes.FindAsync(id);
        if (athlete == null)
        {
            return NotFound();
        }

        _context.Athletes.Remove(athlete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}