using BalconyFarm.Application.DTOs;

namespace BalconyFarm.Application.Data;

public static class PlantingCalendarData
{
    public static readonly Dictionary<string, CityClimateInfo> CityClimateData = new()
    {
        { "北京", new CityClimateInfo { Name = "北京", Province = "北京", ClimateZone = "暖温带半湿润大陆性季风气候", AverageTempByMonth = new[] { -4, -2, 6, 14, 21, 25, 27, 26, 21, 13, 4, -3 }, PrecipitationByMonth = new[] { 3, 5, 10, 25, 35, 75, 180, 175, 55, 25, 10, 3 } } },
        { "上海", new CityClimateInfo { Name = "上海", Province = "上海", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 4, 6, 10, 16, 21, 25, 29, 28, 24, 18, 12, 6 }, PrecipitationByMonth = new[] { 50, 60, 95, 105, 115, 170, 145, 140, 155, 65, 55, 45 } } },
        { "广州", new CityClimateInfo { Name = "广州", Province = "广东", ClimateZone = "南亚热带海洋性季风气候", AverageTempByMonth = new[] { 14, 15, 19, 23, 26, 28, 29, 28, 27, 24, 20, 16 }, PrecipitationByMonth = new[] { 45, 70, 95, 185, 290, 295, 230, 225, 185, 70, 45, 35 } } },
        { "深圳", new CityClimateInfo { Name = "深圳", Province = "广东", ClimateZone = "南亚热带海洋性季风气候", AverageTempByMonth = new[] { 15, 16, 19, 23, 27, 28, 29, 29, 27, 24, 21, 16 }, PrecipitationByMonth = new[] { 30, 55, 80, 175, 260, 320, 290, 270, 220, 75, 40, 30 } } },
        { "杭州", new CityClimateInfo { Name = "杭州", Province = "浙江", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 5, 7, 11, 17, 22, 26, 30, 29, 24, 18, 12, 7 }, PrecipitationByMonth = new[] { 65, 75, 120, 130, 140, 210, 145, 145, 170, 85, 65, 55 } } },
        { "南京", new CityClimateInfo { Name = "南京", Province = "江苏", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 3, 5, 10, 16, 22, 26, 30, 29, 23, 17, 10, 5 }, PrecipitationByMonth = new[] { 45, 55, 80, 95, 105, 180, 175, 130, 75, 65, 55, 40 } } },
        { "武汉", new CityClimateInfo { Name = "武汉", Province = "湖北", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 4, 7, 12, 18, 23, 27, 30, 29, 24, 18, 12, 6 }, PrecipitationByMonth = new[] { 50, 65, 100, 130, 150, 210, 155, 125, 80, 70, 60, 45 } } },
        { "成都", new CityClimateInfo { Name = "成都", Province = "四川", ClimateZone = "亚热带湿润气候", AverageTempByMonth = new[] { 6, 8, 13, 18, 23, 25, 27, 26, 22, 17, 12, 7 }, PrecipitationByMonth = new[] { 8, 12, 20, 45, 75, 105, 230, 220, 130, 45, 20, 8 } } },
        { "重庆", new CityClimateInfo { Name = "重庆", Province = "重庆", ClimateZone = "亚热带湿润季风气候", AverageTempByMonth = new[] { 8, 10, 15, 20, 24, 27, 29, 28, 23, 18, 14, 9 }, PrecipitationByMonth = new[] { 20, 25, 45, 100, 160, 170, 175, 150, 115, 95, 55, 25 } } },
        { "西安", new CityClimateInfo { Name = "西安", Province = "陕西", ClimateZone = "暖温带半湿润大陆性季风气候", AverageTempByMonth = new[] { -1, 2, 8, 15, 21, 26, 28, 26, 20, 13, 6, 0 }, PrecipitationByMonth = new[] { 5, 10, 25, 40, 50, 55, 95, 80, 105, 60, 25, 5 } } },
        { "天津", new CityClimateInfo { Name = "天津", Province = "天津", ClimateZone = "暖温带半湿润大陆性季风气候", AverageTempByMonth = new[] { -4, -2, 6, 14, 21, 25, 27, 26, 21, 13, 4, -3 }, PrecipitationByMonth = new[] { 3, 5, 10, 22, 32, 70, 170, 160, 45, 22, 10, 3 } } },
        { "苏州", new CityClimateInfo { Name = "苏州", Province = "江苏", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 4, 6, 10, 16, 21, 25, 29, 28, 23, 17, 11, 6 }, PrecipitationByMonth = new[] { 50, 60, 90, 100, 110, 165, 140, 135, 150, 60, 50, 40 } } },
        { "长沙", new CityClimateInfo { Name = "长沙", Province = "湖南", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 5, 7, 12, 18, 23, 27, 30, 29, 24, 18, 12, 7 }, PrecipitationByMonth = new[] { 65, 85, 135, 170, 195, 200, 130, 115, 75, 80, 70, 55 } } },
        { "郑州", new CityClimateInfo { Name = "郑州", Province = "河南", ClimateZone = "暖温带大陆性季风气候", AverageTempByMonth = new[] { 0, 3, 9, 16, 22, 26, 28, 26, 21, 15, 8, 2 }, PrecipitationByMonth = new[] { 8, 12, 25, 40, 60, 70, 140, 120, 85, 45, 25, 10 } } },
        { "青岛", new CityClimateInfo { Name = "青岛", Province = "山东", ClimateZone = "温带季风气候", AverageTempByMonth = new[] { -1, 1, 6, 12, 18, 22, 26, 26, 22, 15, 8, 2 }, PrecipitationByMonth = new[] { 10, 12, 20, 35, 55, 80, 145, 160, 75, 45, 25, 10 } } },
        { "沈阳", new CityClimateInfo { Name = "沈阳", Province = "辽宁", ClimateZone = "温带半湿润大陆性季风气候", AverageTempByMonth = new[] { -12, -9, 0, 9, 17, 22, 25, 24, 17, 8, -2, -9 }, PrecipitationByMonth = new[] { 5, 7, 15, 35, 55, 90, 175, 155, 70, 40, 15, 7 } } },
        { "哈尔滨", new CityClimateInfo { Name = "哈尔滨", Province = "黑龙江", ClimateZone = "中温带大陆性季风气候", AverageTempByMonth = new[] { -19, -15, -5, 6, 15, 21, 23, 21, 14, 5, -7, -16 }, PrecipitationByMonth = new[] { 4, 4, 10, 20, 45, 85, 165, 110, 55, 25, 10, 5 } } },
        { "昆明", new CityClimateInfo { Name = "昆明", Province = "云南", ClimateZone = "亚热带高原季风气候", AverageTempByMonth = new[] { 9, 11, 14, 18, 20, 21, 20, 20, 19, 16, 12, 9 }, PrecipitationByMonth = new[] { 15, 15, 15, 25, 80, 170, 215, 195, 125, 80, 35, 15 } } },
        { "厦门", new CityClimateInfo { Name = "厦门", Province = "福建", ClimateZone = "南亚热带海洋性季风气候", AverageTempByMonth = new[] { 13, 13, 15, 20, 24, 27, 28, 28, 27, 24, 20, 15 }, PrecipitationByMonth = new[] { 40, 75, 115, 155, 210, 210, 135, 165, 130, 45, 35, 35 } } },
        { "济南", new CityClimateInfo { Name = "济南", Province = "山东", ClimateZone = "暖温带半湿润大陆性季风气候", AverageTempByMonth = new[] { -1, 2, 9, 16, 22, 27, 28, 27, 22, 15, 7, 1 }, PrecipitationByMonth = new[] { 6, 10, 20, 35, 55, 85, 190, 155, 65, 45, 20, 8 } } },
        { "合肥", new CityClimateInfo { Name = "合肥", Province = "安徽", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 3, 5, 10, 16, 22, 26, 29, 28, 22, 16, 10, 5 }, PrecipitationByMonth = new[] { 40, 55, 85, 100, 110, 170, 160, 125, 70, 60, 50, 35 } } },
        { "福州", new CityClimateInfo { Name = "福州", Province = "福建", ClimateZone = "南亚热带季风气候", AverageTempByMonth = new[] { 11, 11, 13, 18, 23, 26, 28, 28, 26, 22, 18, 13 }, PrecipitationByMonth = new[] { 50, 85, 135, 160, 205, 240, 130, 170, 145, 55, 45, 45 } } },
        { "南昌", new CityClimateInfo { Name = "南昌", Province = "江西", ClimateZone = "亚热带季风气候", AverageTempByMonth = new[] { 6, 8, 13, 19, 24, 27, 30, 29, 25, 19, 13, 8 }, PrecipitationByMonth = new[] { 70, 95, 165, 210, 235, 280, 150, 125, 75, 65, 70, 50 } } },
        { "贵阳", new CityClimateInfo { Name = "贵阳", Province = "贵州", ClimateZone = "亚热带湿润温和气候", AverageTempByMonth = new[] { 5, 7, 11, 16, 20, 22, 24, 23, 20, 15, 11, 6 }, PrecipitationByMonth = new[] { 25, 25, 35, 75, 140, 220, 180, 145, 90, 85, 50, 25 } } },
        { "南宁", new CityClimateInfo { Name = "南宁", Province = "广西", ClimateZone = "南亚热带季风气候", AverageTempByMonth = new[] { 13, 14, 17, 22, 26, 28, 28, 28, 27, 24, 19, 15 }, PrecipitationByMonth = new[] { 40, 50, 60, 95, 200, 255, 235, 210, 120, 65, 45, 35 } } },
        { "海口", new CityClimateInfo { Name = "海口", Province = "海南", ClimateZone = "热带海洋性季风气候", AverageTempByMonth = new[] { 18, 19, 22, 25, 28, 29, 29, 28, 27, 25, 22, 19 }, PrecipitationByMonth = new[] { 20, 30, 40, 70, 150, 210, 230, 260, 250, 200, 80, 40 } } },
        { "兰州", new CityClimateInfo { Name = "兰州", Province = "甘肃", ClimateZone = "温带大陆性干旱气候", AverageTempByMonth = new[] { -7, -3, 5, 12, 18, 22, 24, 23, 17, 10, 2, -5 }, PrecipitationByMonth = new[] { 2, 3, 8, 15, 30, 40, 60, 70, 40, 20, 5, 2 } } },
        { "乌鲁木齐", new CityClimateInfo { Name = "乌鲁木齐", Province = "新疆", ClimateZone = "温带大陆性干旱气候", AverageTempByMonth = new[] { -15, -11, -1, 10, 18, 23, 25, 23, 16, 7, -4, -12 }, PrecipitationByMonth = new[] { 10, 10, 15, 25, 30, 30, 35, 35, 25, 25, 20, 15 } } },
        { "呼和浩特", new CityClimateInfo { Name = "呼和浩特", Province = "内蒙古", ClimateZone = "中温带大陆性季风气候", AverageTempByMonth = new[] { -13, -9, -1, 8, 16, 21, 23, 21, 15, 7, -3, -10 }, PrecipitationByMonth = new[] { 2, 3, 8, 15, 30, 50, 100, 120, 50, 20, 5, 2 } } },
        { "太原", new CityClimateInfo { Name = "太原", Province = "山西", ClimateZone = "暖温带大陆性季风气候", AverageTempByMonth = new[] { -5, -2, 5, 13, 20, 24, 25, 23, 18, 11, 3, -3 }, PrecipitationByMonth = new[] { 3, 5, 15, 25, 35, 55, 105, 110, 65, 35, 15, 3 } } },
        { "长春", new CityClimateInfo { Name = "长春", Province = "吉林", ClimateZone = "中温带大陆性季风气候", AverageTempByMonth = new[] { -16, -12, -3, 7, 16, 21, 24, 22, 15, 6, -5, -13 }, PrecipitationByMonth = new[] { 5, 5, 10, 25, 55, 95, 170, 135, 55, 30, 15, 5 } } },
        { "石家庄", new CityClimateInfo { Name = "石家庄", Province = "河北", ClimateZone = "暖温带半湿润大陆性季风气候", AverageTempByMonth = new[] { -2, 1, 8, 16, 22, 26, 27, 26, 21, 14, 6, 0 }, PrecipitationByMonth = new[] { 5, 8, 20, 35, 45, 65, 145, 125, 55, 40, 20, 5 } } }
    };

    public static readonly List<CropPlantingInfo> CropPlantingData = new()
    {
        new CropPlantingInfo { Name = "小番茄", Variety = "千禧樱桃番茄", Difficulty = "简单", GrowthDays = "90-120天", Tips = "喜阳光，需要充足日照，保持土壤湿润但不积水", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 8, 9 } }, { "亚热带", new List<int> { 2, 3, 4, 9, 10 } }, { "南亚热带", new List<int> { 1, 2, 3, 10, 11 } }, { "热带", new List<int> { 10, 11, 12, 1 } } }, SuitableLocation = "南阳台、东阳台" },
        new CropPlantingInfo { Name = "生菜", Variety = "奶油生菜", Difficulty = "简单", GrowthDays = "40-60天", Tips = "喜凉爽，避免高温暴晒，保持土壤湿润", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 8, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 4, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2 } }, { "热带", new List<int> { 11, 12, 1, 2 } } }, SuitableLocation = "东阳台、西阳台、北阳台" },
        new CropPlantingInfo { Name = "小白菜", Variety = "上海青", Difficulty = "简单", GrowthDays = "30-45天", Tips = "生长速度快，需要充足水分和肥料", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 8, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 4, 9, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2, 3 } }, { "热带", new List<int> { 11, 12, 1, 2 } } }, SuitableLocation = "南阳台、东阳台" },
        new CropPlantingInfo { Name = "辣椒", Variety = "小米椒", Difficulty = "中等", GrowthDays = "90-150天", Tips = "喜温暖阳光，开花结果期需要充足磷钾肥", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5 } }, { "亚热带", new List<int> { 2, 3, 4, 5 } }, { "南亚热带", new List<int> { 1, 2, 3 } }, { "热带", new List<int> { 11, 12, 1, 2 } } }, SuitableLocation = "南阳台、西阳台" },
        new CropPlantingInfo { Name = "黄瓜", Variety = "水果黄瓜", Difficulty = "中等", GrowthDays = "55-70天", Tips = "需要搭架攀爬，喜水肥，开花结果期注意授粉", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 4, 5, 6, 7, 8 } }, { "亚热带", new List<int> { 3, 4, 5, 8, 9 } }, { "南亚热带", new List<int> { 2, 3, 4, 9, 10 } }, { "热带", new List<int> { 10, 11, 12, 1, 2 } } }, SuitableLocation = "南阳台（需搭架）" },
        new CropPlantingInfo { Name = "香菜", Variety = "大叶香菜", Difficulty = "简单", GrowthDays = "40-60天", Tips = "喜凉爽，不耐高温，夏季种植需要遮阴", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 4, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2 } }, { "热带", new List<int> { 11, 12, 1, 2 } } }, SuitableLocation = "东阳台、北阳台" },
        new CropPlantingInfo { Name = "小葱", Variety = "四季小葱", Difficulty = "简单", GrowthDays = "50-70天", Tips = "可多次采收，保持土壤湿润，定期追施氮肥", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 6, 8, 9, 10 } }, { "亚热带", new List<int> { 1, 2, 3, 4, 9, 10, 11, 12 } }, { "南亚热带", new List<int> { 1, 2, 3, 4, 10, 11, 12 } }, { "热带", new List<int> { 1, 2, 10, 11, 12 } } }, SuitableLocation = "南阳台、东阳台" },
        new CropPlantingInfo { Name = "大蒜", Variety = "紫皮大蒜", Difficulty = "简单", GrowthDays = "90-120天", Tips = "需低温春化才能抽薹结蒜，秋季种植最佳", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 9, 10 } }, { "亚热带", new List<int> { 10, 11 } }, { "南亚热带", new List<int> { 11, 12 } }, { "热带", new List<int> { 11, 12 } } }, SuitableLocation = "南阳台、东阳台" },
        new CropPlantingInfo { Name = "菠菜", Variety = "大叶菠菜", Difficulty = "简单", GrowthDays = "40-60天", Tips = "喜凉爽，25度以上容易抽薹，适合春秋种植", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 8, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 10, 11, 12 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2 } }, { "热带", new List<int> { 11, 12, 1 } } }, SuitableLocation = "东阳台、西阳台" },
        new CropPlantingInfo { Name = "韭菜", Variety = "宽叶韭菜", Difficulty = "简单", GrowthDays = "60-90天（可多年采收）", Tips = "多年生蔬菜，可多次收割，割后追施氮肥促进再生", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 8, 9 } }, { "亚热带", new List<int> { 2, 3, 4, 9, 10 } }, { "南亚热带", new List<int> { 1, 2, 3, 10, 11, 12 } }, { "热带", new List<int> { 1, 2, 11, 12 } } }, SuitableLocation = "南阳台、东阳台" },
        new CropPlantingInfo { Name = "茄子", Variety = "紫长茄", Difficulty = "中等", GrowthDays = "100-150天", Tips = "喜高温阳光，结果期需要充足水肥，注意防治病虫害", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5 } }, { "亚热带", new List<int> { 2, 3, 4 } }, { "南亚热带", new List<int> { 1, 2, 3, 10 } }, { "热带", new List<int> { 11, 12, 1 } } }, SuitableLocation = "南阳台、西阳台" },
        new CropPlantingInfo { Name = "空心菜", Variety = "青梗空心菜", Difficulty = "简单", GrowthDays = "35-50天", Tips = "喜高温高湿，可水培或土培，多次采收", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 5, 6, 7, 8 } }, { "亚热带", new List<int> { 4, 5, 6, 7, 8, 9 } }, { "南亚热带", new List<int> { 3, 4, 5, 6, 9, 10 } }, { "热带", new List<int> { 1, 2, 3, 4, 10, 11, 12 } } }, SuitableLocation = "南阳台、东阳台" },
        new CropPlantingInfo { Name = "苋菜", Variety = "红苋菜", Difficulty = "简单", GrowthDays = "35-50天", Tips = "喜高温，耐热性强，生长速度快", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 5, 6, 7, 8 } }, { "亚热带", new List<int> { 4, 5, 6, 7, 8, 9 } }, { "南亚热带", new List<int> { 3, 4, 5, 6, 9, 10 } }, { "热带", new List<int> { 1, 2, 3, 4, 10, 11, 12 } } }, SuitableLocation = "南阳台" },
        new CropPlantingInfo { Name = "茼蒿", Variety = "小叶茼蒿", Difficulty = "简单", GrowthDays = "40-55天", Tips = "喜冷凉，不耐高温，适合春秋种植", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 4, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2 } }, { "热带", new List<int> { 11, 12, 1, 2 } } }, SuitableLocation = "东阳台、西阳台" },
        new CropPlantingInfo { Name = "萝卜", Variety = "樱桃萝卜", Difficulty = "简单", GrowthDays = "30-45天", Tips = "根部蔬菜，需要疏松土壤，保持水分均匀", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 8, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 4, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2 } }, { "热带", new List<int> { 11, 12, 1 } } }, SuitableLocation = "南阳台、东阳台（深盆）" },
        new CropPlantingInfo { Name = "草莓", Variety = "红颜草莓", Difficulty = "中等", GrowthDays = "90-120天（多年生）", Tips = "喜凉爽阳光，开花期注意授粉，结果期追施磷钾肥", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 8, 9, 10 } }, { "亚热带", new List<int> { 9, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12 } }, { "热带", new List<int> { 11, 12, 1 } } }, SuitableLocation = "南阳台、东阳台" },
        new CropPlantingInfo { Name = "油麦菜", Variety = "香油麦菜", Difficulty = "简单", GrowthDays = "35-50天", Tips = "生长快速，喜凉爽湿润，避免暴晒", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 8, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 4, 9, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2, 3 } }, { "热带", new List<int> { 11, 12, 1, 2 } } }, SuitableLocation = "东阳台、西阳台" },
        new CropPlantingInfo { Name = "苦菊", Variety = "碎叶苦菊", Difficulty = "简单", GrowthDays = "40-60天", Tips = "喜凉爽，耐寒性好，微苦口感", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 9, 10 } }, { "亚热带", new List<int> { 2, 3, 4, 10, 11 } }, { "南亚热带", new List<int> { 10, 11, 12, 1, 2 } }, { "热带", new List<int> { 11, 12, 1 } } }, SuitableLocation = "东阳台、北阳台" },
        new CropPlantingInfo { Name = "荆芥", Variety = "河南荆芥", Difficulty = "简单", GrowthDays = "45-60天", Tips = "耐热耐旱，香味浓郁，可多次采收", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 4, 5, 6, 7, 8 } }, { "亚热带", new List<int> { 3, 4, 5, 6, 7, 8, 9 } }, { "南亚热带", new List<int> { 2, 3, 4, 9, 10 } }, { "热带", new List<int> { 1, 2, 10, 11, 12 } } }, SuitableLocation = "南阳台、西阳台" },
        new CropPlantingInfo { Name = "罗勒", Variety = "甜罗勒", Difficulty = "简单", GrowthDays = "50-70天", Tips = "喜温暖阳光，香气浓郁，经常掐顶促进分枝", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 4, 5, 6, 7, 8 } }, { "亚热带", new List<int> { 3, 4, 5, 6, 7, 8, 9 } }, { "南亚热带", new List<int> { 2, 3, 4, 9, 10, 11 } }, { "热带", new List<int> { 1, 2, 10, 11, 12 } } }, SuitableLocation = "南阳台、西阳台" },
        new CropPlantingInfo { Name = "薄荷", Variety = "留兰香薄荷", Difficulty = "简单", GrowthDays = "50-70天（多年生）", Tips = "繁殖能力强，喜湿润阳光，可水培", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 3, 4, 5, 8, 9 } }, { "亚热带", new List<int> { 2, 3, 4, 9, 10 } }, { "南亚热带", new List<int> { 1, 2, 3, 10, 11, 12 } }, { "热带", new List<int> { 1, 2, 10, 11, 12 } } }, SuitableLocation = "东阳台、西阳台" },
        new CropPlantingInfo { Name = "紫苏", Variety = "紫叶紫苏", Difficulty = "简单", GrowthDays = "50-70天", Tips = "喜温暖，叶片紫红色，具有特殊香气", SuitableMonthsByZone = new Dictionary<string, List<int>> { { "温带", new List<int> { 4, 5, 6, 7, 8 } }, { "亚热带", new List<int> { 3, 4, 5, 6, 7, 8, 9 } }, { "南亚热带", new List<int> { 2, 3, 4, 9, 10 } }, { "热带", new List<int> { 1, 2, 10, 11, 12 } } }, SuitableLocation = "南阳台、西阳台" }
    };

    public static readonly Dictionary<int, SolarTermInfo> SolarTermData = new()
    {
        { 1, new SolarTermInfo { Term = "小寒/大寒", Description = "一年中最寒冷的时期，适合种植耐寒蔬菜，注意防冻保温" } },
        { 2, new SolarTermInfo { Term = "立春/雨水", Description = "气温开始回升，雨水增多，适合早春播种，注意防寒" } },
        { 3, new SolarTermInfo { Term = "惊蛰/春分", Description = "春雷乍动，万物复苏，春播黄金期，适合大多数蔬菜播种" } },
        { 4, new SolarTermInfo { Term = "清明/谷雨", Description = "气温回升快，雨量充足，是播种和移栽的最佳时期" } },
        { 5, new SolarTermInfo { Term = "立夏/小满", Description = "夏季开始，气温升高，喜温蔬菜进入生长旺季" } },
        { 6, new SolarTermInfo { Term = "芒种/夏至", Description = "炎热季节开始，需注意防暑降温，适合耐热蔬菜种植" } },
        { 7, new SolarTermInfo { Term = "小暑/大暑", Description = "一年中最热的时期，注意遮阴防晒，适合空心菜、苋菜等耐热蔬菜" } },
        { 8, new SolarTermInfo { Term = "立秋/处暑", Description = "秋季开始，早晚转凉，秋播蔬菜可以开始准备" } },
        { 9, new SolarTermInfo { Term = "白露/秋分", Description = "天气转凉，昼夜温差大，秋播黄金期，适合叶菜和根茎类蔬菜" } },
        { 10, new SolarTermInfo { Term = "寒露/霜降", Description = "气温继续下降，注意晚霜冻害，适合耐寒蔬菜种植" } },
        { 11, new SolarTermInfo { Term = "立冬/小雪", Description = "冬季开始，气温下降，做好越冬蔬菜的防寒保暖" } },
        { 12, new SolarTermInfo { Term = "大雪/冬至", Description = "最寒冷的时节来临，露地蔬菜减少，可考虑室内种植" } }
    };

    public static string GetClimateZone(string climateZoneFull)
    {
        if (climateZoneFull.Contains("热带")) return "热带";
        if (climateZoneFull.Contains("南亚热带")) return "南亚热带";
        if (climateZoneFull.Contains("亚热带")) return "亚热带";
        if (climateZoneFull.Contains("温带")) return "温带";
        if (climateZoneFull.Contains("高原")) return "亚热带";
        return "亚热带";
    }

    public static string GetPlantingSeason(int month, string climateZone)
    {
        switch (climateZone)
        {
            case "温带":
                if (month is 3 or 4 or 5) return "春播";
                if (month is 8 or 9 or 10) return "秋播";
                if (month is 6 or 7) return "夏播（耐热蔬菜）";
                return "越冬种植";
            case "亚热带":
                if (month is 2 or 3 or 4) return "春播";
                if (month is 9 or 10 or 11) return "秋播";
                if (month is 5 or 6 or 7 or 8) return "夏播（耐热蔬菜）";
                return "冬播（耐寒蔬菜）";
            case "南亚热带":
                if (month is 1 or 2 or 3) return "春播";
                if (month is 10 or 11 or 12) return "秋冬播";
                return "夏播（耐热蔬菜）";
            case "热带":
                if (month is 10 or 11 or 12 or 1) return "旱季种植";
                return "雨季种植（耐涝蔬菜）";
            default:
                return "当季种植";
        }
    }
}

public class CityClimateInfo
{
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string ClimateZone { get; set; } = string.Empty;
    public int[] AverageTempByMonth { get; set; } = Array.Empty<int>();
    public int[] PrecipitationByMonth { get; set; } = Array.Empty<int>();
}

public class CropPlantingInfo
{
    public string Name { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string GrowthDays { get; set; } = string.Empty;
    public string Tips { get; set; } = string.Empty;
    public string SuitableLocation { get; set; } = string.Empty;
    public Dictionary<string, List<int>> SuitableMonthsByZone { get; set; } = new();
}

public class SolarTermInfo
{
    public string Term { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
