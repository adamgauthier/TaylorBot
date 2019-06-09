foreach ($solution in (Get-ChildItem . -Filter *.sln -Recurse)) {
    dotnet build $solution.FullName;
}