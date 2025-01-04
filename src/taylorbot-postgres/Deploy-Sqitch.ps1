param (
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString,

    [Parameter(Mandatory = $false)]
    [string]$BundleTarFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $config = Get-Content -Path "$PSScriptRoot/postgres.Local.json" -Raw | ConvertFrom-Json
    $ConnectionString = "postgresql://taylorbot:$($config.TaylorBotRolePassword)@$($config.ContainerName):5432"
}

$sqitchBundleDir = Join-Path $PSScriptRoot "mounts/sqitch"
if (Test-Path $sqitchBundleDir -PathType Container) { Remove-Item $sqitchBundleDir -Recurse -Force }

if (-not [string]::IsNullOrWhiteSpace($BundleTarFile)) {
    New-Item -ItemType Directory -Path $sqitchBundleDir | Out-Null

    Write-Output "Using tar archive as sqitch bundle $BundleTarFile"
    tar -xvf $BundleTarFile -C $sqitchBundleDir --strip-components=1
}
else {
    Write-Output "Using local sqitch source"
    $sqitchSource = Join-Path $PSScriptRoot "sqitch"
    docker container run `
        --rm `
        --mount "type=bind,source=$sqitchSource,destination=/repo,readonly" `
        --mount "type=bind,source=$sqitchBundleDir,destination=/out" `
        sqitch/sqitch:latest `
        bundle --dest-dir /out
}

$networkName = "taylorbot-network"
$networkExists = docker network ls --filter name="^${networkName}$" --format "{{.Name}}"
if (-not $networkExists) {
    docker network create $networkName
}

docker container run `
    --rm `
    --network $networkName `
    --mount "type=bind,source=$sqitchBundleDir,destination=/repo,readonly" `
    sqitch/sqitch:latest `
    deploy "db:$ConnectionString/taylorbot"
