param (
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$currentDate = Get-Date -Format "yyyy.MM.dd-HH.mm.ss"
$dumpFileName = "$($currentDate)_dump.sql"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $config = Get-Content -Path "$PSScriptRoot/postgres.Local.json" -Raw | ConvertFrom-Json

    docker exec $config.ContainerName `
        pg_dump --username postgres --dbname taylorbot --file "/home/pg-backups/$dumpFileName"
}
else {
    $mountsDir = Join-Path $PSScriptRoot "mounts"
    Write-Output "Dumping to $mountsDir/$dumpFileName"

    docker container run `
        --rm `
        --mount "type=bind,source=$mountsDir,destination=/out" `
        postgres:15 `
        pg_dump --file "/out/$dumpFileName" "$ConnectionString/taylorbot"
}
