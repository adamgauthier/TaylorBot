param (
    [Parameter(Mandatory = $false)]
    [ValidateSet("Local", "Azure")]
    [string]$Environment = "Local",

    [Parameter(Mandatory = $false)]
    [string]$ImageName = "taylorbot/commands-discord:dev",

    [Parameter(Mandatory = $false)]
    [bool]$IsProduction = $false,

    [Parameter(Mandatory = $false)]
    [string]$ImageFile,

    [Parameter(Mandatory = $false)]
    [string]$Secrets,

    [Parameter(Mandatory = $false)]
    [string]$SecretsFile,

    [Parameter(Mandatory = $false)]
    [string]$AzureConfigJson,

    [Parameter(Mandatory = $false)]
    [string]$AzureConfigFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../Deploy-TaylorBotNetComponent.ps1" `
    -Environment $Environment `
    -ImageName $ImageName `
    -EnvFile "$PSScriptRoot/commands-discord.env" `
    -IsProduction $IsProduction `
    -ImageFile $ImageFile `
    -Secrets $Secrets `
    -SecretsFile $SecretsFile `
    -AzureConfigJson $AzureConfigJson `
    -AzureConfigFile $AzureConfigFile `
    -LocalContainerName "taylorbot-commands-discord"
