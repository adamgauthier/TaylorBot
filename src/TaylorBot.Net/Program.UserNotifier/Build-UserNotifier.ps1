param (
    [Parameter(Mandatory = $false)]
    [string]$ImageName = "taylorbot/user-notifier:dev"
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

docker build "$PSScriptRoot/.." --file "$PSScriptRoot/Dockerfile" --tag $ImageName
