using BalconyFarm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalconyFarm.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<CropCareTask> CropCareTasks => Set<CropCareTask>();
    public DbSet<HarvestRecord> HarvestRecords => Set<HarvestRecord>();
    public DbSet<PestRecord> PestRecords => Set<PestRecord>();
    public DbSet<TreatmentLog> TreatmentLogs => Set<TreatmentLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CommunityQuestion> Questions => Set<CommunityQuestion>();
    public DbSet<CommunityReply> Replies => Set<CommunityReply>();
    public DbSet<CommunityTag> Tags => Set<CommunityTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Crop>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.UserId).IsRequired();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Variety).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Location).IsRequired().HasMaxLength(200);
            entity.Property(c => c.ContainerType).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Status).IsRequired();
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Crops)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CropCareTask>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.CropId).IsRequired();
            entity.Property(t => t.TaskType).IsRequired();
            entity.Property(t => t.ScheduledDate).IsRequired();
            entity.Property(t => t.Status).IsRequired();
            entity.Property(t => t.Note).HasMaxLength(1000);

            entity.HasOne(t => t.Crop)
                  .WithMany(c => c.CareTasks)
                  .HasForeignKey(t => t.CropId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HarvestRecord>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.CropId).IsRequired();
            entity.Property(h => h.HarvestDate).IsRequired();
            entity.Property(h => h.Quantity).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(h => h.Unit).IsRequired().HasMaxLength(50);
            entity.Property(h => h.Quality).IsRequired();
            entity.Property(h => h.QualityNote).HasMaxLength(1000);

            entity.HasOne(h => h.Crop)
                  .WithMany(c => c.HarvestRecords)
                  .HasForeignKey(h => h.CropId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PestRecord>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.CropId).IsRequired();
            entity.Property(p => p.IssueType).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Symptoms).IsRequired().HasMaxLength(1000);
            entity.Property(p => p.Treatment).IsRequired().HasMaxLength(1000);
            entity.Property(p => p.DetectedDate).IsRequired();
            entity.Property(p => p.Status).IsRequired();

            entity.HasOne(p => p.Crop)
                  .WithMany(c => c.PestRecords)
                  .HasForeignKey(p => p.CropId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.TreatmentLogs)
                  .WithOne(t => t.PestRecord)
                  .HasForeignKey(t => t.PestRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TreatmentLog>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.PestRecordId).IsRequired();
            entity.Property(t => t.Medication).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(t => t.SymptomChange).IsRequired().HasMaxLength(1000);
            entity.Property(t => t.TreatmentDate).IsRequired();
            entity.Property(t => t.Note).HasMaxLength(500);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.UserId).IsRequired();
            entity.Property(n => n.CropCareTaskId).IsRequired();
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            entity.Property(n => n.NotificationType).IsRequired();
            entity.Property(n => n.IsRead).IsRequired();
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(n => n.UserId);
            entity.HasIndex(n => new { n.UserId, n.IsRead });
            entity.HasIndex(n => new { n.CropCareTaskId, n.NotificationType }).IsUnique();

            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.CropCareTask)
                  .WithMany()
                  .HasForeignKey(n => n.CropCareTaskId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommunityQuestion>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.UserId).IsRequired();
            entity.Property(q => q.Title).IsRequired().HasMaxLength(200);
            entity.Property(q => q.Content).IsRequired().HasMaxLength(5000);
            entity.Property(q => q.CropType).HasMaxLength(50);
            entity.Property(q => q.IsResolved).IsRequired();
            entity.Property(q => q.ViewCount).IsRequired();
            entity.Property(q => q.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(q => q.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(q => q.CropType);
            entity.HasIndex(q => q.IsResolved);
            entity.HasIndex(q => q.CreatedAt);

            entity.HasOne(q => q.User)
                  .WithMany()
                  .HasForeignKey(q => q.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(q => q.Replies)
                  .WithOne(r => r.Question)
                  .HasForeignKey(r => r.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(q => q.Tags)
                  .WithOne(t => t.Question)
                  .HasForeignKey(t => t.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommunityReply>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.QuestionId).IsRequired();
            entity.Property(r => r.UserId).IsRequired();
            entity.Property(r => r.Content).IsRequired().HasMaxLength(2000);
            entity.Property(r => r.IsAccepted).IsRequired();
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(r => r.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(r => r.QuestionId);

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommunityTag>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.QuestionId).IsRequired();
            entity.Property(t => t.Name).IsRequired().HasMaxLength(20);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(t => t.Name);
            entity.HasIndex(t => t.QuestionId);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is User && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            ((User)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
