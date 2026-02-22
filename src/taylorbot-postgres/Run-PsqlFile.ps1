param (
    [Parameter(Mandatory = $true)]
    [string]$SqlFile,

    [Parameter(Mandatory = $false)]
    [string]$ConnectionString,

    [Parameter(Mandatory = $false)]
    [switch]$ContinueOnError
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    Write-Output "Using local container as target"

    $config = Get-Content -Path "$PSScriptRoot/postgres.Local.json" -Raw | ConvertFrom-Json

    $ConnectionString = "postgresql://postgres:$($config.PostgresPassword)@$($config.ContainerName):5432"
}

$networkName = "taylorbot-network"
$networkExists = docker network ls --filter name="^${networkName}$" --format "{{.Name}}"
if (-not $networkExists) {
    docker network create $networkName
}

$sqlFileName = Split-Path -Path $SqlFile -Leaf

if ($ContinueOnError) {
    docker container run `
        --rm `
        --network $networkName `
        --mount "type=bind,source=$SqlFile,destination=/sqlscripts/$sqlFileName,readonly" `
        postgres:15 `
        psql --file=/sqlscripts/$sqlFileName "$ConnectionString/taylorbot"
}
else {
    docker container run `
        --rm `
        --network $networkName `
        --mount "type=bind,source=$SqlFile,destination=/sqlscripts/$sqlFileName,readonly" `
        postgres:15 `
        psql --variable=ON_ERROR_STOP=1 --file=/sqlscripts/$sqlFileName "$ConnectionString/taylorbot"
}
