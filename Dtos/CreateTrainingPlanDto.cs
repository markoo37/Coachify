namespace CoachCRM.Dtos
{
    public class CreateTrainingPlanDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? AthleteId { get; set; }
        public int? TeamId { get; set; }
    }
}