param (
    [Parameter(Mandatory = $false)]
    [ValidateSet("Local", "Azure")]
    [string]$Environment = "Local"
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$config = Get-Content -Path "$PSScriptRoot/redis.$Environment.json" -Raw | ConvertFrom-Json
$networkName = Get-Content (Join-Path $PSScriptRoot "..\linux-infrastructure\docker-network.name")

if ($Environment -eq "Azure") {
    Write-Output "Creating Azure Container Instance resource"

    $created = az container create `
        --resource-group $config.AzureResourceGroupName `
        --name $config.AzureResourceName `
        --image redis `
        --command-line "redis-server --requirepass $($config.RedisPassword) --save 3600 1 --loglevel warning" `
        --ports 6379 `
        --restart-policy Always `
        --os-type Linux `
        --ip-address Public `
        --output json

    $createdParsed = $created | ConvertFrom-Json
    $redisHost = $createdParsed.ipAddress.ip

    Write-Output "Created resource with host $redisHost"
}
else {
    Write-Output "Creating local container"

    $containerName = $config.ContainerName
    $currentDate = Get-Date -Format "yyyy.MM.dd-HH.mm.ss"

    $mountsDir = Join-Path $PSScriptRoot "mounts"
    $containerPath = "$mountsDir/$currentDate-$containerName"

    $dataPath = Join-Path $containerPath "data"
    New-Item -ItemType Directory -Path $dataPath -Force | Out-Null

    docker container run `
        --detach `
        --name $containerName `
        --network $networkName `
        --mount "type=bind,source=$dataPath,destination=/data" `
        --publish "$($config.PublishedPort):6379" `
        redis redis-server --requirepass $config.RedisPassword --save 3600 1 --loglevel warning
}
