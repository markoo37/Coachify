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
                Email = coach.Email
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
                TeamId = athlete.TeamId
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
    }
}
