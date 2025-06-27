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

    // ═════════════════════════════════════════════════════════════════
    // COACH ENDPOINTS (módosított funkciók)
    // ═════════════════════════════════════════════════════════════════

    // GET: api/athletes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AthleteDto>>> GetAthletes()
    {
        int userId = User.GetUserId();

        // Az edző összes sportolója (minden csapatából, beleértve a rejtett "_Unassigned" csapatot is)
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

    // GET: api/athletes/available-for-team/{teamId}
    [HttpGet("available-for-team/{teamId}")]
    public async Task<ActionResult<IEnumerable<AthleteDto>>> GetAvailableAthletesForTeam(int teamId)
    {
        int userId = User.GetUserId();
        var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
        if (coach == null) return Unauthorized();

        // Ellenőrizzük, hogy a csapat az edzőé-e
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.CoachId == coach.Id);
        if (team == null) return BadRequest("Invalid team.");

        // Az edző összes sportolója, akik NINCSENEK ebben a konkrét csapatban
        // (beleértve azokat is, akik a "_Unassigned" csapatban vannak vagy más csapatokban)
        var availableAthletes = await _context.TeamMemberships
            .Include(tm => tm.Athlete)
                .ThenInclude(a => a.User)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Coach)
            .Where(tm => tm.Team.Coach.UserId == userId && tm.TeamId != teamId)
            .Select(tm => tm.Athlete)
            .Distinct()
            .ToListAsync();

        var dtoList = availableAthletes
            .Select(a => a.ToDto())
            .ToList();

        return Ok(dtoList);
    }

    // POST: api/athletes/{athleteId}/assign-to-team/{teamId}
    [HttpPost("{athleteId}/assign-to-team/{teamId}")]
    public async Task<IActionResult> AssignAthleteToTeam(int athleteId, int teamId)
    {
        int userId = User.GetUserId();
        var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
        if (coach == null) return Unauthorized();

        // Ellenőrizzük a csapatot
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.CoachId == coach.Id);
        if (team == null) return BadRequest("Invalid team.");

        // Ellenőrizzük, hogy a sportoló az edző sportolója-e (bármely csapatában van)
        var isCoachAthlete = await _context.TeamMemberships
            .AnyAsync(tm => tm.AthleteId == athleteId && tm.Team.Coach.UserId == userId);
        
        if (!isCoachAthlete) return BadRequest("Invalid athlete.");

        // Ellenőrizzük, hogy már tag-e ebben a csapatban
        var existingMembership = await _context.TeamMemberships
            .FirstOrDefaultAsync(tm => tm.AthleteId == athleteId && tm.TeamId == teamId);
        if (existingMembership != null) return BadRequest("Athlete is already a member of this team.");

        // Hozzáadjuk a csapathoz
        var membership = new TeamMembership
        {
            AthleteId = athleteId,
            TeamId = teamId
        };
        _context.TeamMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // POST: api/athletes/{athleteId}/remove-from-team/{teamId}
    [HttpPost("{athleteId}/remove-from-team/{teamId}")]
    public async Task<IActionResult> RemoveAthleteFromTeam(int athleteId, int teamId)
    {
        int userId = User.GetUserId();
        var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
        if (coach == null) return Unauthorized();

        // Ellenőrizzük a csapatot
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.CoachId == coach.Id);
        if (team == null) return BadRequest("Invalid team.");

        // Keressük meg a membership-et
        var membership = await _context.TeamMemberships
            .FirstOrDefaultAsync(tm => tm.AthleteId == athleteId && tm.TeamId == teamId);
        
        if (membership == null) return NotFound("Membership not found.");

        // Töröljük csak a membership-et, nem a sportolót
        _context.TeamMemberships.Remove(membership);

        // Ha a sportoló már nincs egyetlen csapatban sem (kivéve "_Unassigned"), 
        // akkor tegyük vissza az "_Unassigned" csapatba
        var remainingMemberships = await _context.TeamMemberships
            .Include(tm => tm.Team)
            .Where(tm => tm.AthleteId == athleteId && tm.Team.Name != "_Unassigned")
            .CountAsync();

        if (remainingMemberships == 1) // Ez az egy membership amit most törlünk
        {
            // Keressük meg vagy hozzuk létre az "_Unassigned" csapatot
            var unassignedTeam = await _context.Teams
                .FirstOrDefaultAsync(t => t.CoachId == coach.Id && t.Name == "_Unassigned");
            
            if (unassignedTeam == null)
            {
                unassignedTeam = new Team
                {
                    Name = "_Unassigned",
                    CoachId = coach.Id
                };
                _context.Teams.Add(unassignedTeam);
                await _context.SaveChangesAsync();
            }

            // Hozzáadjuk az "_Unassigned" csapathoz
            var unassignedMembership = new TeamMembership
            {
                AthleteId = athleteId,
                TeamId = unassignedTeam.Id
            };
            _context.TeamMemberships.Add(unassignedMembership);
        }

        await _context.SaveChangesAsync();
        return Ok();
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
    [HttpPost]
    public async Task<ActionResult<AthleteDto>> PostAthlete(CreateAthleteDto dto)
    {
        int userId = User.GetUserId();
        var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
        if (coach == null) return Unauthorized();

        Team? targetTeam = null;
        
        if (dto.TeamId.HasValue)
        {
            // Konkrét csapat megadva
            targetTeam = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId.Value && t.CoachId == coach.Id);
            if (targetTeam == null) return BadRequest("Invalid team.");
        }
        else
        {
            // Nincs csapat megadva - használjuk a rejtett "_Unassigned" csapatot
            targetTeam = await _context.Teams
                .FirstOrDefaultAsync(t => t.CoachId == coach.Id && t.Name == "_Unassigned");
            
            // Ha nincs "_Unassigned" csapat, létrehozzuk
            if (targetTeam == null)
            {
                targetTeam = new Team
                {
                    Name = "_Unassigned",
                    CoachId = coach.Id
                };
                _context.Teams.Add(targetTeam);
                await _context.SaveChangesAsync();
            }
        }

        var athlete = new Athlete
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            BirthDate = dto.BirthDate,
            Weight = dto.Weight,
            Height = dto.Height,
            Email = dto.Email
        };
        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync();

        // Hozzáadjuk a célcsapathoz (lehet "_Unassigned" vagy konkrét csapat)
        var membership = new TeamMembership
        {
            AthleteId = athlete.Id,
            TeamId = targetTeam.Id
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
        athlete.LastName = dto.LastName;
        athlete.BirthDate = dto.BirthDate;
        athlete.Weight = dto.Weight;
        athlete.Height = dto.Height;
        athlete.Email = dto.Email;

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

    // ═════════════════════════════════════════════════════════════════
    // PLAYER ENDPOINTS (meglévő funkciók)
    // ═════════════════════════════════════════════════════════════════

    // GET: api/athletes/my-profile
    [HttpGet("my-profile")]
    public async Task<ActionResult<PlayerProfileDto>> GetMyProfile()
    {
        int userId = User.GetUserId();

        var playerUser = await _context.PlayerUsers
            .Include(pu => pu.Athlete)
                .ThenInclude(a => a.TeamMemberships)
                    .ThenInclude(tm => tm.Team)
                        .ThenInclude(t => t.Coach)
            .FirstOrDefaultAsync(pu => pu.AthleteId == userId);

        if (playerUser?.Athlete == null)
            return NotFound("Player profile not found.");

        var athlete = playerUser.Athlete;
        
        // Calculate age
        int? age = null;
        if (athlete.BirthDate.HasValue)
        {
            age = DateTime.Today.Year - athlete.BirthDate.Value.Year;
            if (athlete.BirthDate.Value.Date > DateTime.Today.AddYears(-age.Value))
                age--;
        }

        var teams = athlete.TeamMemberships.Select(tm => new TeamInfoDto
        {
            Id = tm.Team.Id,
            Name = tm.Team.Name,
            Coach = new CoachInfoDto
            {
                Id = tm.Team.Coach.Id,
                FirstName = tm.Team.Coach.FirstName,
                LastName = tm.Team.Coach.LastName,
                Email = tm.Team.Coach.Email
            },
            PlayerCount = tm.Team.TeamMemberships.Count
        }).ToList();

        var profile = new PlayerProfileDto
        {
            Id = athlete.Id,
            FirstName = athlete.FirstName,
            LastName = athlete.LastName,
            Email = athlete.Email,
            BirthDate = athlete.BirthDate,
            Weight = athlete.Weight,
            Height = athlete.Height,
            Age = age,
            Teams = teams,
            HasUserAccount = true
        };

        return Ok(profile);
    }

    // GET: api/athletes/my-teams
    [HttpGet("my-teams")]
    public async Task<ActionResult<IEnumerable<TeamInfoDto>>> GetMyTeams()
    {
        // 1) A User.GetUserId() most a PlayerUser.Id-t adja vissza (nem az Athlete.Id-t)
        int userId = User.GetUserId();

        // 2) Lekérjük a PlayerUser-t a PlayerUsers DbSet-ből a userId alapján
        var playerUser = await _context.PlayerUsers
            .Include(pu => pu.Athlete)
            .ThenInclude(a => a.TeamMemberships)
            .ThenInclude(tm => tm.Team)
            .ThenInclude(t => t.Coach)
            .FirstOrDefaultAsync(pu => pu.Id == userId);  // <— korábban pu.AthleteId == userId volt

        if (playerUser?.Athlete == null)
            return NotFound("Player not found.");

        // 3) Innen állítjuk össze a TeamInfoDto-k listáját
        var teams = playerUser.Athlete.TeamMemberships.Select(tm => new TeamInfoDto
        {
            Id = tm.Team.Id,
            Name = tm.Team.Name,
            Coach = new CoachInfoDto
            {
                Id = tm.Team.Coach.Id,
                FirstName = tm.Team.Coach.FirstName,
                LastName = tm.Team.Coach.LastName,
                Email = tm.Team.Coach.Email
            },
            PlayerCount = tm.Team.TeamMemberships.Count
        }).ToList();

        return Ok(teams);
    }

    // GET: api/athletes/my-training-plans
    [HttpGet("my-training-plans")]
    public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetMyTrainingPlans()
    {
        // ───────────────────────────────────────────────
        // 1) A JWT-ből kinyerjük a PlayerUser.Id-t, nem az AthleteId-t
        int userId = User.GetUserId();

        // 2) Betöltjük a PlayerUser-t, beletekintve az Athlete-be
        var playerUser = await _context.PlayerUsers
            .Include(pu => pu.Athlete)
            .FirstOrDefaultAsync(pu => pu.Id == userId);

        if (playerUser?.Athlete == null)
            return NotFound("Player not found.");

        var athleteId = playerUser.Athlete.Id;
        // ───────────────────────────────────────────────

        // 3) Összerakjuk a lekérdezést: 
        //    - vagy az atlétához rendelt tervek,
        //    - vagy az atléta csapataira vonatkozó tervek
        var query = _context.TrainingPlans
            .Include(tp => tp.Team)
            .Include(tp => tp.Athlete)
            .Where(tp =>
                tp.AthleteId == athleteId
                || (tp.TeamId != null && _context.TeamMemberships
                    .Any(tm => tm.TeamId == tp.TeamId && tm.AthleteId == athleteId))
            )
            // 4) Először a dátum szerinti rendezés
            .OrderBy(tp => tp.Date);

        // 5) Majd a kezdési idő szerinti rendezés
        //    Itt explicit a Queryable.ThenBy, hogy a compiler ne keverje az
        //    Enumerable és a Queryable overloadokat
        query = System.Linq.Queryable.ThenBy<TrainingPlan, TimeOnly?>(
            query,
            tp => tp.StartTime
        );

        // 6) Lekérjük a listát az adatbázisból
        var trainingPlans = await query.ToListAsync();

        // 7) DTO-vá alakítás
        var dtoList = trainingPlans
            .Select(tp => tp.ToDto())
            .ToList();

        return Ok(dtoList);
    }

    // GET: api/athletes/teams/{teamId}/training-plans
    [HttpGet("teams/{teamId}/training-plans")]
    public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetTeamTrainingPlans(int teamId)
    {
        int userId = User.GetUserId();

        var playerUser = await _context.PlayerUsers
            .Include(pu => pu.Athlete)
                .ThenInclude(a => a.TeamMemberships)
            .FirstOrDefaultAsync(pu => pu.AthleteId == userId);

        if (playerUser?.Athlete == null)
            return NotFound("Player not found.");

        // Check if player is member of this team
        var isMember = playerUser.Athlete.TeamMemberships
            .Any(tm => tm.TeamId == teamId);

        if (!isMember)
            return Forbid("You are not a member of this team.");

        var trainingPlans = await _context.TrainingPlans
            .Include(tp => tp.Team)
            .Include(tp => tp.Athlete)
            .Where(tp => tp.TeamId == teamId)
            .OrderBy(tp => tp.Date)
            .ThenBy(tp => tp.StartTime)
            .ToListAsync();

        var dtoList = trainingPlans.Select(tp => tp.ToDto()).ToList();
        return Ok(dtoList);
    }

    // GET: api/athletes/training-plans/{id}
    [HttpGet("training-plans/{id}")]
    public async Task<ActionResult<TrainingPlanDto>> GetTrainingPlan(int id)
    {
        int userId = User.GetUserId();

        var playerUser = await _context.PlayerUsers
            .Include(pu => pu.Athlete)
                .ThenInclude(a => a.TeamMemberships)
            .FirstOrDefaultAsync(pu => pu.AthleteId == userId);

        if (playerUser?.Athlete == null)
            return NotFound("Player not found.");

        var athleteId = playerUser.Athlete.Id;

        var trainingPlan = await _context.TrainingPlans
            .Include(tp => tp.Team)
            .Include(tp => tp.Athlete)
            .FirstOrDefaultAsync(tp => tp.Id == id && (
                // Plan assigned directly to this athlete
                tp.AthleteId == athleteId
                // OR plan assigned to a team where this athlete is a member
                || (tp.TeamId != null && _context.TeamMemberships
                    .Any(tm => tm.TeamId == tp.TeamId && tm.AthleteId == athleteId))
            ));

        if (trainingPlan == null)
            return NotFound("Training plan not found or not accessible.");

        return Ok(trainingPlan.ToDto());
    }

    // GET: api/athletes/upcoming-training-plans
    [HttpGet("upcoming-training-plans")]
    public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetUpcomingTrainingPlans()
    {
        int userId = User.GetUserId();

        var playerUser = await _context.PlayerUsers
            .Include(pu => pu.Athlete)
            .FirstOrDefaultAsync(pu => pu.AthleteId == userId);

        if (playerUser?.Athlete == null)
            return NotFound("Player not found.");

        var athleteId = playerUser.Athlete.Id;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var upcomingTrainings = await _context.TrainingPlans
            .Include(tp => tp.Team)
            .Include(tp => tp.Athlete)
            .Where(tp => tp.Date >= today && (
                // Plans assigned directly to this athlete
                tp.AthleteId == athleteId
                // OR plans assigned to teams where this athlete is a member
                || (tp.TeamId != null && _context.TeamMemberships
                    .Any(tm => tm.TeamId == tp.TeamId && tm.AthleteId == athleteId))
            ))
            .OrderBy(tp => tp.Date)
            .ThenBy(tp => tp.StartTime)
            .Take(10) // Limit to next 10 trainings
            .ToListAsync();

        var dtoList = upcomingTrainings.Select(tp => tp.ToDto()).ToList();
        return Ok(dtoList);
    }

    // GET: api/athletes/today-training-plans
    [HttpGet("today-training-plans")]
    public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetTodayTrainingPlans()
    {
        int userId = User.GetUserId();

        var playerUser = await _context.PlayerUsers
            .Include(pu => pu.Athlete)
            .FirstOrDefaultAsync(pu => pu.AthleteId == userId);

        if (playerUser?.Athlete == null)
            return NotFound("Player not found.");

        var athleteId = playerUser.Athlete.Id;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var todayTrainings = await _context.TrainingPlans
            .Include(tp => tp.Team)
            .Include(tp => tp.Athlete)
            .Where(tp => tp.Date == today && (
                tp.AthleteId == athleteId
                || (tp.TeamId != null && _context.TeamMemberships
                    .Any(tm => tm.TeamId == tp.TeamId && tm.AthleteId == athleteId))
            ))
            .OrderBy(tp => tp.StartTime)
            .ToListAsync();

        var dtoList = todayTrainings.Select(tp => tp.ToDto()).ToList();
        return Ok(dtoList);
    }
}