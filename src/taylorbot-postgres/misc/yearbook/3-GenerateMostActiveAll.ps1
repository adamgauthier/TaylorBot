param(
    [switch]$IncludeAllUsers
)

$extraArgs = if ($IncludeAllUsers) { "--include-all-users" } else { $null }
dotnet run "$PSScriptRoot\Yearbook.cs" -- query-all-activity $extraArgs
