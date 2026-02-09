param(
    [Parameter(Mandatory=$true)]
    [string]$DatabaseBackupFile
)

& "$PSScriptRoot\..\Deploy-Postgres.ps1" -Environment Local -SkipSqitch -DatabaseBackupFile $DatabaseBackupFile
