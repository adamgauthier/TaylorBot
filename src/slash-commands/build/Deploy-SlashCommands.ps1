param (
    [Parameter(Mandatory = $true)]
    [string]$ApplicationId,

    [Parameter(Mandatory = $true)]
    [string]$BotToken,

    [Parameter(Mandatory = $false)]
    [string]$GuildCommandsGuildId
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = 'SilentlyContinue'

[System.ArgumentException]::ThrowIfNullOrWhiteSpace($ApplicationId, "ApplicationId")
[System.ArgumentException]::ThrowIfNullOrWhiteSpace($BotToken, "BotToken")

# Global commands
$slashCommandsDir = "$PSScriptRoot/.."
[array] $files = Get-ChildItem -File -Path $slashCommandsDir
Write-Output "Processing $($files.Count) slash command file(s)"

$jsonArray = @()
foreach ($file in $files) {
    $jsonContent = Get-Content $file.FullName | ConvertFrom-Json
    $jsonArray += $jsonContent
}
$merged = ConvertTo-Json -Depth 100 $jsonArray

$url = "https://discord.com/api/v10/applications/$ApplicationId/commands"
Write-Output "Publishing to $url"

$response = Invoke-WebRequest -Uri $url `
    -UserAgent 'TaylorBot-Deploy (https://taylorbot.app/, 0.3.0)' `
    -Headers @{ Authorization = "Bot $BotToken" } `
    -Method 'PUT' `
    -ContentType 'application/json' `
    -Body $merged

Write-Output "$url ($($response.StatusCode))"

# Guild-specific commands
if ([string]::IsNullOrEmpty($GuildCommandsGuildId)) {
    foreach ($subDir in (Get-ChildItem -Directory "$slashCommandsDir/guilds")) {
        [array] $guildFiles = Get-ChildItem -File -Path $subDir.FullName
        Write-Output "Processing $($guildFiles.Count) slash command file(s)"

        $jsonArray = @()
        foreach ($guildFile in $guildFiles) {
            $jsonContent = Get-Content $guildFile.FullName | ConvertFrom-Json
            $jsonArray += $jsonContent
        }
        $merged = ConvertTo-Json -Depth 100 $jsonArray

        $guildId = $subDir.Name
        $url = "https://discord.com/api/v10/applications/$ApplicationId/guilds/$guildId/commands"
        Write-Output "Publishing $($guildFiles.Count) to $url"

        $response = Invoke-WebRequest -Uri $url `
            -UserAgent 'TaylorBot-Deploy (https://taylorbot.app/, 0.3.0)' `
            -Headers @{ Authorization = "Bot $BotToken" } `
            -Method 'PUT' `
            -ContentType 'application/json' `
            -Body $merged

        Write-Output "$url ($($response.StatusCode))"
    }
}
else {
    Write-Output "Publishing all guild commands to $GuildCommandsGuildId"
    [array] $guildDirs = Get-ChildItem -Directory "$slashCommandsDir/guilds"

    $guildCommandsArray = @()
    foreach ($guildDir in $guildDirs) {
        [array] $guildCommands = Get-ChildItem -File -Path $guildDir.FullName
        foreach ($file in $guildCommands) {
            $jsonContent = Get-Content $file.FullName | ConvertFrom-Json
            $guildCommandsArray += $jsonContent
        }
    }
    Write-Output "Processing $($guildCommandsArray.Count) slash command file(s)"

    $merged = ConvertTo-Json -Depth 100 $guildCommandsArray

    $url = "https://discord.com/api/v10/applications/$ApplicationId/guilds/$GuildCommandsGuildId/commands"
    Write-Output "Publishing to $url"

    $guildResponse = Invoke-WebRequest -Uri $url `
        -UserAgent 'TaylorBot-Deploy (https://taylorbot.app/, 0.3.0)' `
        -Headers @{ Authorization = "Bot $BotToken" } `
        -Method 'PUT' `
        -ContentType 'application/json' `
        -Body $merged

    Write-Output "$url ($($guildResponse.StatusCode))"
}
