using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Application.Services;

public interface IAchievementService
{
    Task<ApiResponse<AchievementSummaryDto>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AchievementDto>>> GetAllAchievementsAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AchievementDto>>> GetUnlockedAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CheckAndUnlockHarvestAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CheckAndUnlockWateringAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CheckAndUnlockPestAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CheckAndUnlockPlantingAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CheckAndUnlockCareTaskAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CheckAndUnlockCommunityAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
}
