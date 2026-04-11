param (
    [Parameter(Mandatory = $true)]
    [string]$ChangeName,

    [Parameter(Mandatory = $true)]
    [string]$Note,

    [Parameter(Mandatory = $false)]
    [string]$Email,

    [Parameter(Mandatory = $false)]
    [string]$FullName
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

[System.ArgumentException]::ThrowIfNullOrWhiteSpace($ChangeName, "ChangeName")
[System.ArgumentException]::ThrowIfNullOrWhiteSpace($Note, "Note")

if ([string]::IsNullOrWhiteSpace($Email)) {
    Write-Output "Getting email from git config"
    $Email = git config user.email
}
[System.ArgumentException]::ThrowIfNullOrWhiteSpace($Email, "Email")

if ([string]::IsNullOrWhiteSpace($FullName)) {
    Write-Output "Getting full name from git config"
    $FullName = git config user.name
}
[System.ArgumentException]::ThrowIfNullOrWhiteSpace($FullName, "FullName")

Set-Location "$PSScriptRoot/sqitch"

$Date = Get-Date -Format "yyyyMMdd"
$FullChangeName = "${Date}_${ChangeName}"

$env:SQITCH_FULLNAME = $FullName
$env:SQITCH_EMAIL = $Email

if ($env:OS -like "Windows*") {
    & cmd /c "sqitch.bat add --change-name `"$FullChangeName`" --note `"$Note`""
} else {
    & bash -c "./sqitch.sh add --change-name '$FullChangeName' --note '$Note'"
}
