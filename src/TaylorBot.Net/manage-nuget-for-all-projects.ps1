$solutionName = "TaylorBot.Net";
dotnet new sln --name $solutionName --output $PSScriptRoot;

$solutionPath = "$PSScriptRoot/$solutionName.sln";

$allProjectPaths = (Get-ChildItem $PSScriptRoot -Filter *.csproj -Recurse) | Select-Object -ExpandProperty FullName;
dotnet sln $solutionPath add @allProjectPaths;

Start-Process $solutionPath -Wait;
Remove-Item -Path $solutionPath;
