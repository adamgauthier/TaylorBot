param (
    [Parameter(Mandatory = $false)]
    [ValidateSet("Local", "Azure")]
    [string]$Environment = "Local",

    [Parameter(Mandatory = $true)]
    [string]$EnvFile,

    [Parameter(Mandatory = $false)]
    [string]$ImageName,

    [Parameter(Mandatory = $false)]
    [string]$ImageFile,

    [Parameter(Mandatory = $false)]
    [bool]$IsProduction = $false,

    [Parameter(Mandatory = $false)]
    [string]$Secrets,

    [Parameter(Mandatory = $false)]
    [string]$SecretsFile,

    [Parameter(Mandatory = $false)]
    [string]$AzureConfigJson,

    [Parameter(Mandatory = $false)]
    [string]$AzureConfigFile,

    [Parameter(Mandatory = $false)]
    [string]$LocalContainerName
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Output "Loading envVars from $EnvFile"
$envVars = [System.IO.File]::ReadAllLines($EnvFile)

Write-Output "Found $($envVars.Length) environment variables"
[System.ArgumentOutOfRangeException]::ThrowIfLessThan($envVars.Length, 2, "envVarsLength")

if ($IsProduction) {
    $envVars = $envVars -Replace 'DOTNET_ENVIRONMENT=Staging', 'DOTNET_ENVIRONMENT=Production'
    Write-Output "Replaced envVars to $envVars"
}

if (-not [string]::IsNullOrWhiteSpace($SecretsFile)) {
    $currentPath = (Get-Location).Path
    $secretsFilePath = Resolve-Path (Join-Path $currentPath $SecretsFile)
    Write-Output "Loading Secrets from $secretsFilePath"
    $Secrets = [string]::Join(" ", [System.IO.File]::ReadAllLines($secretsFilePath))
}
[System.ArgumentException]::ThrowIfNullOrWhiteSpace($Secrets, "Secrets")
$ParsedSecrets = $Secrets.Split(" ")
Write-Output "Found $($ParsedSecrets.Length) secrets"
[System.ArgumentOutOfRangeException]::ThrowIfLessThan($ParsedSecrets.Length, 2, "secretsLength")

if (-not [string]::IsNullOrWhiteSpace($ImageFile)) {
    Write-Output "Loading from file $ImageFile"
    $loadOutput = docker load --input $ImageFile
    $ImageName = $loadOutput.Trim().Split()[-1]
}
[System.ArgumentException]::ThrowIfNullOrWhiteSpace($ImageName, "ImageName")
Write-Output "Image name is $ImageName"

if ($Environment -eq "Azure") {
    Write-Output "Loading Azure config"

    if ([string]::IsNullOrWhiteSpace($AzureConfigJson)) {
        [System.ArgumentException]::ThrowIfNullOrWhiteSpace($AzureConfigFile, "AzureConfigFile")
        Write-Output "Loading AzureConfigJson from $AzureConfigFile"
        $AzureConfigJson = Get-Content -Path $AzureConfigFile -Raw
    }
    [System.ArgumentException]::ThrowIfNullOrWhiteSpace($AzureConfigJson, "AzureConfigJson")
    $config = $AzureConfigJson | ConvertFrom-Json

    Write-Output "Checking if Azure Container App exists"

    $existingAppName = az containerapp show `
        --name $config.AzureResourceName `
        --resource-group $config.AzureResourceGroupName `
        --output tsv --query "name" 2>$null

    if (-not [string]::IsNullOrEmpty($existingAppName)) {
        Write-Output "Azure Container App already exists, updating"

        az containerapp secret set `
            --name $config.AzureResourceName `
            --resource-group $config.AzureResourceGroupName `
            --secrets $ParsedSecrets

        az containerapp update `
            --name $config.AzureResourceName `
            --resource-group $config.AzureResourceGroupName `
            --image $ImageName `
            --replace-env-vars $envVars `
            --cpu $config.ContainerCpu `
            --memory $config.ContainerMemory `
            --min-replicas 1 `
            --max-replicas 1
    } else {
        Write-Output "Azure Container App doesn't exist, creating"

        az containerapp create `
            --name $config.AzureResourceName `
            --resource-group $config.AzureResourceGroupName `
            --environment $config.AzureContainerAppEnvName `
            --image $ImageName `
            --secrets $ParsedSecrets `
            --env-vars $envVars `
            --cpu $config.ContainerCpu `
            --memory $config.ContainerMemory `
            --min-replicas 1 `
            --max-replicas 1
    }
}
else {
    Write-Output "Creating local container"
    [System.ArgumentException]::ThrowIfNullOrWhiteSpace($LocalContainerName, "LocalContainerName")

    $networkName = "taylorbot-network"
    $networkExists = docker network ls --filter name="^${networkName}$" --format "{{.Name}}"
    if (-not $networkExists) {
        docker network create $networkName
    }

    $secretsLookup = @{}
    foreach ($line in $ParsedSecrets) {
        if ($line -match "^(.*?)=(.*)$") {
            $secretsLookup[$matches[1]] = $matches[2]
        }
        else {
            throw "Invalid secrets"
        }
    }

    $generatedenvVars = $envVars -Replace 'secretref:([^\s,]+)', {
        $secret = $_.Groups[1].Value
        if ($secretsLookup.ContainsKey($secret)) {
            $secretsLookup[$secret]
        } else {
            throw "Can't find $secret"
        }
    }

    $generatedFile = $EnvFile.Replace(".env", ".generated.secrets.env")
    Write-Output "Writing $generatedFile"

    $generatedenvVars -Split ' ' | Out-File -FilePath $generatedFile -Encoding utf8

    docker container run `
        --detach `
        --name $LocalContainerName `
        --network $networkName `
        --env-file $generatedFile `
        --restart=on-failure:100 `
        $ImageName
}
