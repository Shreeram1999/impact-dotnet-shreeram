<#
  Task 7.10/7.11 (+ Week 8 Teacher-only writes) - the same API test run for every data layer (this plays the
  Postman collection's role; StudentApi/StudentApi.http has the same requests
  for clicking through by hand).

  Start the API on the provider under test, then run this script:
    dotnet run --project StudentApi -- --DataLayer:Provider=AdoNet
    powershell -ExecutionPolicy Bypass -File smoke-tests.ps1

  Every value it creates is unique per run (and deleted at the end), so it
  can be re-run against a persistent database. Exit code 0 = all checks passed.
#>
param([string]$BaseUrl = "http://localhost:5095")

$ErrorActionPreference = "Stop"
$script:failures = 0
$suffix = Get-Date -Format "HHmmssfff"

function Invoke-Api([string]$Method, [string]$Path, $Body = $null, [string]$Token = $null) {
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }
    $params = @{ Uri = "$BaseUrl$Path"; Method = $Method; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $Body) { $params["Body"] = ($Body | ConvertTo-Json); $params["ContentType"] = "application/json" }
    try {
        $response = Invoke-WebRequest @params
        $json = if ($response.Content) { $response.Content | ConvertFrom-Json } else { $null }
        return @{ Status = [int]$response.StatusCode; Json = $json; Headers = $response.Headers }
    }
    catch [System.Net.WebException] {
        if ($null -eq $_.Exception.Response) { throw }
        return @{ Status = [int]$_.Exception.Response.StatusCode; Json = $null; Headers = @{} }
    }
}

function Check([string]$Name, [bool]$Condition) {
    if ($Condition) { Write-Host "PASS  $Name" -ForegroundColor Green }
    else { Write-Host "FAIL  $Name" -ForegroundColor Red; $script:failures++ }
}

function New-Student([string]$Name, [string]$Roll, [int]$Age = 20) {
    @{ name = $Name; age = $Age; rollNumber = $Roll; email = "$Roll@smoke.example"; score = 75; enrolledOn = "2026-09-01" }
}

Write-Host "Smoke tests against $BaseUrl" -ForegroundColor Cyan

Check "GET /api/students -> 200" ((Invoke-Api GET "/api/students").Status -eq 200)

$teacher = Invoke-Api POST "/api/auth/login" @{ username = "teacher1"; password = "Teacher@123" }
$student = Invoke-Api POST "/api/auth/login" @{ username = "student1"; password = "Student@123" }
Check "login teacher1 -> 200 + token" ($teacher.Status -eq 200 -and $teacher.Json.token)
Check "login student1 -> 200 + token" ($student.Status -eq 200 -and $student.Json.token)
Check "login wrong password -> 401" ((Invoke-Api POST "/api/auth/login" @{ username = "teacher1"; password = "nope" }).Status -eq 401)
$tt = $teacher.Json.token
$st = $student.Json.token

$roll = "S$suffix"
Check "POST student, no token -> 401" ((Invoke-Api POST "/api/students" (New-Student "Smoke" $roll)).Status -eq 401)
Check "POST student, Student token -> 403" ((Invoke-Api POST "/api/students" (New-Student "Smoke" $roll) $st).Status -eq 403)

$created = Invoke-Api POST "/api/students" (New-Student "Smoke Tester $suffix" $roll) $tt
$id = $created.Json.id
Check "POST student, Teacher token -> 201 + Location" ($created.Status -eq 201 -and $created.Headers["Location"])
Check "POST duplicate roll number -> 409" ((Invoke-Api POST "/api/students" (New-Student "Dup" $roll) $tt).Status -eq 409)
Check "POST age 200 -> 400" ((Invoke-Api POST "/api/students" (New-Student "Old" "O$suffix" 200) $tt).Status -eq 400)

$fetched = Invoke-Api GET "/api/students/$id"
Check "GET /api/students/{id} -> 200" ($fetched.Status -eq 200 -and $fetched.Json.rollNumber -eq $roll)
Check "GET read DTO has enrolledOn, no internalNotes" ($fetched.Json.enrolledOn -eq "2026-09-01" -and -not ($fetched.Json.PSObject.Properties.Name -contains "internalNotes"))
Check "GET /api/students/999999 -> 404" ((Invoke-Api GET "/api/students/999999").Status -eq 404)

$search = Invoke-Api GET "/api/students/search?name=tester%20$suffix"
Check "search (case-insensitive) -> 200 with 1 match" ($search.Status -eq 200 -and @($search.Json).Count -eq 1)
Check "grade ?scale=gpa -> 200" ((Invoke-Api GET "/api/students/$id/grade?scale=gpa").Json.grade -eq "3.00 GPA")

$update = New-Student "Smoke Renamed $suffix" $roll
Check "PUT /api/students/{id} -> 204" ((Invoke-Api PUT "/api/students/$id" $update $tt).Status -eq 204)
Check "PUT persisted the change" ((Invoke-Api GET "/api/students/$id").Json.name -eq "Smoke Renamed $suffix")
Check "PUT /api/students/999999 -> 404" ((Invoke-Api PUT "/api/students/999999" $update $tt).Status -eq 404)

# Task 7.3 - SQL injection attempt is stored as literal text.
$evil = "'; DROP TABLE Students;--"
$injected = Invoke-Api POST "/api/students" (New-Student $evil "X$suffix") $tt
Check "POST injection string as name -> 201" ($injected.Status -eq 201)
Check "injection string stored literally" ((Invoke-Api GET "/api/students/$($injected.Json.id)").Json.name -eq $evil)
Check "Students table still exists" ((Invoke-Api GET "/api/students").Status -eq 200)
Invoke-Api DELETE "/api/students/$($injected.Json.id)" $null $tt | Out-Null

Check "DELETE, Student token -> 403" ((Invoke-Api DELETE "/api/students/$id" $null $st).Status -eq 403)
Check "DELETE /api/students/{id} -> 204" ((Invoke-Api DELETE "/api/students/$id" $null $tt).Status -eq 204)
Check "DELETE again -> 404" ((Invoke-Api DELETE "/api/students/$id" $null $tt).Status -eq 404)
Check "GET /api/teachers -> 200" ((Invoke-Api GET "/api/teachers").Status -eq 200)

if ($script:failures -eq 0) { Write-Host "ALL CHECKS PASSED" -ForegroundColor Green; exit 0 }
Write-Host "$script:failures CHECK(S) FAILED" -ForegroundColor Red
exit 1
