param (
    [Parameter(Mandatory = $false)]
    [ValidateSet("Local", "Azure")]
    [string]$Environment = "Local",

    [Parameter(Mandatory = $false)]
    [string]$PostgresPassword,

    [Parameter(Mandatory = $false)]
    [string]$TaylorBotRolePassword,

    [Parameter(Mandatory = $false)]
    [string]$PostgresHost,

    [Parameter(Mandatory = $false)]
    [string]$PostgresPort = "5432",

    [Parameter(Mandatory = $false)]
    [string]$DatabaseBackupFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$config = Get-Content -Path "$PSScriptRoot/postgres.$Environment.json" -Raw | ConvertFrom-Json

$networkName = "taylorbot-network"
$networkExists = docker network ls --filter name="^${networkName}$" --format "{{.Name}}"
if (-not $networkExists) {
    docker network create $networkName
}

$mountsDir = Join-Path $PSScriptRoot "mounts"

if ([string]::IsNullOrWhiteSpace($PostgresPassword)) {
    $PostgresPassword = $config.PostgresPassword
}

if ([string]::IsNullOrWhiteSpace($TaylorBotRolePassword)) {
    $TaylorBotRolePassword = $config.TaylorBotRolePassword
}

if ([string]::IsNullOrWhiteSpace($PostgresHost)) {
    if ($Environment -eq "Azure") {
        Write-Output "Creating Azure Container Instance resource"

        $created = az container create `
            --resource-group $config.AzureResourceGroupName `
            --name $config.AzureResourceName `
            --image postgres:15 `
            --ports 5432 `
            --restart-policy Always `
            --os-type Linux `
            --ip-address Public `
            --secure-environment-variables "POSTGRES_PASSWORD=$PostgresPassword" `
            --cpu 1 `
            --memory 1 `
            --output json

        $createdParsed = $created | ConvertFrom-Json
        $PostgresHost = $createdParsed.ipAddress.ip
        $PostgresPort = "5432"

        Write-Output "Created resource with host $PostgresHost"

        Write-Output "Waiting for database server to be ready..."
        Start-Sleep -Seconds 60
    }
    else {
        Write-Output "Creating local container"

        $containerName = $config.ContainerName
        $currentDate = Get-Date -Format "yyyy.MM.dd-HH.mm.ss"
        $containerPath = "$mountsDir/$currentDate-$containerName"

        $dataPath = Join-Path $containerPath "pg-data"
        New-Item -ItemType Directory -Path $dataPath -Force | Out-Null

        $backupsPath = Join-Path $containerPath "pg-backups"
        New-Item -ItemType Directory -Path $backupsPath -Force | Out-Null

        $psqlrc = Join-Path $PSScriptRoot ".psqlrc"

        docker container run `
            --detach `
            --name $containerName `
            --network $networkName `
            --env "POSTGRES_PASSWORD=$PostgresPassword" `
            --mount "type=bind,source=$dataPath,destination=/var/lib/postgresql/data" `
            --mount "type=bind,source=$backupsPath,destination=/home/pg-backups" `
            --mount "type=bind,source=$psqlrc,destination=/root/.psqlrc,readonly" `
            --publish "$($config.PublishedPort):5432" `
            postgres:15

        $PostgresHost = $containerName
        $PostgresPort = "5432"

        Write-Host "Waiting for database server to be ready..."
        Start-Sleep -Seconds 30
    }
}

Write-Output "Creating taylorbot database and role"
$connection = "postgresql://postgres:$PostgresPassword@$($PostgresHost):$PostgresPort"

docker container run `
    --rm `
    --network $networkName `
    postgres:15 `
    psql --command="CREATE DATABASE taylorbot;" "$connection/postgres"

docker container run `
    --rm `
    --network $networkName `
    postgres:15 `
    psql --variable=ON_ERROR_STOP=1 --command="CREATE ROLE taylorbot WITH LOGIN PASSWORD '$TaylorBotRolePassword';GRANT ALL PRIVILEGES ON DATABASE taylorbot TO taylorbot;" "$connection/postgres"

docker container run `
    --rm `
    --network $networkName `
    postgres:15 `
    psql --command="CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;GRANT ALL ON SCHEMA public TO taylorbot;" "$connection/taylorbot"

if (-not [string]::IsNullOrWhiteSpace($DatabaseBackupFile)) {
    Write-Output "Restoring from backup"

    . "$PSScriptRoot/Run-PsqlFile.ps1" -ConnectionString $connection -SqlFile $DatabaseBackupFile
}

Write-Output "Deploying sqitch schema"

. "$PSScriptRoot/Deploy-Sqitch.ps1" -ConnectionString "postgresql://taylorbot:$TaylorBotRolePassword@$($PostgresHost):$PostgresPort"
