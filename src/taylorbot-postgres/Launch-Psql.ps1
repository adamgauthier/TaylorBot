param (
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $config = Get-Content -Path "$PSScriptRoot/postgres.Local.json" -Raw | ConvertFrom-Json

    docker exec `
        --interactive `
        --tty $config.ContainerName `
        psql --username=postgres --dbname=taylorbot
}
else {
    $psqlrc = Join-Path $PSScriptRoot ".psqlrc"

    docker container run `
        --rm `
        --interactive `
        --mount "type=bind,source=$psqlrc,destination=/root/.psqlrc,readonly" `
        postgres:15 `
        psql "$ConnectionString/taylorbot"
}
