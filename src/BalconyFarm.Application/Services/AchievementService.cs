using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Application.Services;

public class AchievementService : IAchievementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(IUnitOfWork unitOfWork, ILogger<AchievementService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<AchievementSummaryDto>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var allAchievements = (await _unitOfWork.Achievements.GetAllAsync(cancellationToken)).ToList();
        var userAchievements = (await _unitOfWork.UserAchievements
            .FindAsync(ua => ua.UserId == userId, cancellationToken)).ToList();

        var unlockedIds = userAchievements.Select(ua => ua.AchievementId).ToHashSet();
        var unlockedDict = userAchievements.ToDictionary(ua => ua.AchievementId);

        var achievementDtos = allAchievements.Select(a =>
        {
            var dto = a.Adapt<AchievementDto>();
            dto.IsUnlocked = unlockedIds.Contains(a.Id);
            if (unlockedDict.TryGetValue(a.Id, out var ua))
            {
                dto.UnlockedAt = ua.UnlockedAt;
                dto.UnlockReason = ua.UnlockReason;
            }
            return dto;
        }).OrderByDescending(a => a.IsUnlocked).ThenBy(a => a.Category).ThenBy(a => a.Points).ToList();

        var summary = new AchievementSummaryDto
        {
            TotalCount = allAchievements.Count,
            UnlockedCount = achievementDtos.Count(a => a.IsUnlocked),
            TotalPoints = allAchievements.Sum(a => a.Points),
            EarnedPoints = achievementDtos.Where(a => a.IsUnlocked).Sum(a => a.Points),
            Achievements = achievementDtos
        };

        return ApiResponse<AchievementSummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<List<AchievementDto>>> GetAllAchievementsAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var allAchievements = (await _unitOfWork.Achievements.GetAllAsync(cancellationToken)).ToList();

        if (!userId.HasValue)
        {
            var dtos = allAchievements.Adapt<List<AchievementDto>>();
            return ApiResponse<List<AchievementDto>>.Success(dtos);
        }

        var userAchievements = (await _unitOfWork.UserAchievements
            .FindAsync(ua => ua.UserId == userId.Value, cancellationToken)).ToList();
        var unlockedIds = userAchievements.Select(ua => ua.AchievementId).ToHashSet();
        var unlockedDict = userAchievements.ToDictionary(ua => ua.AchievementId);

        var achievementDtos = allAchievements.Select(a =>
        {
            var dto = a.Adapt<AchievementDto>();
            dto.IsUnlocked = unlockedIds.Contains(a.Id);
            if (unlockedDict.TryGetValue(a.Id, out var ua))
            {
                dto.UnlockedAt = ua.UnlockedAt;
                dto.UnlockReason = ua.UnlockReason;
            }
            return dto;
        }).ToList();

        return ApiResponse<List<AchievementDto>>.Success(achievementDtos);
    }

    public async Task<ApiResponse<List<AchievementDto>>> GetUnlockedAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userAchievements = (await _unitOfWork.UserAchievements
            .FindAsync(ua => ua.UserId == userId, cancellationToken)).ToList();
        var achievementIds = userAchievements.Select(ua => ua.AchievementId).ToList();
        var allAchievements = (await _unitOfWork.Achievements.GetAllAsync(cancellationToken))
            .Where(a => achievementIds.Contains(a.Id))
            .ToList();

        var unlockedDict = userAchievements.ToDictionary(ua => ua.AchievementId);

        var dtos = allAchievements.Select(a =>
        {
            var dto = a.Adapt<AchievementDto>();
            dto.IsUnlocked = true;
            if (unlockedDict.TryGetValue(a.Id, out var ua))
            {
                dto.UnlockedAt = ua.UnlockedAt;
                dto.UnlockReason = ua.UnlockReason;
            }
            return dto;
        }).OrderByDescending(a => a.UnlockedAt).ToList();

        return ApiResponse<List<AchievementDto>>.Success(dtos);
    }

    public async Task CheckAndUnlockHarvestAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userCrops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();
        var userCropIds = userCrops.Select(c => c.Id).ToList();
        var harvestRecords = (await _unitOfWork.HarvestRecords
            .FindAsync(h => userCropIds.Contains(h.CropId), cancellationToken)).ToList();

        var harvestCount = harvestRecords.Count;
        var hasPerfectQuality = harvestRecords.Any(h => h.Quality == HarvestQuality.Excellent);

        if (harvestCount >= 1)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.FirstHarvest, $"完成第一次收获，共收获 {harvestCount} 次", cancellationToken);
        }
        if (harvestCount >= 10)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.Harvest10Times, $"累计收获 {harvestCount} 次", cancellationToken);
        }
        if (harvestCount >= 50)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.Harvest50Times, $"累计收获 {harvestCount} 次", cancellationToken);
        }
        if (hasPerfectQuality)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.PerfectQualityHarvest, "收获了优秀品质的作物", cancellationToken);
        }
    }

    public async Task CheckAndUnlockWateringAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userCrops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();
        var userCropIds = userCrops.Select(c => c.Id).ToList();
        var wateringTasks = (await _unitOfWork.CropCareTasks
            .FindAsync(t => userCropIds.Contains(t.CropId)
                          && t.TaskType == TaskType.Water
                          && t.Status == TaskStatus.Completed
                          && t.CompletedDate.HasValue, cancellationToken)).ToList();

        var consecutiveDays = CalculateMaxConsecutiveDays(wateringTasks.Select(t => t.CompletedDate!.Value.Date).Distinct().ToList());

        if (consecutiveDays >= 7)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.ConsecutiveWatering7Days, $"连续浇水 {consecutiveDays} 天", cancellationToken);
        }
        if (consecutiveDays >= 30)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.ConsecutiveWatering30Days, $"连续浇水 {consecutiveDays} 天", cancellationToken);
        }
        if (consecutiveDays >= 100)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.ConsecutiveWatering100Days, $"连续浇水 {consecutiveDays} 天", cancellationToken);
        }
    }

    public async Task CheckAndUnlockPestAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userCrops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();
        var userCropIds = userCrops.Select(c => c.Id).ToList();
        var resolvedPests = (await _unitOfWork.PestRecords
            .FindAsync(p => userCropIds.Contains(p.CropId) && p.Status == PestStatus.Resolved, cancellationToken)).ToList();

        var resolvedCount = resolvedPests.Count;

        if (resolvedCount >= 1)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.FirstPestResolved, $"成功解决第一个病虫害问题", cancellationToken);
        }
        if (resolvedCount >= 5)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.PestResolved5Times, $"累计解决 {resolvedCount} 次病虫害问题", cancellationToken);
        }
        if (resolvedCount >= 20)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.PestResolved20Times, $"累计解决 {resolvedCount} 次病虫害问题", cancellationToken);
        }
    }

    public async Task CheckAndUnlockPlantingAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cropCount = await _unitOfWork.Crops.CountAsync(c => c.UserId == userId, cancellationToken);

        if (cropCount >= 1)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.FirstCropPlanted, $"种植了第一棵作物", cancellationToken);
        }
        if (cropCount >= 5)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.CropsPlanted5, $"累计种植 {cropCount} 棵作物", cancellationToken);
        }
        if (cropCount >= 20)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.CropsPlanted20, $"累计种植 {cropCount} 棵作物", cancellationToken);
        }
    }

    public async Task CheckAndUnlockCareTaskAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userCrops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();
        var userCropIds = userCrops.Select(c => c.Id).ToList();
        var completedTasks = (await _unitOfWork.CropCareTasks
            .FindAsync(t => userCropIds.Contains(t.CropId) && t.Status == TaskStatus.Completed, cancellationToken)).ToList();

        var completedTaskTypes = completedTasks.Select(t => t.TaskType).Distinct().ToList();
        var allTaskTypes = Enum.GetValues(typeof(TaskType)).Cast<TaskType>().ToList();

        if (allTaskTypes.All(tt => completedTaskTypes.Contains(tt)))
        {
            await TryUnlockAchievementAsync(userId, AchievementType.AllCareTasksCompleted, "完成了浇水、施肥、修剪、换盆所有类型的养护任务", cancellationToken);
        }
    }

    public async Task CheckAndUnlockCommunityAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var acceptedReplies = await _unitOfWork.Replies.CountAsync(
            r => r.UserId == userId && r.IsAccepted, cancellationToken);

        if (acceptedReplies >= 1)
        {
            await TryUnlockAchievementAsync(userId, AchievementType.CommunityHelper, $"帮助解决了 {acceptedReplies} 个社区问题", cancellationToken);
        }
    }

    private async Task TryUnlockAchievementAsync(Guid userId, AchievementType type, string reason, CancellationToken cancellationToken)
    {
        var achievement = (await _unitOfWork.Achievements
            .FindAsync(a => a.Type == type, cancellationToken)).FirstOrDefault();

        if (achievement == null)
        {
            _logger.LogWarning("成就类型 {AchievementType} 未找到", type);
            return;
        }

        var alreadyUnlocked = await _unitOfWork.UserAchievements.ExistsAsync(
            ua => ua.UserId == userId && ua.AchievementId == achievement.Id, cancellationToken);

        if (alreadyUnlocked)
        {
            return;
        }

        var userAchievement = new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementId = achievement.Id,
            UnlockedAt = DateTime.UtcNow,
            UnlockReason = reason
        };

        await _unitOfWork.UserAchievements.AddAsync(userAchievement, cancellationToken);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = $"🎉 成就解锁：{achievement.Name}",
            Message = $"{achievement.Description}\n获得 {achievement.Points} 成就点！",
            NotificationType = NotificationType.AchievementUnlocked,
            IsRead = false
        };

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("用户 {UserId} 解锁成就 {AchievementName} ({AchievementType})", userId, achievement.Name, type);
    }

    private static int CalculateMaxConsecutiveDays(List<DateTime> dates)
    {
        if (dates.Count == 0)
            return 0;

        var sortedDates = dates.Distinct().OrderBy(d => d).ToList();
        int maxStreak = 1;
        int currentStreak = 1;

        for (int i = 1; i < sortedDates.Count; i++)
        {
            if ((sortedDates[i] - sortedDates[i - 1]).TotalDays == 1)
            {
                currentStreak++;
                maxStreak = Math.Max(maxStreak, currentStreak);
            }
            else
            {
                currentStreak = 1;
            }
        }

        return maxStreak;
    }
}
