using CoachCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CoachCRM.Data;

public class AppDbContext : DbContext
{
    // Base user tables
    public DbSet<BaseUser> Users      { get; set; } = null!;
    public DbSet<CoachUser> CoachUsers  { get; set; } = null!;
    public DbSet<PlayerUser> PlayerUsers { get; set; } = null!;

    public DbSet<Athlete> Athletes       { get; set; } = null!;
    public DbSet<Team> Teams             { get; set; } = null!;
    public DbSet<TeamMembership> TeamMemberships { get; set; } = null!;
    public DbSet<TrainingPlan> TrainingPlans   { get; set; } = null!;
    public DbSet<Coach> Coaches         { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens   { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) 
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── User inheritance (TPH) ───────────────────────────────────────────
        modelBuilder.Entity<BaseUser>()
            .HasDiscriminator<string>("UserType")
            .HasValue<CoachUser>("Coach")
            .HasValue<PlayerUser>("Player");

        // ─── CoachUser ↔ Coach 1:1 ────────────────────────────────────────────
        modelBuilder.Entity<CoachUser>()
            .HasOne(cu => cu.Coach)
            .WithOne(c => c.User)
            .HasForeignKey<CoachUser>(cu => cu.CoachId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── PlayerUser ↔ Athlete 1:1 ─────────────────────────────────────────
        modelBuilder.Entity<PlayerUser>()
            .HasOne(pu => pu.Athlete)
            .WithOne(a => a.User)
            .HasForeignKey<PlayerUser>(pu => pu.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Unique email ────────────────────────────────────────────────────
        modelBuilder.Entity<BaseUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // ─── Athlete konfiguráció ─────────────────────────────────────────────
        modelBuilder.Entity<Athlete>(entity =>
        {
            entity.Property(e => e.BirthDate).HasColumnType("date");
            entity.Property(e => e.Email).IsRequired(false);
        });

        // ─── TrainingPlan konfiguráció ────────────────────────────────────────
        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.Property(tp => tp.StartDate).HasColumnType("date");
            entity.Property(tp => tp.EndDate).HasColumnType("date");
        });

        // ─── Coach ↔ Teams 1:N ─────────────────────────────────────────────────
        modelBuilder.Entity<Coach>()
            .HasMany(c => c.Teams)
            .WithOne(t => t.Coach)
            .HasForeignKey(t => t.CoachId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── RefreshToken ↔ BaseUser 1:N ───────────────────────────────────────
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── **ÚJ: TeamMembership N:N konfiguráció** ─────────────────────────
        modelBuilder.Entity<TeamMembership>()
            .HasOne(tm => tm.Athlete)
            .WithMany(a => a.TeamMemberships)
            .HasForeignKey(tm => tm.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeamMembership>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.TeamMemberships)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
