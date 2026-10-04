param (
    [string]$Filter
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repository = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
$dockerfile = Get-Content (Join-Path $PSScriptRoot "Dockerfile") -Raw
if ($dockerfile -notmatch '(?m)^FROM\s+(mcr\.microsoft\.com/dotnet/sdk:\S+)\s+AS\s+build\s*$') {
    throw "Cannot find the SDK build image in the commands-discord Dockerfile."
}
$sdkImage = $Matches[1]
$name = "taylorbot-integration-linux-$([Guid]::NewGuid().ToString('N'))"
$networkCreated = $false
$runAttempted = $false
$testExitCode = 1

try {
    docker network create $name
    if ($LASTEXITCODE -ne 0) { throw "Could not create Linux test network." }
    $networkCreated = $true

    $arguments = @(
        "run", "--rm", "--name", $name, "--network", $name,
        "--mount", "type=bind,source=$repository,target=/repo,readonly",
        "--mount", "type=bind,source=/var/run/docker.sock,target=/var/run/docker.sock",
        "--env", "TAYLORBOT_TEST_NETWORK=$name",
        "--env", "TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal",
        "--env", "DOTNET_CLI_TELEMETRY_OPTOUT=1",
        "--workdir", "/repo/src/TaylorBot.Net",
        $sdkImage,
        "dotnet", "test", "TaylorBot.Net.Commands.Discord.slnx",
        "--configuration", "Release", "--artifacts-path", "/tmp/taylorbot-artifacts",
        "--verbosity", "minimal", "-p:TreatWarningsAsErrors=true", "-p:UseTaylorBotFeed=false"
    )
    if ($Filter) {
        $arguments += "--filter", $Filter
    }
    $runAttempted = $true
    docker @arguments
    $testExitCode = $LASTEXITCODE
}
finally {
    if ($runAttempted) {
        $runner = @(docker container ls --all --quiet --no-trunc --filter "name=^/$name$")
        if ($LASTEXITCODE -ne 0) { throw "Could not inspect the Linux test runner during cleanup." }
        if ($runner) {
            docker container rm --force @runner
            if ($LASTEXITCODE -ne 0) { throw "Could not remove the Linux test runner." }
        }
    }
    if ($networkCreated) {
        $containers = @(docker container ls --all --quiet --no-trunc --filter "network=$name")
        if ($LASTEXITCODE -ne 0) { throw "Could not inspect Linux test containers during cleanup." }
        if ($containers) {
            docker container rm --force @containers
            if ($LASTEXITCODE -ne 0) { throw "Could not remove containers on the isolated Linux test network." }
        }
        docker network rm $name
        if ($LASTEXITCODE -ne 0) { throw "Could not remove the Linux test network." }
    }
}

if ($testExitCode -ne 0) {
    throw "Linux tests failed (exit code $testExitCode)."
}
