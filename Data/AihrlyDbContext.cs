using Aihrly.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Data;

public class AihrlyDbContext : DbContext
{
    public AihrlyDbContext(DbContextOptions<AihrlyDbContext> options)
        : base(options)
    {
    }

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
    public DbSet<StageHistory> StageHistoryEntries => Set<StageHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.HasMany(x => x.Applications)
                .WithOne(x => x.Job)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.Property(x => x.CandidateName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CandidateEmail).HasMaxLength(320).IsRequired();
            entity.Property(x => x.CoverLetter).HasMaxLength(4000);

            entity.HasIndex(x => new { x.JobId, x.CandidateEmail }).IsUnique();

            entity.HasMany(x => x.Notes)
                .WithOne(x => x.Application)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.StageHistoryEntries)
                .WithOne(x => x.Application)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.CultureFitUpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.CultureFitUpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InterviewUpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.InterviewUpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssessmentUpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.AssessmentUpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationNote>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.HasIndex(x => x.ApplicationId);

            entity.HasOne(x => x.CreatedBy)
                .WithMany(x => x.NotesCreated)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StageHistory>(entity =>
        {
            entity.Property(x => x.Reason).HasMaxLength(1000);
            entity.HasIndex(x => x.ApplicationId);

            entity.HasOne(x => x.ChangedBy)
                .WithMany(x => x.StageChanges)
                .HasForeignKey(x => x.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeamMember>().HasData(
            new TeamMember
            {
                Id = 1,
                Name = "Alex Johnson",
                Email = "alex.johnson@aihrly.test",
                Role = TeamMemberRole.Recruiter
            },
            new TeamMember
            {
                Id = 2,
                Name = "Sam Patel",
                Email = "sam.patel@aihrly.test",
                Role = TeamMemberRole.HiringManager
            },
            new TeamMember
            {
                Id = 3,
                Name = "Jordan Lee",
                Email = "jordan.lee@aihrly.test",
                Role = TeamMemberRole.Recruiter
            }
        );
    }
}
