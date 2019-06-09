$solutionName = "TaylorBot.Net";
dotnet new sln --name $solutionName;

$solutionPath = "$solutionName.sln";
foreach ($project in (Get-ChildItem . -Filter *.csproj -Recurse)) {
    dotnet sln "$solutionName.sln" add $project.FullName;
}

(Start-Process "$solutionName.sln" -PassThru).WaitForExit();
Remove-Item -Path $solutionPath;