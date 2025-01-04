param (
    [Parameter(Mandatory = $false)]
    [ValidateSet("Local", "Azure")]
    [string]$Environment = "Local"
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$config = Get-Content -Path "$PSScriptRoot/redis.$Environment.json" -Raw | ConvertFrom-Json

if ($Environment -eq "Azure") {
    if ([string]::IsNullOrWhiteSpace($config.AzureContainerAppEnvName)) {
        Write-Output "Creating Azure Container Instance resource"

        $created = az container create `
            --resource-group $config.AzureResourceGroupName `
            --name $config.AzureResourceName `
            --image redis:latest `
            --command-line "redis-server --requirepass $($config.RedisPassword)" `
            --ports 6379 `
            --restart-policy Always `
            --os-type Linux `
            --ip-address Public `
            --cpu 1 `
            --memory 1 `
            --output json

        $createdParsed = $created | ConvertFrom-Json
        $redisHost = $createdParsed.ipAddress.ip

        Write-Output "Created resource with host $redisHost"
    }
    else {
        Write-Output "Creating Azure Container App resource"

        az containerapp create `
            --name $config.AzureResourceName `
            --resource-group $config.AzureResourceGroupName `
            --environment $config.AzureContainerAppEnvName `
            --image redis:latest `
            --cpu $config.ContainerCpu `
            --memory $config.ContainerMemory `
            --target-port 6379 `
            --ingress internal `
            --transport tcp `
            --min-replicas 1 `
            --max-replicas 1 `
            --args "--requirepass $($config.RedisPassword)"
    }
}
else {
    Write-Output "Creating local container"

    $networkName = "taylorbot-network"
    $networkExists = docker network ls --filter name="^${networkName}$" --format "{{.Name}}"
    if (-not $networkExists) {
        docker network create $networkName
    }

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
        redis:latest redis-server --requirepass $config.RedisPassword
}
