using CoachCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CoachCRM.Data;

public class AppDbContext : DbContext
{
    public DbSet<CoachUser> CoachUsers { get; set; } = null!;
    
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
            entity.Property(e => e.BirthDate).HasColumnType("date");
        });

        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnType("date");
        });

        modelBuilder.Entity<CoachUser>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }


}