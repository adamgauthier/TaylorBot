param (
    [Parameter(Mandatory = $false)]
    [ValidateSet("Local", "Azure")]
    [string]$Environment = "Local",

    [Parameter(Mandatory = $false)]
    [string]$ImageName = "taylorbot/commands-discord:dev",

    [Parameter(Mandatory = $false)]
    [string]$EnvFile,

    [Parameter(Mandatory = $false)]
    [string]$ImageFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($EnvFile)) {
    $EnvFile = Join-Path $PSScriptRoot "commands-discord.env"
}

if (-not [string]::IsNullOrWhiteSpace($ImageFile)) {
    Write-Output "Loading from file $ImageFile"
    $loadOutput = docker load --input $ImageFile
    $ImageName = $loadOutput.Trim().Split()[-1]
}

if ($Environment -eq "Azure") {
    Write-Output "Creating Azure Container Instance resource"

    $config = Get-Content -Path "$PSScriptRoot/commands-discord.$Environment.json" -Raw | ConvertFrom-Json
    $envContent = Get-Content $EnvFile

    $envVars = @()
    foreach ($line in $envContent) {
        $parts = $line -split '=', 2

        if ($parts.Count -eq 2) {
            $secureValue = ConvertTo-SecureString $parts[1] -AsPlainText -Force
            $envVars += New-AzContainerInstanceEnvironmentVariableObject -Name $parts[0] -SecureValue $secureValue
        }
        else {
            throw "Invalid line in .env file"
        }
    }

    $container = New-AzContainerInstanceObject `
        -Name $config.AzureResourceName `
        -Image $ImageName `
        -EnvironmentVariable $envVars

    New-AzContainerGroup `
        -Location $config.AzureLocation `
        -ResourceGroupName $config.AzureResourceGroupName `
        -Name $config.AzureResourceName `
        -RestartPolicy 'OnFailure' `
        -OSType 'Linux' `
        -Container $container `
        -RequestCpu 1 `
        -RequestMemoryInGb 1
}
else {
    Write-Output "Creating local container"

    $networkName = "taylorbot-network"
    try {
        docker network create $networkName
    } catch {}

    docker container run `
        --detach `
        --name taylorbot-commands-discord `
        --network $networkName `
        --env-file $EnvFile `
        --restart=on-failure:100 `
        $ImageName
}
