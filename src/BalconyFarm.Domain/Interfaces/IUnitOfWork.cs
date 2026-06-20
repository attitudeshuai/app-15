using BalconyFarm.Domain.Entities;

namespace BalconyFarm.Domain.Interfaces;

public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<PlantingLocation> PlantingLocations { get; }
    IRepository<Crop> Crops { get; }
    IRepository<CropCareTask> CropCareTasks { get; }
    IRepository<HarvestRecord> HarvestRecords { get; }
    IRepository<PestRecord> PestRecords { get; }
    IRepository<TreatmentLog> TreatmentLogs { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<CommunityQuestion> Questions { get; }
    IRepository<CommunityReply> Replies { get; }
    IRepository<CommunityTag> Tags { get; }
    IRepository<CropPhoto> CropPhotos { get; }
    IRepository<SeedInventory> SeedInventories { get; }
    IRepository<Achievement> Achievements { get; }
    IRepository<UserAchievement> UserAchievements { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
