$solutionName = "TaylorBot.Net";
dotnet new sln --name $solutionName --output $PSScriptRoot;

$solutionPath = "$PSScriptRoot/$solutionName.sln";

$allProjectPaths = (Get-ChildItem $PSScriptRoot -Filter *.csproj -Recurse) | Select-Object -ExpandProperty FullName;
dotnet sln $solutionPath add @allProjectPaths;

(Start-Process $solutionPath -PassThru).WaitForExit();
Remove-Item -Path $solutionPath;
