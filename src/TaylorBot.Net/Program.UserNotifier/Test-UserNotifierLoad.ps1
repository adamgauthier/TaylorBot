param (
    [string]$Profile = (Join-Path $PSScriptRoot "test\profiles\smoke.json"),
    [string]$OutputDirectory,
    [ValidateRange(0.01, 64)]
    [decimal]$Cpu = 0.25,
    [ValidateRange(64, 65536)]
    [int]$MemoryMiB = 512,
    [string]$CaptureHeapAtPhase,
    [switch]$AllocationTrace,
    [switch]$SkipBuild
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$profilePath = (Resolve-Path $Profile).Path
$profileData = Get-Content $profilePath -Raw | ConvertFrom-Json
$deadlineSeconds = if ($profileData.PSObject.Properties.Name -contains "DeadlineSeconds") { $profileData.DeadlineSeconds } else { 4200 }
$name = "taylorbot-load-$([Guid]::NewGuid().ToString('N'))"
if (!$OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot ".load-results\$name" }
$results = (New-Item -ItemType Directory -Path $OutputDirectory -Force).FullName
if (@(Get-ChildItem -LiteralPath $results -Force).Count -ne 0) { throw "Output directory must be empty." }
$sdk = (Get-Content (Join-Path $dotnetRoot "global.json") -Raw | ConvertFrom-Json).sdk.version
$runtimeLine = Get-Content (Join-Path $PSScriptRoot "Dockerfile") | Where-Object { $_ -match '^FROM (\S+) AS runtime$' }
if (@($runtimeLine).Count -ne 1) { throw "Could not determine production runtime image." }
$runtime = [regex]::Match($runtimeLine, '^FROM (\S+) AS runtime$').Groups[1].Value
$workerImage = "taylorbot-load-worker:local"
$controllerImage = "taylorbot-load-controller:local"
$worker = "$name-worker"
$controller = "$name-controller"
$diagnostics = "$name-diagnostics"
$workerNetwork = "$name-worker-net"
$networkCreated = $false
$workerNetworkCreated = $false
$volumeCreated = $false
$heapCaptured = $false
$exitCode = 1

function Invoke-Docker {
    param ([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)
    & docker @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Docker command failed ($LASTEXITCODE): $($Arguments[0])" }
}

try {
    if (!$SkipBuild) {
        # The named context keeps schema sources outside the .NET build context.
        foreach ($target in @("worker", "controller")) {
            Invoke-Docker build --file (Join-Path $PSScriptRoot "test\Dockerfile.load") `
                --build-context "load-sqitch=$(Join-Path $dotnetRoot '..\taylorbot-postgres\sqitch')" `
                --build-arg "DOTNET_SDK_VERSION=$sdk" --build-arg "RUNTIME_IMAGE=$runtime" `
                --target $target --tag "taylorbot-load-${target}:local" $dotnetRoot
        }
    }
    if ($CaptureHeapAtPhase) {
        Invoke-Docker build --file (Join-Path $PSScriptRoot "test\Dockerfile.load") `
            --build-context "load-sqitch=$(Join-Path $dotnetRoot '..\taylorbot-postgres\sqitch')" `
            --build-arg "DOTNET_SDK_VERSION=$sdk" --build-arg "RUNTIME_IMAGE=$runtime" `
            --target profiler --tag taylorbot-load-profiler:local $dotnetRoot
        Set-Content -LiteralPath (Join-Path $results "capture-phase") -Value $CaptureHeapAtPhase -NoNewline
    }
    Invoke-Docker network create $name
    $networkCreated = $true
    Invoke-Docker network create --internal $workerNetwork
    $workerNetworkCreated = $true
    Invoke-Docker volume create $diagnostics
    $volumeCreated = $true
    Invoke-Docker run --detach --name $controller --network $name --network-alias load-controller `
        --mount "type=bind,source=$profilePath,target=/inputs/profile.json,readonly" `
        --mount "type=bind,source=$results,target=/results" `
        --mount "type=bind,source=/var/run/docker.sock,target=/var/run/docker.sock" `
        --env "TAYLORBOT_TEST_NETWORK=$name" --env TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal `
        --env TESTCONTAINERS_RYUK_DISABLED=true `
        --log-driver local --log-opt max-size=10m --log-opt max-file=2 $controllerImage
    $startup = [Diagnostics.Stopwatch]::StartNew()
    while (!(Test-Path (Join-Path $results "controller-ready"))) {
        $state = Invoke-Docker inspect --format '{{json .State}}' $controller | ConvertFrom-Json
        if (!$state.Running -or $startup.Elapsed.TotalSeconds -gt 300) {
            Invoke-Docker logs $controller
            throw "Load controller failed to initialize."
        }
        Start-Sleep -Seconds 1
    }
    Invoke-Docker network connect --alias load-controller $workerNetwork $controller
    $dataContainers = @(Invoke-Docker container ls --quiet --filter "network=$name")
    foreach ($dataContainer in $dataContainers) {
        if ($dataContainer -eq (Invoke-Docker inspect --format '{{.Id}}' $controller).Substring(0, 12)) { continue }
        $aliases = Invoke-Docker inspect --format "{{json (index .NetworkSettings.Networks `"$name`").Aliases}}" $dataContainer | ConvertFrom-Json
        $alias = @($aliases | Where-Object { $_ -in @("taylorbot-integration-postgres", "taylorbot-integration-redis") })
        if ($alias.Count -ne 1) { throw "Unexpected data service on task network." }
        Invoke-Docker network connect --alias $alias[0] $workerNetwork $dataContainer
    }
    $arguments = @(
        "run", "--detach", "--name", $worker, "--network", $workerNetwork,
        "--memory", "${MemoryMiB}m", "--memory-swap", "${MemoryMiB}m",
        "--cpus", $Cpu.ToString([Globalization.CultureInfo]::InvariantCulture),
        "--mount", "type=bind,source=$results,target=/results",
        "--mount", "type=volume,source=$diagnostics,target=/tmp",
        "--env", "DOTNET_ENVIRONMENT=LoadTest",
        "--log-driver", "local", "--log-opt", "max-size=10m", "--log-opt", "max-file=2"
    )
    if ($AllocationTrace) {
        $arguments += @("--env", "DOTNET_EnableEventPipe=1", "--env", "DOTNET_EventPipeOutputPath=/results/allocations.nettrace",
            "--env", "DOTNET_EventPipeConfig=Microsoft-Windows-DotNETRuntime:0x1:5")
    }
    $arguments += $workerImage
    @{
        WorkerImage = (Invoke-Docker image inspect --format '{{.Id}}' $workerImage)
        ControllerImage = (Invoke-Docker image inspect --format '{{.Id}}' $controllerImage)
        Runtime = $runtime; Sdk = $sdk; Cpu = $Cpu; MemoryMiB = $MemoryMiB
        HeapPhase = $CaptureHeapAtPhase; AllocationTrace = $AllocationTrace.IsPresent
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $results "environment.json")
    Invoke-Docker @arguments
    Invoke-Docker inspect --format '{{json .HostConfig}}' $worker | Set-Content -LiteralPath (Join-Path $results "limits.json")
    $run = [Diagnostics.Stopwatch]::StartNew()
    while ($true) {
        $state = Invoke-Docker inspect --format '{{json .State}}' $worker | ConvertFrom-Json
        $observation = @{ At = [DateTimeOffset]::UtcNow; State = $state }
        $observation | ConvertTo-Json -Depth 8 -Compress | Add-Content -LiteralPath (Join-Path $results "container.jsonl")
        if (!$state.Running) {
            $state | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $results "outcome.json")
            $exitCode = $state.ExitCode
            break
        }
        if ($run.Elapsed.TotalSeconds -gt $deadlineSeconds + 90) { throw "Load worker exceeded the run/shutdown deadline." }
        if ($CaptureHeapAtPhase -and !$heapCaptured -and (Test-Path (Join-Path $results "phase")) -and
            (Get-Content (Join-Path $results "phase") -Raw).Trim() -eq $CaptureHeapAtPhase) {
            Invoke-Docker run --name "$name-profiler" --pid "container:$worker" --memory 3g --memory-swap 3g --cpus 2 `
                --mount "type=volume,source=$diagnostics,target=/tmp" `
                --mount "type=bind,source=$results,target=/results" `
                taylorbot-load-profiler:local sh -c `
                '/tools/dotnet-gcdump --version > /results/profiler-version.txt && /tools/dotnet-gcdump collect --process-id 1 --timeout 180 --output /results/heap.gcdump && /tools/dotnet-gcdump report /results/heap.gcdump > /results/heap-types.txt'
            $heapCaptured = $true
            Set-Content -LiteralPath (Join-Path $results "capture-complete") -Value "complete"
        }
        Start-Sleep -Seconds 2
    }
    if ($exitCode -eq 0) {
        Invoke-Docker exec $controller dotnet TaylorBot.Net.UserNotifier.LoadController.dll verify
    }
    if ($CaptureHeapAtPhase -and !$heapCaptured) { throw "Requested heap phase was not reached." }
}
finally {
    foreach ($container in @("$name-profiler", $worker, $controller)) {
        $exists = @(docker container ls --all --quiet --filter "name=^/$container$")
        if ($LASTEXITCODE -ne 0) { throw "Could not inspect load containers during cleanup." }
        if ($exists) {
            $cleanupState = Invoke-Docker inspect --format '{{json .State}}' $container | ConvertFrom-Json
            if ($container -eq "$name-profiler") {
                $cleanupState | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $results "profiler-outcome.json")
            }
            if ($container -eq $worker -and !(Test-Path (Join-Path $results "outcome.json"))) {
                $cleanupState | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $results "aborted-worker-state.json")
            }
            docker logs $container 2>&1 | Set-Content -LiteralPath (Join-Path $results "$($container.Split('-')[-1]).log")
            Invoke-Docker stop --timeout 90 $container
            Invoke-Docker rm $container
        }
    }
    if ($networkCreated) {
        $containers = @(Invoke-Docker container ls --all --quiet --filter "network=$name")
        if ($containers) { Invoke-Docker container rm --force @containers }
        Invoke-Docker network rm $name
    }
    if ($workerNetworkCreated) { Invoke-Docker network rm $workerNetwork }
    if ($volumeCreated) { Invoke-Docker volume rm $diagnostics }
    $samplesPath = Join-Path $results "samples.jsonl"
    if (Test-Path $samplesPath) {
        $samples = @(Get-Content -LiteralPath $samplesPath | ConvertFrom-Json)
        $phases = @($samples | Group-Object Phase | ForEach-Object {
            @{
                Phase = $_.Name
                PeakCgroupMiB = [Math]::Round((($_.Group.Worker | Measure-Object -Property CgroupCurrent -Maximum).Maximum / 1MB), 2)
                PeakManagedMiB = [Math]::Round((($_.Group.Worker | Measure-Object -Property ManagedBytes -Maximum).Maximum / 1MB), 2)
                MaxPendingTasks = ($_.Group.Worker | Measure-Object -Property PendingTasks -Maximum).Maximum
            }
        })
        @{ Phases = $phases; LastSample = $samples | Select-Object -Last 1 } |
            ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $results "summary.json")
    }
}
Write-Host "Load artifacts: $results"
if ($exitCode -ne 0) { throw "Load worker failed ($exitCode). Inspect outcome.json and worker.log for managed OOM versus OOMKilled." }
