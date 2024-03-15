param (
    # redis://@host:port/0
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $config = Get-Content -Path "$PSScriptRoot/redis.Local.json" -Raw | ConvertFrom-Json

    docker exec `
        --interactive `
        --tty $config.ContainerName `
        redis-cli -a $config.RedisPassword
}
else {
    $config = Get-Content -Path "$PSScriptRoot/redis.Azure.json" -Raw | ConvertFrom-Json

    docker container run `
        --rm `
        --interactive `
        redis redis-cli -u $ConnectionString -a $config.RedisPassword
}
