<#
  Creates the three per-service databases on a local SQL Server (LocalDB by
  default) - the same scripts docker-compose's db-init container runs:
    StudentPortal_Identity   <- Identity/Identity.Api/Database/*.sql      (ADO.NET-owned schema)
    StudentPortal_Reporting  <- Reporting/Reporting.Api/Database/*.sql    (the data team's "pre-existing" schema)
    StudentPortal_Academics  <- dotnet ef database update                 (EF Code First migrations)
  Every step is idempotent, so it is safe to re-run.
#>
param([string]$Server = "(localdb)\MSSQLLocalDB")

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

function Invoke-Scripts([string]$Database, [string]$Folder) {
    sqlcmd -S $Server -b -Q "IF DB_ID(N'$Database') IS NULL CREATE DATABASE [$Database];"
    if ($LASTEXITCODE -ne 0) { throw "Could not create $Database on $Server." }
    foreach ($script in Get-ChildItem (Join-Path $root $Folder) -Filter *.sql | Sort-Object Name) {
        sqlcmd -S $Server -b -d $Database -i $script.FullName | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "$($script.Name) failed on $Database." }
        Write-Host "  $Database <- $($script.Name)"
    }
}

Write-Host "Identity (ADO.NET scripts)" -ForegroundColor Cyan
Invoke-Scripts "StudentPortal_Identity" "Identity\Identity.Api\Database"

Write-Host "Reporting (pre-existing schema for EF DB First)" -ForegroundColor Cyan
Invoke-Scripts "StudentPortal_Reporting" "Reporting\Reporting.Api\Database"

Write-Host "Academics (EF Code First migrations)" -ForegroundColor Cyan
dotnet tool restore | Out-Null
dotnet ef database update --project (Join-Path $root "Academics\Academics.Api")
if ($LASTEXITCODE -ne 0) { throw "Academics migrations failed." }

Write-Host "Databases ready." -ForegroundColor Green
