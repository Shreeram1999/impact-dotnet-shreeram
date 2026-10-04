<#
  Task 9.9 - the whole suite from a clean checkout, in the required order:
    1. backend unit tests (xUnit + Moq)  + coverage gate (>= 80% lines)
    2. frontend unit tests (Jest + RTL)  + coverage gate (jest coverageThreshold 80%)
    3. E2E (Selenium) against the real, running portal
  E2E only runs if both unit stages are green.

  Prerequisites: .NET 10 SDK, Node 20+ (npm on PATH), SQL Server LocalDB,
  Chrome or Edge. Ports 5095 (API) and 5173 (React) must be free.

  Usage (from Week_9):
    powershell -ExecutionPolicy Bypass -File run-all-tests.ps1
    powershell -ExecutionPolicy Bypass -File run-all-tests.ps1 -Browser edge -Headed
#>
param(
    [ValidateSet("chrome", "edge")] [string]$Browser = "chrome",
    [switch]$Headed
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$out = Join-Path $root "TestResults"
$results = [ordered]@{}

function Write-Stage([string]$Text) { Write-Host "`n=== $Text ===" -ForegroundColor Cyan }

function Wait-ForUrl([string]$Url, [int]$Seconds = 90) {
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        try { Invoke-WebRequest $Url -UseBasicParsing -TimeoutSec 3 | Out-Null; return } catch { Start-Sleep -Milliseconds 500 }
    }
    throw "Timed out waiting for $Url"
}

function Test-PortFree([int]$Port) {
    -not (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
}

if (Test-Path $out) { Remove-Item -Recurse -Force $out }
New-Item -ItemType Directory -Force (Join-Path $out "logs") | Out-Null

# ---------------------------------------------------------------- 1. backend
Write-Stage "1/3 Backend unit tests (StudentApi.Tests)"
dotnet test (Join-Path $root "StudentApi.Tests") --collect:"XPlat Code Coverage" --results-directory (Join-Path $out "backend")
$backendOk = $LASTEXITCODE -eq 0
$coverageFile = Get-ChildItem (Join-Path $out "backend") -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1
if ($coverageFile) {
    $rate = [double]::Parse(([xml](Get-Content $coverageFile.FullName)).coverage.'line-rate', [Globalization.CultureInfo]::InvariantCulture)
    $percent = [math]::Round($rate * 100, 1)
    Write-Host "Backend line coverage: $percent%" -ForegroundColor ($(if ($rate -ge 0.8) { "Green" } else { "Red" }))
    if ($rate -lt 0.8) { $backendOk = $false }
}
else { $backendOk = $false; $percent = "n/a" }
$results["Backend unit (coverage $percent%)"] = $backendOk

# --------------------------------------------------------------- 2. frontend
Write-Stage "2/3 Frontend unit tests (student-portal-web)"
Push-Location (Join-Path $root "student-portal-web")
try {
    if (-not (Test-Path "node_modules")) { npm ci --no-audit --no-fund }
    npm run test:coverage -- --ci   # fails by itself if any metric < 80%
    $results["Frontend unit (Jest coverage >= 80%)"] = $LASTEXITCODE -eq 0
}
finally { Pop-Location }

# -------------------------------------------------------------------- 3. E2E
if ($results.Values -contains $false) {
    Write-Stage "3/3 E2E skipped - unit suites must be green first"
    $results["E2E (Selenium)"] = $false
}
else {
    Write-Stage "3/3 E2E (Selenium, $Browser)"
    foreach ($port in 5095, 5173) {
        if (-not (Test-PortFree $port)) { throw "Port $port is already in use - stop whatever is running there first." }
    }

    dotnet tool restore | Out-Null
    dotnet ef database update --project (Join-Path $root "StudentApi") --context AppDbContext
    dotnet build (Join-Path $root "StudentApi") --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "API build failed." }

    $logs = Join-Path $out "logs"
    $api = Start-Process dotnet -PassThru -WindowStyle Hidden `
        -ArgumentList "run --project `"$(Join-Path $root 'StudentApi')`" --no-build --launch-profile http -- --DataLayer:Provider=EfCodeFirst" `
        -RedirectStandardOutput (Join-Path $logs "api.log") -RedirectStandardError (Join-Path $logs "api.err.log")
    $web = Start-Process cmd.exe -PassThru -WindowStyle Hidden -WorkingDirectory (Join-Path $root "student-portal-web") `
        -ArgumentList "/c npx vite --port 5173 --strictPort" `
        -RedirectStandardOutput (Join-Path $logs "web.log") -RedirectStandardError (Join-Path $logs "web.err.log")
    try {
        Wait-ForUrl "http://localhost:5095/api/teachers"
        Wait-ForUrl "http://localhost:5173"

        $env:E2E_BROWSER = $Browser
        $env:E2E_HEADLESS = $(if ($Headed) { "false" } else { "true" })
        dotnet test (Join-Path $root "StudentPortal.E2E") --logger "console;verbosity=normal"
        $results["E2E (Selenium)"] = $LASTEXITCODE -eq 0
    }
    finally {
        # /T - also stop the child processes (StudentApi.exe, node)
        cmd /c "taskkill /PID $($api.Id) /T /F >nul 2>&1"
        cmd /c "taskkill /PID $($web.Id) /T /F >nul 2>&1"
    }
}

# ---------------------------------------------------------------- summary
Write-Stage "Summary"
foreach ($entry in $results.GetEnumerator()) {
    $label = if ($entry.Value) { "PASS" } else { "FAIL" }
    Write-Host ("{0,-5} {1}" -f $label, $entry.Key) -ForegroundColor ($(if ($entry.Value) { "Green" } else { "Red" }))
}
if ($results.Values -contains $false) { exit 1 }
Write-Host "`nALL SUITES GREEN" -ForegroundColor Green
exit 0
