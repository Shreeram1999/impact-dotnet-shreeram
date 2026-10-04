<#
  Task 10.7 - every suite against the composed system, in order:
    1. each service's own xUnit suite, each gated at >= 80% line coverage
       (Identity, Academics, Reporting, Gateway)
    2. the React suite (Jest coverageThreshold 80%)
    3. bring the composed stack up locally (3 databases, 3 services,
       gateway, React), run the Selenium E2E suite through the gateway
    4. Task 10.10 - stop Reporting and prove the rest of the portal still works

  docker-compose.yml runs the same composition in containers; this script
  uses local processes + LocalDB so it also works on a machine without Docker.

  Prerequisites: .NET 10 SDK, Node 20+, SQL Server LocalDB + sqlcmd, Chrome or
  Edge. Ports 5100-5103 and 5173 must be free.

  Usage (from Week_10):
    powershell -ExecutionPolicy Bypass -File run-all-tests.ps1 [-Browser edge] [-Headed]
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

function Get-StatusCode([string]$Url, [hashtable]$Headers = @{}) {
    try { return [int](Invoke-WebRequest $Url -UseBasicParsing -Headers $Headers -TimeoutSec 10).StatusCode }
    catch [System.Net.WebException] { if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode } return 0 }
}

if (Test-Path $out) { Remove-Item -Recurse -Force $out }
$logs = Join-Path $out "logs"
New-Item -ItemType Directory -Force $logs | Out-Null

# ------------------------------------------------- 1. per-service unit suites
$services = [ordered]@{
    "Identity (ADO.NET)"       = "Identity\Identity.Tests"
    "Academics (EF Code First)" = "Academics\Academics.Tests"
    "Reporting (EF DB First)"  = "Reporting\Reporting.Tests"
    "Gateway (YARP)"           = "Gateway\Gateway.Tests"
}
$i = 0
foreach ($entry in $services.GetEnumerator()) {
    $i++
    Write-Stage "1.$i $($entry.Key) unit tests"
    $coverageDir = Join-Path $out ("coverage-" + ($entry.Value -split '\\')[0].ToLower())
    dotnet test (Join-Path $root $entry.Value) --collect:"XPlat Code Coverage" --results-directory $coverageDir
    $ok = $LASTEXITCODE -eq 0
    $file = Get-ChildItem $coverageDir -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1
    $percent = "n/a"
    if ($file) {
        $rate = [double]::Parse(([xml](Get-Content $file.FullName)).coverage.'line-rate', [Globalization.CultureInfo]::InvariantCulture)
        $percent = [math]::Round($rate * 100, 1)
        if ($rate -lt 0.8) { $ok = $false }
    }
    else { $ok = $false }
    Write-Host "$($entry.Key) line coverage: $percent%" -ForegroundColor ($(if ($ok) { "Green" } else { "Red" }))
    $results["$($entry.Key) unit (coverage $percent%)"] = $ok
}

# ------------------------------------------------------------- 2. frontend
Write-Stage "2 Frontend unit tests (web/student-portal-web)"
Push-Location (Join-Path $root "web\student-portal-web")
try {
    if (-not (Test-Path "node_modules")) { npm ci --no-audit --no-fund }
    npm run test:coverage -- --ci
    $results["Frontend unit (Jest coverage >= 80%)"] = $LASTEXITCODE -eq 0
}
finally { Pop-Location }

# ------------------------------------------------- 3. composed stack + E2E
if ($results.Values -contains $false) {
    Write-Stage "3 E2E skipped - unit suites must be green first"
    $results["E2E (Selenium, through the gateway)"] = $false
}
else {
    Write-Stage "3 Composed stack + E2E (Selenium, $Browser)"
    foreach ($port in 5100, 5101, 5102, 5103, 5173) {
        if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) { throw "Port $port is already in use." }
    }

    & (Join-Path $root "setup-local-databases.ps1")
    dotnet build (Join-Path $root "ImpactDotnetShreeram.slnx") --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "Build failed." }

    $processes = [ordered]@{}
    foreach ($svc in "Identity\Identity.Api", "Academics\Academics.Api", "Reporting\Reporting.Api", "Gateway\Gateway.Api") {
        $name = ($svc -split '\\')[0]
        $processes[$name] = Start-Process dotnet -PassThru -WindowStyle Hidden `
            -ArgumentList "run --project `"$(Join-Path $root $svc)`" --no-build --launch-profile http" `
            -RedirectStandardOutput (Join-Path $logs "$name.log") -RedirectStandardError (Join-Path $logs "$name.err.log")
    }
    $processes["web"] = Start-Process cmd.exe -PassThru -WindowStyle Hidden -WorkingDirectory (Join-Path $root "web\student-portal-web") `
        -ArgumentList "/c npx vite --port 5173 --strictPort" `
        -RedirectStandardOutput (Join-Path $logs "web.log") -RedirectStandardError (Join-Path $logs "web.err.log")

    try {
        foreach ($port in 5101, 5102, 5103, 5100) { Wait-ForUrl "http://localhost:$port/health" }
        Wait-ForUrl "http://localhost:5173"

        $env:E2E_BROWSER = $Browser
        $env:E2E_HEADLESS = $(if ($Headed) { "false" } else { "true" })
        dotnet test (Join-Path $root "e2e\StudentPortal.E2E") --no-build --logger "console;verbosity=normal"
        $results["E2E (Selenium, through the gateway)"] = $LASTEXITCODE -eq 0

        # ------------------------------------------------ 4. Task 10.10
        Write-Stage "4 Degraded mode - Reporting stopped"
        $login = Invoke-RestMethod "http://localhost:5100/identity/api/auth/login" -Method Post -ContentType "application/json" `
            -Body (@{ username = "student1"; password = "Student@123" } | ConvertTo-Json)
        $auth = @{ Authorization = "Bearer $($login.token)" }
        cmd /c "taskkill /PID $($processes['Reporting'].Id) /T /F >nul 2>&1"
        Start-Sleep -Seconds 2
        $reporting = Get-StatusCode "http://localhost:5100/reporting/api/reports/terms" $auth
        $academics = Get-StatusCode "http://localhost:5100/academics/api/students" $auth
        $identity = Get-StatusCode "http://localhost:5100/identity/health"
        Write-Host "reporting -> $reporting (expected 502), academics -> $academics (expected 200), identity -> $identity (expected 200)"
        $results["Degraded mode (Reporting down, rest works)"] = ($reporting -eq 502 -and $academics -eq 200 -and $identity -eq 200)
    }
    finally {
        foreach ($p in $processes.Values) { cmd /c "taskkill /PID $($p.Id) /T /F >nul 2>&1" }  # already-stopped processes are fine
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
