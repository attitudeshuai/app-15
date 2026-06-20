$baseUrl = "http://localhost:8085"
$token = ""

function Write-Result {
    param($testName, $response, $expectedCode = 200)

    $statusCode = $response.StatusCode
    $body = $response.Content | ConvertFrom-Json -ErrorAction SilentlyContinue

    if ($statusCode -eq $expectedCode) {
        Write-Host "✅ $testName" -ForegroundColor Green
        if ($body -and $body.code -eq 0) {
            Write-Host "   Code: $($body.code), Message: $($body.message)" -ForegroundColor Gray
            if ($body.data -and $body.data.results) {
                Write-Host "   总结果数: $($body.data.results.totalCount)" -ForegroundColor Cyan
                if ($body.data.countByType) {
                    Write-Host "   按类型统计:" -ForegroundColor Cyan
                    foreach ($key in $body.data.countByType.PSObject.Properties.Name) {
                        Write-Host "     - $key : $($body.data.countByType.$key)" -ForegroundColor Gray
                    }
                }
                Write-Host "   当前页结果数: $($body.data.results.items.Count)" -ForegroundColor Gray
                foreach ($item in $body.data.results.items) {
                    Write-Host "     [$($item.type)] $($item.title) | 状态: $($item.status) | 日期: $($item.date)" -ForegroundColor DarkGray
                }
            }
        }
    } else {
        Write-Host "❌ $testName (Expected: $expectedCode, Got: $statusCode)" -ForegroundColor Red
        Write-Host "   Response: $($response.Content)" -ForegroundColor Gray
    }
    Write-Host ""
    return $body
}

function Invoke-Api {
    param($method, $endpoint, $body = $null, $headers = @{})

    if ($token -and -not $headers.ContainsKey("Authorization")) {
        $headers["Authorization"] = "Bearer $token"
    }

    $params = @{
        Uri = "$baseUrl$endpoint"
        Method = $method
        Headers = $headers
        UseBasicParsing = $true
    }

    if ($body) {
        $params["Body"] = ($body | ConvertTo-Json -Depth 10)
        $params["ContentType"] = "application/json"
    }

    try {
        $response = Invoke-WebRequest @params
        return [pscustomobject]@{
            StatusCode = $response.StatusCode
            Content = $response.Content
        }
    } catch {
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            return [pscustomobject]@{
                StatusCode = [int]$_.Exception.Response.StatusCode
                Content = $responseBody
            }
        }
        throw
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  全局搜索 多条件组合筛选 测试" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "--- 🔐 Step 0: 用户登录 ---" -ForegroundColor Yellow
$loginBody = @{
    usernameOrEmail = "gardener1@example.com"
    password = "password123"
}
$response = Invoke-Api "POST" "/api/auth/login" $loginBody
$result = Write-Result "用户登录（gardener1）" $response 200
if ($result -and $result.code -eq 0) {
    $token = $result.data.token
    Write-Host "   Token 已获取" -ForegroundColor Cyan
}
Write-Host ""

if (-not $token) {
    Write-Host "❌ 未获取到 Token，无法继续测试" -ForegroundColor Red
    exit 1
}

Write-Host "--- 🌱 Step 1: 准备测试数据 ---" -ForegroundColor Yellow

$crop1Name = "全局搜索测试-番茄"
$crop2Name = "全局搜索测试-黄瓜"
$crop1Id = ""
$crop2Id = ""
$task1Id = ""
$task2Id = ""
$pest1Id = ""
$harvest1Id = ""

$cropBody1 = @{
    name = $crop1Name
    variety = "樱桃番茄"
    plantingDate = "2024-03-15"
    location = "南阳台"
    containerType = "陶瓷花盆"
    status = "Growing"
}
$response = Invoke-Api "POST" "/api/crops" $cropBody1
if ($response.StatusCode -eq 201) {
    $result = $response.Content | ConvertFrom-Json
    if ($result.code -eq 0) {
        $crop1Id = $result.data.id
        Write-Host "   创建作物1: $crop1Name -> $crop1Id" -ForegroundColor Gray
    }
}

$cropBody2 = @{
    name = $crop2Name
    variety = "水果黄瓜"
    plantingDate = "2024-05-20"
    location = "东阳台"
    containerType = "塑料花盆"
    status = "Harvesting"
}
$response = Invoke-Api "POST" "/api/crops" $cropBody2
if ($response.StatusCode -eq 201) {
    $result = $response.Content | ConvertFrom-Json
    if ($result.code -eq 0) {
        $crop2Id = $result.data.id
        Write-Host "   创建作物2: $crop2Name -> $crop2Id" -ForegroundColor Gray
    }
}

if ($crop1Id) {
    $taskBody1 = @{
        cropId = $crop1Id
        taskType = "Water"
        scheduledDate = "2024-06-01"
        note = "番茄浇水任务500ml"
    }
    $response = Invoke-Api "POST" "/api/cropcaretasks" $taskBody1
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $task1Id = $result.data.id
            Write-Host "   创建任务1(浇水,番茄) -> $task1Id" -ForegroundColor Gray
        }
    }

    $pestBody1 = @{
        cropId = $crop1Id
        issueType = "蚜虫"
        symptoms = "叶片背面出现蚜虫"
        treatment = "喷洒肥皂水"
        detectedDate = "2024-06-10"
        status = "Treating"
    }
    $response = Invoke-Api "POST" "/api/pestrecords" $pestBody1
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $pest1Id = $result.data.id
            Write-Host "   创建病虫害1(蚜虫,番茄) -> $pest1Id" -ForegroundColor Gray
        }
    }
}

if ($crop2Id) {
    $taskBody2 = @{
        cropId = $crop2Id
        taskType = "Fertilize"
        scheduledDate = "2024-06-15"
        note = "黄瓜施肥任务"
    }
    $response = Invoke-Api "POST" "/api/cropcaretasks" $taskBody2
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $task2Id = $result.data.id
            Write-Host "   创建任务2(施肥,黄瓜) -> $task2Id" -ForegroundColor Gray
        }
    }

    $harvestBody1 = @{
        cropId = $crop2Id
        harvestDate = "2024-06-20"
        quantity = 1.2
        unit = "kg"
        qualityNote = "黄瓜初次采收品质好"
    }
    $response = Invoke-Api "POST" "/api/harvestrecords" $harvestBody1
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $harvest1Id = $result.data.id
            Write-Host "   创建收获1(黄瓜) -> $harvest1Id" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "--- 🔍 Step 2: 全局搜索多条件组合测试 ---" -ForegroundColor Yellow
Write-Host ""

Write-Host "【测试1】按关键词搜索（匹配作物名称）" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?searchKeyword=番茄&pageSize=20"
Write-Result "关键词='番茄'（应匹配番茄相关的作物、任务、病虫害）" $response 200

Write-Host "【测试2】按作物状态筛选" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?cropStatus=Growing&pageSize=20"
Write-Result "cropStatus=Growing（只显示生长中的作物相关）" $response 200

Write-Host "【测试3】按任务类型筛选" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?taskType=Water&pageSize=20"
Write-Result "taskType=Water（只显示浇水任务）" $response 200

Write-Host "【测试4】按病虫害状态筛选" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?pestStatus=Treating&pageSize=20"
Write-Result "pestStatus=Treating（只显示治疗中的病虫害）" $response 200

Write-Host "【测试5】按时间范围筛选" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?dateFrom=2024-06-01&dateTo=2024-06-30&pageSize=20"
Write-Result "dateFrom=2024-06-01, dateTo=2024-06-30（6月份的数据）" $response 200

Write-Host "【测试6】多条件组合：关键词 + 作物状态 + 时间范围" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?searchKeyword=番茄&cropStatus=Growing&dateFrom=2024-01-01&dateTo=2024-12-31&pageSize=20"
Write-Result "关键词='番茄' + cropStatus=Growing + 2024全年" $response 200

Write-Host "【测试7】多条件组合：任务类型 + 任务状态" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?taskType=Water&taskStatus=Pending&pageSize=20"
Write-Result "taskType=Water + taskStatus=Pending（待处理的浇水任务）" $response 200

Write-Host "【测试8】指定搜索类型：只搜索作物和收获记录" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?searchTypes=Crop&searchTypes=HarvestRecord&pageSize=20"
Write-Result "searchTypes=[Crop, HarvestRecord]（不显示任务和病虫害）" $response 200

Write-Host "【测试9】关键词 + 时间范围 + 只搜索护理任务" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?searchKeyword=黄瓜&dateFrom=2024-06-01&searchTypes=CropCareTask&pageSize=20"
Write-Result "关键词='黄瓜' + 6月后 + 只搜索任务" $response 200

Write-Host "【测试10】分页测试" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?pageNumber=1&pageSize=2"
Write-Result "pageNumber=1, pageSize=2（第一页，每页2条）" $response 200

Write-Host ""
Write-Host "--- 🧹 Step 3: 清理测试数据 ---" -ForegroundColor Yellow

if ($harvest1Id) { $null = Invoke-Api "DELETE" "/api/harvestrecords/$harvest1Id"; Write-Host "   删除收获记录: $harvest1Id" -ForegroundColor Gray }
if ($task2Id) { $null = Invoke-Api "DELETE" "/api/cropcaretasks/$task2Id"; Write-Host "   删除任务2: $task2Id" -ForegroundColor Gray }
if ($pest1Id) { $null = Invoke-Api "DELETE" "/api/pestrecords/$pest1Id"; Write-Host "   删除病虫害1: $pest1Id" -ForegroundColor Gray }
if ($task1Id) { $null = Invoke-Api "DELETE" "/api/cropcaretasks/$task1Id"; Write-Host "   删除任务1: $task1Id" -ForegroundColor Gray }
if ($crop2Id) { $null = Invoke-Api "DELETE" "/api/crops/$crop2Id"; Write-Host "   删除作物2: $crop2Id" -ForegroundColor Gray }
if ($crop1Id) { $null = Invoke-Api "DELETE" "/api/crops/$crop1Id"; Write-Host "   删除作物1: $crop1Id" -ForegroundColor Gray }

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  全局搜索测试完成！" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📘 可用查询参数说明：" -ForegroundColor Yellow
Write-Host "  - searchKeyword: 关键词搜索（作物名称/品种/位置, 任务备注, 病虫害问题/症状/处理, 收获单位/品质备注 + 所有关联的作物名称）" -ForegroundColor Gray
Write-Host "  - dateFrom / dateTo: 时间范围筛选（作物种植/创建日期, 任务计划/完成日期, 病虫害发现/解决日期, 收获日期）" -ForegroundColor Gray
Write-Host "  - cropStatus: 作物状态 (Growing / Harvesting / Finished)" -ForegroundColor Gray
Write-Host "  - taskType: 任务类型 (Water / Fertilize / Prune / Repot)" -ForegroundColor Gray
Write-Host "  - taskStatus: 任务状态 (Pending / InProgress / Completed / Cancelled)" -ForegroundColor Gray
Write-Host "  - pestStatus: 病虫害状态 (Detected / Treating / Resolved)" -ForegroundColor Gray
Write-Host "  - searchTypes: 指定搜索类型，可多选 (Crop / CropCareTask / PestRecord / HarvestRecord)" -ForegroundColor Gray
Write-Host "  - pageNumber / pageSize: 分页参数" -ForegroundColor Gray
Write-Host "  - sortOrder: 排序 (asc / desc)，默认按日期倒序" -ForegroundColor Gray
Write-Host ""
