param (
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString,

    [Parameter(Mandatory = $false)]
    [switch]$SchemaOnly
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$currentDate = Get-Date -Format "yyyy.MM.dd-HH.mm.ss"
$dumpType = $SchemaOnly.IsPresent ? "schema" : "data"
$dumpFileName = "$currentDate-$dumpType-dump.sql"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $config = Get-Content -Path "$PSScriptRoot/postgres.Local.json" -Raw | ConvertFrom-Json

    $commandArgs = @(
        "pg_dump"
        "--username"
        "postgres"
        "--dbname"
        "taylorbot"
        "--file"
        "/home/pg-backups/$dumpFileName"
    )

    if ($SchemaOnly.IsPresent) {
        $commandArgs += "--schema-only", "--no-comments", "--no-owner", "--no-privileges"
    }

    docker container exec $config.ContainerName $commandArgs
}
else {
    $networkName = "taylorbot-network"
    $networkExists = docker network ls --filter name="^${networkName}$" --format "{{.Name}}"
    if (-not $networkExists) {
        docker network create $networkName
    }

    $mountsDir = Join-Path $PSScriptRoot "mounts"
    Write-Output "Dumping to $mountsDir/$dumpFileName"

    $commandArgs = @(
        "pg_dump"
        "--file"
        "/out/$dumpFileName"
    )

    if ($SchemaOnly.IsPresent) {
        $commandArgs += "--schema-only", "--no-comments", "--no-owner", "--no-privileges"
    }

    $commandArgs += "$ConnectionString/taylorbot"

    docker container run `
        --rm `
        --mount "type=bind,source=$mountsDir,destination=/out" `
        --network $networkName `
        postgres:15 `
        $commandArgs
}
