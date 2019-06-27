$solutionName = "TaylorBot.Net";
dotnet new sln --name $solutionName;

$solutionPath = "$solutionName.sln";

$allProjectPaths = (Get-ChildItem . -Filter *.csproj -Recurse) | Select-Object -ExpandProperty FullName;
dotnet sln "$solutionName.sln" add @allProjectPaths;

(Start-Process "$solutionName.sln" -PassThru).WaitForExit();
Remove-Item -Path $solutionPath;
