$solutionName = "TaylorBot.Net";
$solutionPath = "$PSScriptRoot/$solutionName.sln";

if (Test-Path $solutionPath) {
    Remove-Item $solutionPath
}

dotnet new sln --name $solutionName --output $PSScriptRoot;

$allProjectPaths = (Get-ChildItem $PSScriptRoot -Filter *.csproj -Recurse) | Select-Object -ExpandProperty FullName;
dotnet sln $solutionPath add @allProjectPaths;

(Start-Process $solutionPath -PassThru).WaitForExit();
Remove-Item -Path $solutionPath;
