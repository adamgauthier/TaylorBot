param(
    [switch]$SortByMinutes
)

$extraArgs = if ($SortByMinutes) { "--sort-by-minutes" } else { $null }
dotnet run "$PSScriptRoot\Yearbook.cs" -- query-new-activity $extraArgs
