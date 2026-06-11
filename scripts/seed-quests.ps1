<#
.SYNOPSIS
Seeds a starter quest pack into a LvlUp account (exercise, skincare, Bible,
coding, conversation practice, vocal training, growth books).

.DESCRIPTION
Logs in with the given credentials; if the account does not exist yet it
registers one (you will need -Name, -Surname and -Username for that).
Quests whose titles already exist on the account are skipped, so the script
is safe to re-run.

.EXAMPLE
./scripts/seed-quests.ps1 -Email you@example.com -Password "your-password"

.EXAMPLE
./scripts/seed-quests.ps1 -Email you@example.com -Password "your-password" `
    -Name Thato -Surname Mokgotsi -Username thato_lvlup
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

# The starter pack: daily habits plus a few one-time milestone quests.
$quests = @(
    # Body
    @{ title = "Workout: 30+ min training";              description = "Strength training, calisthenics or gym - move with intent."; category = "Strength";     difficulty = "Hard";   type = "Daily" },
    @{ title = "Cardio: 20 min or 8k steps";             description = "Run, cycle, brisk walk - get the heart rate up.";            category = "Stamina";      difficulty = "Medium"; type = "Daily" },
    @{ title = "Stretch or mobility: 10 min";            description = "Loosen up - morning or before bed.";                         category = "Physique";     difficulty = "Easy";   type = "Daily" },
    @{ title = "Drink 2L of water";                      description = "Hydration is a stat buff.";                                  category = "Physique";     difficulty = "Easy";   type = "Daily" },
    # Looks / self-care
    @{ title = "Skincare routine (AM + PM)";             description = "Cleanse and moisturise, morning and night.";                 category = "Looks";        difficulty = "Easy";   type = "Daily" },
    # Spirit
    @{ title = "Read the Bible: 1 chapter";              description = "One chapter, every day. Start with the Gospel of John.";     category = "WellBeing";    difficulty = "Medium"; type = "Daily" },
    @{ title = "Read 10 pages of a growth book";         description = "Mindset, habits, confidence - 10 pages minimum.";            category = "Intelligence"; difficulty = "Medium"; type = "Daily" },
    # Mind
    @{ title = "Code for 1 hour (personal)";             description = "Side projects, katas, learning - outside of work tasks.";    category = "Intelligence"; difficulty = "Hard";   type = "Daily" },
    # Voice & confidence
    @{ title = "Start one conversation";                 description = "Greet someone, ask a question, make small talk. One counts."; category = "Charisma";    difficulty = "Medium"; type = "Daily" },
    @{ title = "Vocal training: 15 min";                 description = "Breathing, articulation, reading aloud, recording yourself."; category = "Charisma";    difficulty = "Easy";   type = "Daily" },
    # Milestones (one-time)
    @{ title = "Finish your first growth book";          description = "Complete one mindset book cover to cover.";                  category = "Intelligence"; difficulty = "Elite";  type = "OneTime" },
    @{ title = "Finish the Gospel of John";              description = "21 chapters - one Gospel, completed.";                       category = "WellBeing";    difficulty = "Elite";  type = "OneTime" },
    @{ title = "Hold a 5-minute conversation";           description = "Keep a conversation going for five minutes with someone new."; category = "Charisma";   difficulty = "Elite";  type = "OneTime" },
    @{ title = "Ship a personal coding project";         description = "Build something and put it out there. LvlUp counts!";        category = "Intelligence"; difficulty = "Elite";  type = "OneTime" }
)

$existing = @(Invoke-RestMethod -Uri "$ApiUrl/quests" -Headers $headers)
$existingTitles = @($existing | ForEach-Object { $_.title })

$created = 0
foreach ($quest in $quests) {
    if ($existingTitles -contains $quest.title) {
        Write-Host "skip    : $($quest.title)"
        continue
    }
    Invoke-RestMethod -Method Post -Uri "$ApiUrl/quests" -Headers $headers -ContentType "application/json" `
        -Body ($quest | ConvertTo-Json) | Out-Null
    Write-Host "created : $($quest.title)  [$($quest.category) / $($quest.difficulty) / $($quest.type)]"
    $created++
}

Write-Host ""
Write-Host "Done. $created quest(s) created, $($quests.Count - $created) skipped."
Write-Host "Daily XP available: $((($quests | Where-Object { $_.type -eq 'Daily' }) | ForEach-Object { switch ($_.difficulty) { 'Easy' {10} 'Medium' {25} 'Hard' {50} 'Elite' {100} } } | Measure-Object -Sum).Sum) XP/day if you clear the board."
