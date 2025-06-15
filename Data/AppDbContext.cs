using CoachCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CoachCRM.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Athlete> Athletes { get; set; }
    public DbSet<TrainingPlan>  TrainingPlans { get; set; }
    public DbSet<Coach> Coaches { get; set; }
    public DbSet<Team> Teams { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Athlete>(entity =>
        {
            entity.Property(e => e.BirthDate).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.Property(e => e.StartDate).HasColumnType("timestamp with time zone");
            entity.Property(e => e.EndDate).HasColumnType("timestamp with time zone");
        });
    }


}