namespace CoachCRM.Dtos
{
    public class AthleteDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public int? TeamId { get; set; }
    }
}