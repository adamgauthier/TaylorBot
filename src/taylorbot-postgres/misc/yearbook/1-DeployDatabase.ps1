param(
    [Parameter(Mandatory=$true)]
    [string]$DatabaseBackupFile,

    [Parameter(Mandatory=$true)]
    [ValidateSet("PreviousYear", "CurrentYear")]
    [string]$Dump
)

$config = Get-Content -Path "$PSScriptRoot/yearbook.json" -Raw | ConvertFrom-Json
$dumpConfig = if ($Dump -eq "PreviousYear") { $config.dumpPreviousYear } else { $config.dumpCurrentYear }

& "$PSScriptRoot\..\..\Deploy-Postgres.ps1" -Environment Local -SkipSqitch `
    -DatabaseBackupFile $DatabaseBackupFile `
    -ContainerName "yearbook-$Dump" `
    -PublishedPort $dumpConfig.port `
    -TaylorBotRolePassword $dumpConfig.password `
    -ContinueOnError `
    -LocalRestore
