<#
.SYNOPSIS
Seeds the starter quest pack into a LvlUp account via POST /quests/starter-pack.

.DESCRIPTION
Logs in with the given credentials; if the account does not exist yet it
registers one (you will need -Name, -Surname and -Username for that).
The starter pack is created server-side and its quest difficulties are
calibrated to the hunter's current stats, so take the in-app AWAKENING
assessment first to start at your level. Idempotent: existing quests are
skipped.

.EXAMPLE
./scripts/seed-quests.ps1 -Email you@example.com -Password "your-password"
#>
param(
    [Parameter(Mandatory = $true)] [string]$Email,
    [Parameter(Mandatory = $true)] [string]$Password,
    [string]$Name,
    [string]$Surname,
    [string]$Username,
    [string]$ApiUrl = "http://localhost:5180"
)

$ErrorActionPreference = "Stop"

function Invoke-Auth {
    try {
        return Invoke-RestMethod -Method Post -Uri "$ApiUrl/auth/login" -ContentType "application/json" `
            -Body (@{ email = $Email; password = $Password } | ConvertTo-Json)
    } catch {
        if ($_.Exception.Response -and [int]$_.Exception.Response.StatusCode -eq 401) {
            if (-not ($Name -and $Surname -and $Username)) {
                throw "Login failed and no -Name/-Surname/-Username provided to register a new account."
            }
            Write-Host "Account not found - registering '$Username'..."
            return Invoke-RestMethod -Method Post -Uri "$ApiUrl/auth/register" -ContentType "application/json" `
                -Body (@{ email = $Email; password = $Password; name = $Name; surname = $Surname; username = $Username } | ConvertTo-Json)
        }
        throw
    }
}

$auth = Invoke-Auth
$headers = @{ Authorization = "Bearer $($auth.token)" }

$me = Invoke-RestMethod -Uri "$ApiUrl/hunters/me" -Headers $headers
if (-not $me.hasCompletedAssessment) {
    Write-Warning "No awakening assessment on this account yet - quests will be calibrated to base stats."
    Write-Warning "Take the assessment in the app first to start at your level."
}

$pack = Invoke-RestMethod -Method Post -Uri "$ApiUrl/quests/starter-pack" -Headers $headers

Write-Host "Starter pack: $($pack.createdCount) quest(s) created, $($pack.skippedCount) skipped."

$quests = Invoke-RestMethod -Uri "$ApiUrl/quests" -Headers $headers
foreach ($quest in $quests) {
    Write-Host ("  {0,-35} {1}/{2}/{3}" -f $quest.title, $quest.category, $quest.difficulty, $quest.type)
}
