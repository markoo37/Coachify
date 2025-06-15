namespace CoachCRM.Models;
public class Athlete
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public double Weight { get; set; }
    public double Height { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public List<TrainingPlan> TrainingPlans { get; set; } = new();
}