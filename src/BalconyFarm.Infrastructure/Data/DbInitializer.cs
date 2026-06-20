using BalconyFarm.Application.Interfaces;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (!await context.Users.AnyAsync(cancellationToken))
        {
            await SeedUsersAsync(context, passwordHashService, cancellationToken);
        }

        if (!await context.PlantingLocations.AnyAsync(cancellationToken))
        {
            await SeedPlantingLocationsAsync(context, cancellationToken);
        }

        if (!await context.Crops.AnyAsync(cancellationToken))
        {
            await SeedCropsAsync(context, cancellationToken);
        }

        if (!await context.CropCareTasks.AnyAsync(cancellationToken))
        {
            await SeedCropCareTasksAsync(context, cancellationToken);
        }

        if (!await context.HarvestRecords.AnyAsync(cancellationToken))
        {
            await SeedHarvestRecordsAsync(context, cancellationToken);
        }

        if (!await context.PestRecords.AnyAsync(cancellationToken))
        {
            await SeedPestRecordsAsync(context, cancellationToken);
        }

        if (!await context.SeedInventories.AnyAsync(cancellationToken))
        {
            await SeedInventoriesAsync(context, cancellationToken);
        }

        if (!await context.Achievements.AnyAsync(cancellationToken))
        {
            await SeedAchievementsAsync(context, cancellationToken);
        }
    }

    private static async Task SeedUsersAsync(AppDbContext context, IPasswordHashService passwordHashService, CancellationToken cancellationToken)
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Username = "gardener1",
                Email = "gardener1@example.com",
                PasswordHash = passwordHashService.HashPassword("password123"),
                Avatar = "https://example.com/avatars/gardener1.png",
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Username = "plantlover",
                Email = "plantlover@example.com",
                PasswordHash = passwordHashService.HashPassword("password123"),
                Avatar = "https://example.com/avatars/plantlover.png",
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new User
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Username = "urbanfarmer",
                Email = "urbanfarmer@example.com",
                PasswordHash = passwordHashService.HashPassword("password123"),
                Avatar = null,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await context.Users.AddRangeAsync(users, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPlantingLocationsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var locations = new List<PlantingLocation>
        {
            new PlantingLocation
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "南阳台",
                Description = "南向阳台，光照充足，适合种植喜光作物",
                LocationType = "阳台",
                SunlightCondition = "全日照",
                Area = 5.5m,
                PhotoUrl = "https://example.com/locations/south-balcony.jpg",
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new PlantingLocation
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "东阳台",
                Description = "东向阳台，上午有光照",
                LocationType = "阳台",
                SunlightCondition = "半日照",
                Area = 3.2m,
                PhotoUrl = "https://example.com/locations/east-balcony.jpg",
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new PlantingLocation
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "北阳台",
                Description = "北向阳台，光照较少，适合耐阴植物",
                LocationType = "阳台",
                SunlightCondition = "散射光",
                Area = 4.0m,
                PhotoUrl = "https://example.com/locations/north-balcony.jpg",
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new PlantingLocation
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "南阳台",
                Description = "南向大阳台，光照条件好",
                LocationType = "阳台",
                SunlightCondition = "全日照",
                Area = 6.0m,
                PhotoUrl = "https://example.com/locations/south-balcony-2.jpg",
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new PlantingLocation
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "天台花园",
                Description = "楼顶天台，空间开阔，光照充足",
                LocationType = "天台",
                SunlightCondition = "全日照",
                Area = 20.0m,
                PhotoUrl = "https://example.com/locations/rooftop-garden.jpg",
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new PlantingLocation
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "西窗台",
                Description = "西侧窗台，下午有阳光",
                LocationType = "窗台",
                SunlightCondition = "半日照",
                Area = 0.8m,
                PhotoUrl = "https://example.com/locations/west-windowsill.jpg",
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await context.PlantingLocations.AddRangeAsync(locations, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCropsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var crops = new List<Crop>
        {
            new Crop
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "小番茄",
                Variety = "千禧樱桃番茄",
                PlantingDate = DateTime.UtcNow.AddDays(-60),
                Location = "南阳台",
                PlantingLocationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ContainerType = "塑料花盆",
                Status = CropStatus.Growing,
                PhotoUrl = "https://example.com/crops/tomato1.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new Crop
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "罗勒",
                Variety = "甜罗勒",
                PlantingDate = DateTime.UtcNow.AddDays(-45),
                Location = "东阳台",
                PlantingLocationId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ContainerType = "陶瓷花盆",
                Status = CropStatus.Harvesting,
                PhotoUrl = "https://example.com/crops/basil1.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new Crop
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "薄荷",
                Variety = "留兰香薄荷",
                PlantingDate = DateTime.UtcNow.AddDays(-90),
                Location = "北阳台",
                PlantingLocationId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ContainerType = "悬挂花盆",
                Status = CropStatus.Finished,
                PhotoUrl = "https://example.com/crops/mint1.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            },
            new Crop
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "生菜",
                Variety = "奶油生菜",
                PlantingDate = DateTime.UtcNow.AddDays(-30),
                Location = "南阳台",
                PlantingLocationId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ContainerType = "种植箱",
                Status = CropStatus.Growing,
                PhotoUrl = "https://example.com/crops/lettuce1.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Crop
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "辣椒",
                Variety = "小米辣",
                PlantingDate = DateTime.UtcNow.AddDays(-75),
                Location = "西窗台",
                PlantingLocationId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                ContainerType = "塑料花盆",
                Status = CropStatus.Harvesting,
                PhotoUrl = "https://example.com/crops/chili1.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-75)
            }
        };

        await context.Crops.AddRangeAsync(crops, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCropCareTasksAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var tasks = new List<CropCareTask>
        {
            new CropCareTask
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                CropId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TaskType = TaskType.Water,
                ScheduledDate = DateTime.UtcNow.AddDays(-2),
                CompletedDate = DateTime.UtcNow.AddDays(-2),
                Status = Domain.Enums.TaskStatus.Completed,
                Note = "浇水500ml"
            },
            new CropCareTask
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                CropId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TaskType = TaskType.Fertilize,
                ScheduledDate = DateTime.UtcNow.AddDays(-7),
                CompletedDate = DateTime.UtcNow.AddDays(-7),
                Status = Domain.Enums.TaskStatus.Completed,
                Note = "施用有机液肥"
            },
            new CropCareTask
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                CropId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TaskType = TaskType.Water,
                ScheduledDate = DateTime.UtcNow.AddDays(1),
                Status = Domain.Enums.TaskStatus.Pending,
                Note = "浇水500ml"
            },
            new CropCareTask
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                CropId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                TaskType = TaskType.Prune,
                ScheduledDate = DateTime.UtcNow.AddDays(3),
                Status = Domain.Enums.TaskStatus.Pending,
                Note = "修剪侧枝，促进主枝生长"
            },
            new CropCareTask
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                CropId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                TaskType = TaskType.Water,
                ScheduledDate = DateTime.UtcNow,
                Status = Domain.Enums.TaskStatus.InProgress,
                Note = "浇水300ml"
            }
        };

        await context.CropCareTasks.AddRangeAsync(tasks, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedHarvestRecordsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var records = new List<HarvestRecord>
        {
            new HarvestRecord
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                CropId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                HarvestDate = DateTime.UtcNow.AddDays(-5),
                Quantity = 0.15m,
                Unit = "kg",
                Quality = HarvestQuality.Excellent,
                QualityNote = "叶片新鲜，香气浓郁",
                PhotoUrl = "https://example.com/harvest/basil_harvest1.jpg"
            },
            new HarvestRecord
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                CropId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                HarvestDate = DateTime.UtcNow.AddDays(-12),
                Quantity = 0.1m,
                Unit = "kg",
                Quality = HarvestQuality.Good,
                QualityNote = "第一次采收，品质很好",
                PhotoUrl = "https://example.com/harvest/basil_harvest2.jpg"
            },
            new HarvestRecord
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                CropId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                HarvestDate = DateTime.UtcNow.AddDays(-3),
                Quantity = 0.08m,
                Unit = "kg",
                Quality = HarvestQuality.Excellent,
                QualityNote = "辣味十足，约50个辣椒",
                PhotoUrl = "https://example.com/harvest/chili_harvest1.jpg"
            },
            new HarvestRecord
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                CropId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                HarvestDate = DateTime.UtcNow.AddDays(-20),
                Quantity = 0.25m,
                Unit = "kg",
                Quality = HarvestQuality.Good,
                QualityNote = "薄荷香气浓郁，共采收3次",
                PhotoUrl = "https://example.com/harvest/mint_harvest1.jpg"
            },
            new HarvestRecord
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                CropId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                HarvestDate = DateTime.UtcNow.AddDays(-8),
                Quantity = 0.3m,
                Unit = "kg",
                Quality = HarvestQuality.Good,
                QualityNote = "小番茄成熟度好，甜度高",
                PhotoUrl = "https://example.com/harvest/tomato_harvest1.jpg"
            },
            new HarvestRecord
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
                CropId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                HarvestDate = DateTime.UtcNow.AddDays(-15),
                Quantity = 0.2m,
                Unit = "kg",
                Quality = HarvestQuality.Fair,
                QualityNote = "部分果实有裂果现象",
                PhotoUrl = "https://example.com/harvest/tomato_harvest2.jpg"
            },
            new HarvestRecord
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000007"),
                CropId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                HarvestDate = DateTime.UtcNow.AddDays(-2),
                Quantity = 0.12m,
                Unit = "kg",
                Quality = HarvestQuality.Excellent,
                QualityNote = "生菜叶片脆嫩，无虫害",
                PhotoUrl = "https://example.com/harvest/lettuce_harvest1.jpg"
            }
        };

        await context.HarvestRecords.AddRangeAsync(records, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPestRecordsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var records = new List<PestRecord>
        {
            new PestRecord
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                CropId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                IssueType = "蚜虫",
                Symptoms = "叶片背面有绿色小虫子，叶片卷曲",
                Treatment = "喷洒肥皂水稀释液，每天一次，连续3天",
                DetectedDate = DateTime.UtcNow.AddDays(-10),
                ResolvedDate = DateTime.UtcNow.AddDays(-5),
                Status = PestStatus.Resolved
            },
            new PestRecord
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                CropId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                IssueType = "蜗牛",
                Symptoms = "叶片边缘有啃食痕迹，夜晚可见蜗牛",
                Treatment = "在花盆周围撒生石灰，人工捕捉",
                DetectedDate = DateTime.UtcNow.AddDays(-3),
                Status = PestStatus.Treating
            },
            new PestRecord
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                CropId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                IssueType = "白粉病",
                Symptoms = "叶片上有白色粉末状物质",
                Treatment = "增加通风，喷洒小苏打溶液",
                DetectedDate = DateTime.UtcNow.AddDays(-1),
                Status = PestStatus.Detected
            }
        };

        await context.PestRecords.AddRangeAsync(records, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedInventoriesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var seeds = new List<SeedInventory>
        {
            new SeedInventory
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "番茄",
                Variety = "千禧樱桃番茄",
                Quantity = 50,
                Unit = "粒",
                PurchaseDate = DateTime.UtcNow.AddMonths(-2),
                ExpiryDate = DateTime.UtcNow.AddDays(5),
                Notes = "春季购买，出芽率高",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new SeedInventory
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "罗勒",
                Variety = "甜罗勒",
                Quantity = 100,
                Unit = "粒",
                PurchaseDate = DateTime.UtcNow.AddMonths(-3),
                ExpiryDate = DateTime.UtcNow.AddDays(20),
                Notes = "香味浓郁，适合配意大利面",
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new SeedInventory
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "生菜",
                Variety = "奶油生菜",
                Quantity = 200,
                Unit = "粒",
                PurchaseDate = DateTime.UtcNow.AddMonths(-1),
                ExpiryDate = DateTime.UtcNow.AddMonths(5),
                Notes = "耐寒品种，可四季种植",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new SeedInventory
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "薄荷",
                Variety = "留兰香薄荷",
                Quantity = 80,
                Unit = "粒",
                PurchaseDate = DateTime.UtcNow.AddMonths(-4),
                ExpiryDate = DateTime.UtcNow.AddDays(15),
                Notes = "生命力强，容易繁殖",
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new SeedInventory
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000005"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "辣椒",
                Variety = "小米辣",
                Quantity = 30,
                Unit = "粒",
                PurchaseDate = DateTime.UtcNow.AddMonths(-6),
                ExpiryDate = DateTime.UtcNow.AddDays(-5),
                Notes = "去年的种子，可能出芽率降低",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new SeedInventory
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000006"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "黄瓜",
                Variety = "水果黄瓜",
                Quantity = 25,
                Unit = "粒",
                PurchaseDate = DateTime.UtcNow.AddMonths(-1),
                ExpiryDate = DateTime.UtcNow.AddMonths(8),
                Notes = "新品种，期待试试",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await context.SeedInventories.AddRangeAsync(seeds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAchievementsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var achievements = new List<Achievement>
        {
            new Achievement
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Type = AchievementType.FirstHarvest,
                Name = "初尝收获",
                Description = "完成第一次收获，享受劳动的果实",
                IconUrl = "https://example.com/achievements/first-harvest.png",
                Points = 10,
                Category = "收获",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Type = AchievementType.Harvest10Times,
                Name = "丰收小能手",
                Description = "累计完成10次收获",
                IconUrl = "https://example.com/achievements/harvest-10.png",
                Points = 30,
                Category = "收获",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Type = AchievementType.Harvest50Times,
                Name = "收获达人",
                Description = "累计完成50次收获",
                IconUrl = "https://example.com/achievements/harvest-50.png",
                Points = 100,
                Category = "收获",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Type = AchievementType.ConsecutiveWatering7Days,
                Name = "勤劳园丁",
                Description = "连续7天完成浇水任务",
                IconUrl = "https://example.com/achievements/water-7.png",
                Points = 20,
                Category = "养护",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Type = AchievementType.ConsecutiveWatering30Days,
                Name = "坚持不懈",
                Description = "连续30天完成浇水任务",
                IconUrl = "https://example.com/achievements/water-30.png",
                Points = 80,
                Category = "养护",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Type = AchievementType.ConsecutiveWatering100Days,
                Name = "滴水穿石",
                Description = "连续100天完成浇水任务",
                IconUrl = "https://example.com/achievements/water-100.png",
                Points = 300,
                Category = "养护",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Type = AchievementType.FirstPestResolved,
                Name = "初战告捷",
                Description = "成功解决第一次病虫害问题",
                IconUrl = "https://example.com/achievements/pest-first.png",
                Points = 15,
                Category = "病虫害",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Type = AchievementType.PestResolved5Times,
                Name = "病虫害克星",
                Description = "成功解决5次病虫害问题",
                IconUrl = "https://example.com/achievements/pest-5.png",
                Points = 50,
                Category = "病虫害",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Type = AchievementType.PestResolved20Times,
                Name = "植物医生",
                Description = "成功解决20次病虫害问题",
                IconUrl = "https://example.com/achievements/pest-20.png",
                Points = 150,
                Category = "病虫害",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Type = AchievementType.FirstCropPlanted,
                Name = "播下希望",
                Description = "种植第一棵作物",
                IconUrl = "https://example.com/achievements/plant-first.png",
                Points = 5,
                Category = "种植",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Type = AchievementType.CropsPlanted5,
                Name = "小菜园主",
                Description = "累计种植5棵作物",
                IconUrl = "https://example.com/achievements/plant-5.png",
                Points = 25,
                Category = "种植",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Type = AchievementType.CropsPlanted20,
                Name = "种植专家",
                Description = "累计种植20棵作物",
                IconUrl = "https://example.com/achievements/plant-20.png",
                Points = 120,
                Category = "种植",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                Type = AchievementType.PerfectQualityHarvest,
                Name = "完美品质",
                Description = "收获一次优秀品质的作物",
                IconUrl = "https://example.com/achievements/perfect-quality.png",
                Points = 40,
                Category = "收获",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000008"),
                Type = AchievementType.AllCareTasksCompleted,
                Name = "无微不至",
                Description = "完成所有类型的养护任务（浇水、施肥、修剪、换盆）",
                IconUrl = "https://example.com/achievements/all-tasks.png",
                Points = 60,
                Category = "养护",
                CreatedAt = DateTime.UtcNow
            },
            new Achievement
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000009"),
                Type = AchievementType.CommunityHelper,
                Name = "社区热心人",
                Description = "在社区帮助他人解决问题",
                IconUrl = "https://example.com/achievements/community-helper.png",
                Points = 35,
                Category = "社区",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Achievements.AddRangeAsync(achievements, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
