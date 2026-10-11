param (
    [Parameter(Mandatory = $false)]
    [string]$ImageName = "taylorbot/user-notifier:dev"
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$sdkVersion = (Get-Content (Join-Path $PSScriptRoot "..\global.json") -Raw | ConvertFrom-Json).sdk.version
docker build (Join-Path $PSScriptRoot "..") --file (Join-Path $PSScriptRoot "Dockerfile") --tag $ImageName --build-arg "DOTNET_SDK_VERSION=$sdkVersion"
if ($LASTEXITCODE -ne 0) { throw "User-notifier image build failed (exit code $LASTEXITCODE)." }
