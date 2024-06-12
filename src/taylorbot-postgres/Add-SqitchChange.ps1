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

C:\WINDOWS\system32\wsl.exe --distribution Ubuntu-20.04 -- `
    bash -c "export SQITCH_FULLNAME='$FullName' && export SQITCH_EMAIL='$Email' && ./sqitch.sh add --change-name '$FullChangeName' --note '$Note'"
