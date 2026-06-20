$baseUrl = "http://localhost:8085"
$token = ""

function Write-Result {
    param($testName, $response, $expectedCode = 200)

    $statusCode = $response.StatusCode
    $body = $null
    try {
        $body = $response.Content | ConvertFrom-Json -ErrorAction Stop
    } catch {
        $body = $null
    }

    if ($statusCode -eq $expectedCode) {
        Write-Host "PASS: $testName" -ForegroundColor Green
        if ($body -and $body.code -eq 200) {
            Write-Host "  Code: $($body.code), Message: $($body.message)" -ForegroundColor Gray
            if ($body.data -and $body.data.results) {
                Write-Host "  Total results: $($body.data.results.totalCount)" -ForegroundColor Cyan
                if ($body.data.countByType) {
                    Write-Host "  Count by type:" -ForegroundColor Cyan
                    foreach ($key in $body.data.countByType.PSObject.Properties.Name) {
                        Write-Host "    - $key : $($body.data.countByType.$key)" -ForegroundColor Gray
                    }
                }
                Write-Host "  Items on page: $($body.data.results.items.Count)" -ForegroundColor Gray
                foreach ($item in $body.data.results.items) {
                    Write-Host "    [$($item.type)] $($item.title) | Status: $($item.status) | Date: $($item.date)" -ForegroundColor DarkGray
                    if ($item.metadata) {
                        Write-Host "      Metadata keys: $($item.metadata.PSObject.Properties.Name -join ', ')" -ForegroundColor DarkGray
                    }
                }
            }
        }
    } else {
        Write-Host "FAIL: $testName (Expected: $expectedCode, Got: $statusCode)" -ForegroundColor Red
        Write-Host "  Response: $($response.Content)" -ForegroundColor Gray
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
Write-Host "  Global Search Multi-Criteria Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "--- Step 0: Login ---" -ForegroundColor Yellow
$loginBody = @{
    usernameOrEmail = "gardener1@example.com"
    password = "password123"
}
$response = Invoke-Api "POST" "/api/auth/login" $loginBody
$result = Write-Result "Login as gardener1" $response 200
if ($result -and $result.code -eq 200) {
    $token = $result.data.token
    Write-Host "  Token obtained" -ForegroundColor Cyan
}
Write-Host ""

if (-not $token) {
    Write-Host "FAIL: No token obtained, cannot continue" -ForegroundColor Red
    exit 1
}

Write-Host "--- Step 1: Prepare test data ---" -ForegroundColor Yellow

$crop1Name = "GlobalSearch-Tomato"
$crop2Name = "GlobalSearch-Cucumber"
$crop1Id = ""
$crop2Id = ""
$task1Id = ""
$task2Id = ""
$pest1Id = ""
$harvest1Id = ""

$cropBody1 = @{
    name = $crop1Name
    variety = "Cherry Tomato"
    plantingDate = "2024-03-15"
    location = "South Balcony"
    containerType = "Ceramic Pot"
    status = "Growing"
}
$response = Invoke-Api "POST" "/api/crops" $cropBody1
if ($response.StatusCode -eq 201) {
    $result = $response.Content | ConvertFrom-Json
    if ($result.code -eq 0) {
        $crop1Id = $result.data.id
        Write-Host "  Created crop1: $crop1Name -> $crop1Id" -ForegroundColor Gray
    }
}

$cropBody2 = @{
    name = $crop2Name
    variety = "English Cucumber"
    plantingDate = "2024-05-20"
    location = "East Balcony"
    containerType = "Plastic Pot"
    status = "Harvesting"
}
$response = Invoke-Api "POST" "/api/crops" $cropBody2
if ($response.StatusCode -eq 201) {
    $result = $response.Content | ConvertFrom-Json
    if ($result.code -eq 0) {
        $crop2Id = $result.data.id
        Write-Host "  Created crop2: $crop2Name -> $crop2Id" -ForegroundColor Gray
    }
}

if ($crop1Id) {
    $taskBody1 = @{
        cropId = $crop1Id
        taskType = "Water"
        scheduledDate = "2024-06-01"
        note = "Tomato watering task 500ml"
    }
    $response = Invoke-Api "POST" "/api/cropcaretasks" $taskBody1
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $task1Id = $result.data.id
            Write-Host "  Created task1 (Water, Tomato) -> $task1Id" -ForegroundColor Gray
        }
    }

    $pestBody1 = @{
        cropId = $crop1Id
        issueType = "Aphids"
        symptoms = "Aphids on the back of leaves"
        treatment = "Spray soapy water"
        detectedDate = "2024-06-10"
        status = "Treating"
    }
    $response = Invoke-Api "POST" "/api/pestrecords" $pestBody1
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $pest1Id = $result.data.id
            Write-Host "  Created pest1 (Aphids, Tomato) -> $pest1Id" -ForegroundColor Gray
        }
    }
}

if ($crop2Id) {
    $taskBody2 = @{
        cropId = $crop2Id
        taskType = "Fertilize"
        scheduledDate = "2024-06-15"
        note = "Cucumber fertilizing task"
    }
    $response = Invoke-Api "POST" "/api/cropcaretasks" $taskBody2
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $task2Id = $result.data.id
            Write-Host "  Created task2 (Fertilize, Cucumber) -> $task2Id" -ForegroundColor Gray
        }
    }

    $harvestBody1 = @{
        cropId = $crop2Id
        harvestDate = "2024-06-20"
        quantity = 1.2
        unit = "kg"
        qualityNote = "First harvest of cucumber, great quality"
    }
    $response = Invoke-Api "POST" "/api/harvestrecords" $harvestBody1
    if ($response.StatusCode -eq 201) {
        $result = $response.Content | ConvertFrom-Json
        if ($result.code -eq 0) {
            $harvest1Id = $result.data.id
            Write-Host "  Created harvest1 (Cucumber) -> $harvest1Id" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "--- Step 2: Global Search Multi-Criteria Tests ---" -ForegroundColor Yellow
Write-Host ""

Write-Host "[Test 1] Search by keyword (matches crop name)" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?searchKeyword=Tomato&pageSize=20"
Write-Result "keyword=Tomato (should match tomato-related crops/tasks/pests)" $response 200

Write-Host "[Test 2] Filter by crop status" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?cropStatus=Growing&pageSize=20"
Write-Result "cropStatus=Growing (only growing crops and related)" $response 200

Write-Host "[Test 3] Filter by task type" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?taskType=Water&pageSize=20"
Write-Result "taskType=Water (only water tasks)" $response 200

Write-Host "[Test 4] Filter by pest status" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?pestStatus=Treating&pageSize=20"
Write-Result "pestStatus=Treating (only treating pests)" $response 200

Write-Host "[Test 5] Filter by date range" -ForegroundColor White
$url = "/api/search?dateFrom=2024-06-01&dateTo=2024-06-30&pageSize=20"
$response = Invoke-Api "GET" $url
Write-Result "dateFrom=2024-06-01, dateTo=2024-06-30 (June data)" $response 200

Write-Host "[Test 6] Multi-condition: keyword + crop status + date range" -ForegroundColor White
$url = "/api/search?searchKeyword=Tomato&cropStatus=Growing&dateFrom=2024-01-01&dateTo=2024-12-31&pageSize=20"
$response = Invoke-Api "GET" $url
Write-Result "keyword=Tomato + cropStatus=Growing + year 2024" $response 200

Write-Host "[Test 7] Multi-condition: task type + task status" -ForegroundColor White
$url = "/api/search?taskType=Water&taskStatus=Pending&pageSize=20"
$response = Invoke-Api "GET" $url
Write-Result "taskType=Water + taskStatus=Pending (pending water tasks)" $response 200

Write-Host "[Test 8] Specific search types: only Crop and HarvestRecord" -ForegroundColor White
$url = "/api/search?searchTypes=Crop&searchTypes=HarvestRecord&pageSize=20"
$response = Invoke-Api "GET" $url
Write-Result "searchTypes=[Crop, HarvestRecord] (no tasks or pests)" $response 200

Write-Host "[Test 9] Keyword + date range + only tasks" -ForegroundColor White
$url = "/api/search?searchKeyword=Cucumber&dateFrom=2024-06-01&searchTypes=CropCareTask&pageSize=20"
$response = Invoke-Api "GET" $url
Write-Result "keyword=Cucumber + after June + only tasks" $response 200

Write-Host "[Test 10] Pagination" -ForegroundColor White
$response = Invoke-Api "GET" "/api/search?pageNumber=1&pageSize=2"
Write-Result "pageNumber=1, pageSize=2 (first page, 2 per page)" $response 200

Write-Host ""
Write-Host "--- Step 3: Cleanup test data ---" -ForegroundColor Yellow

if ($harvest1Id) { $null = Invoke-Api "DELETE" "/api/harvestrecords/$harvest1Id"; Write-Host "  Deleted harvest: $harvest1Id" -ForegroundColor Gray }
if ($task2Id) { $null = Invoke-Api "DELETE" "/api/cropcaretasks/$task2Id"; Write-Host "  Deleted task2: $task2Id" -ForegroundColor Gray }
if ($pest1Id) { $null = Invoke-Api "DELETE" "/api/pestrecords/$pest1Id"; Write-Host "  Deleted pest: $pest1Id" -ForegroundColor Gray }
if ($task1Id) { $null = Invoke-Api "DELETE" "/api/cropcaretasks/$task1Id"; Write-Host "  Deleted task1: $task1Id" -ForegroundColor Gray }
if ($crop2Id) { $null = Invoke-Api "DELETE" "/api/crops/$crop2Id"; Write-Host "  Deleted crop2: $crop2Id" -ForegroundColor Gray }
if ($crop1Id) { $null = Invoke-Api "DELETE" "/api/crops/$crop1Id"; Write-Host "  Deleted crop1: $crop1Id" -ForegroundColor Gray }

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Global Search Tests Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Query parameter reference:" -ForegroundColor Yellow
Write-Host "  - searchKeyword: keyword search" -ForegroundColor Gray
Write-Host "  - dateFrom / dateTo: date range filter" -ForegroundColor Gray
Write-Host "  - cropStatus: Growing / Harvesting / Finished" -ForegroundColor Gray
Write-Host "  - taskType: Water / Fertilize / Prune / Repot" -ForegroundColor Gray
Write-Host "  - taskStatus: Pending / InProgress / Completed / Cancelled" -ForegroundColor Gray
Write-Host "  - pestStatus: Detected / Treating / Resolved" -ForegroundColor Gray
Write-Host "  - searchTypes: Crop / CropCareTask / PestRecord / HarvestRecord (multi)" -ForegroundColor Gray
Write-Host "  - pageNumber / pageSize: pagination" -ForegroundColor Gray
Write-Host "  - sortOrder: asc / desc (default desc by date)" -ForegroundColor Gray
Write-Host ""
