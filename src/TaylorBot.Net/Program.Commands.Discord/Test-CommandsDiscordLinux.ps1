param (
    [string]$Filter
)

& "$PSScriptRoot\..\Testing\Test-IntegrationLinux.ps1" -Application Commands.Discord -Filter $Filter
