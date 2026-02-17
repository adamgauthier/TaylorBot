param (
    [Parameter(Mandatory = $true)]
    [string]$CsvFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$CsvFile = Resolve-Path $CsvFile

Write-Output "Generating DOT file..."
dotnet run "$PSScriptRoot/GraphGenerator.cs" -- $CsvFile

$dotFile = [System.IO.Path]::ChangeExtension($CsvFile, ".dot")
$pngFile = [System.IO.Path]::ChangeExtension($CsvFile, ".png")

Write-Output "Rendering PNG..."
docker run --rm `
    -v "${dotFile}:/input.dot:ro" `
    -v "$(Split-Path $pngFile):/output" `
    nshine/dot dot -Tpng /input.dot -o "/output/$(Split-Path $pngFile -Leaf)"

Write-Output "Done! Output: $pngFile"
