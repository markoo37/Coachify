using CoachCRM.Dtos;
using CoachCRM.Models;

namespace CoachCRM.Extensions
{
    public static class MappingExtensions
    {
        public static TeamDto ToDto(this Team team) =>
            new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                CoachId = team.CoachId
            };

        public static CoachDto ToDto(this Coach coach) =>
            new CoachDto
            {
                Id = coach.Id,
                FirstName = coach.FirstName,
                LastName = coach.LastName,
                Email = coach.Email,
                // ÚJ: User account státusz
                HasUserAccount = coach.User != null
            };

        public static AthleteDto ToDto(this Athlete athlete) =>
            new AthleteDto
            {
                Id = athlete.Id,
                FirstName = athlete.FirstName,
                LastName = athlete.LastName,
                BirthDate = athlete.BirthDate,
                Weight = athlete.Weight,
                Height = athlete.Height,
                Email = athlete.Email,
                HasUserAccount = athlete.User != null,

                // ÚJ: minden tagság lekérése
                TeamIds = athlete.TeamMemberships
                    .Select(tm => tm.TeamId)
                    .ToList()
            };

        public static TrainingPlanDto ToDto(this TrainingPlan plan) =>
            new TrainingPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                AthleteId = plan.AthleteId,
                TeamId = plan.TeamId
            };

        // ÚJ: User mappings
        public static BaseUserDto ToDto(this BaseUser user) =>
            user switch
            {
                CoachUser coachUser => coachUser.ToDto(),
                PlayerUser playerUser => playerUser.ToDto(),
                _ => throw new ArgumentException($"Unknown user type: {user.GetType()}")
            };

        public static CoachUserDto ToDto(this CoachUser user) =>
            new CoachUserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserType = user.UserType,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                CoachId = user.CoachId,
                Coach = user.Coach?.ToDto()
            };

        public static PlayerUserDto ToDto(this PlayerUser user) =>
            new PlayerUserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserType = user.UserType,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                AthleteId = user.AthleteId,
                Athlete = user.Athlete?.ToDto()
            };

        // ÚJ: Player-specific mappings mobil app-nak
        public static PlayerProfileDto ToPlayerProfileDto(this Athlete athlete) =>
            new PlayerProfileDto
            {
                Id = athlete.Id,
                FirstName = athlete.FirstName,
                LastName = athlete.LastName,
                Email = athlete.Email,
                BirthDate = athlete.BirthDate,
                Weight = athlete.Weight,
                Height = athlete.Height,
                Age = athlete.BirthDate.HasValue 
                    ? DateTime.Now.Year - athlete.BirthDate.Value.Year 
                    : null,
                
                // ÚJ: listázd a csapatokat
                Teams = athlete.TeamMemberships
                    .Select(tm => tm.Team.ToTeamInfoDto())
                    .ToList(),

                HasUserAccount = athlete.User != null
            };

        public static TeammateDto ToTeammateDto(this Athlete athlete) =>
            new TeammateDto
            {
                Id = athlete.Id,
                FirstName = athlete.FirstName,
                LastName = athlete.LastName,
                BirthDate = athlete.BirthDate,
                Weight = athlete.Weight,
                Height = athlete.Height,
                Age = athlete.BirthDate.HasValue 
                    ? DateTime.Now.Year - athlete.BirthDate.Value.Year 
                    : null
            };

        public static TeamInfoDto ToTeamInfoDto(this Team team) =>
            new TeamInfoDto
            {
                Id = team.Id,
                Name = team.Name,
                Coach = team.Coach != null 
                    ? new CoachInfoDto
                    {
                        Id = team.Coach.Id,
                        FirstName = team.Coach.FirstName,
                        LastName = team.Coach.LastName,
                        Email = team.Coach.Email
                    }
                    : null,

                // módosítva: Athletes.Count helyett TeamMemberships.Count
                PlayerCount = team.TeamMemberships.Count
            };

        // ÚJ: Login response mapping
        public static PlayerLoginResponseDto ToLoginResponseDto(this PlayerUser user) =>
            new PlayerLoginResponseDto
            {
                Id        = user.Athlete.Id,
                FirstName = user.Athlete.FirstName,
                LastName  = user.Athlete.LastName,
                Email     = user.Email,

                TeamNames  = user.Athlete.TeamMemberships
                    .Select(tm => tm.Team.Name)
                    .ToList(),

                CoachNames = user.Athlete.TeamMemberships
                    .Where(tm => tm.Team.Coach != null)
                    .Select(tm => $"{tm.Team.Coach.FirstName} {tm.Team.Coach.LastName}")
                    .Distinct()
                    .ToList()
            };

    }
}