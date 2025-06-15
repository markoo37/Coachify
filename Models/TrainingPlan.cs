namespace CoachCRM.Models;

public class TrainingPlan
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    
    //kapcsolatok
    public int? AthleteId { get; set; }
    public Athlete? Athlete { get; set; }
    
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
}