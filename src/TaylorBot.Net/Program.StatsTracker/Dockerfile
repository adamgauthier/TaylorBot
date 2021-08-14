FROM mcr.microsoft.com/dotnet/sdk:5.0.400 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Directory.Build.props ./Directory.Build.props
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.StatsTracker/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.StatsTracker/src/${file%.*}/ && mv $file Program.StatsTracker/src/${file%.*}/; done

RUN dotnet restore ./Program.StatsTracker/src/TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.StatsTracker/src ./Program.StatsTracker/src
RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true --no-restore ./Program.StatsTracker/src/TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
RUN dotnet new sln --name Tests --output .

COPY ./Core/test ./Core/test
RUN dotnet sln ./Tests.sln add ./Core/test/**/*.csproj

COPY ./Program.StatsTracker/test ./Program.StatsTracker/test
RUN dotnet sln ./Tests.sln add ./Program.StatsTracker/test/**/*.csproj

RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true ./Tests.sln

ENTRYPOINT ["dotnet", "test", "Tests.sln", "--no-build", "--configuration:Release", "--logger:trx", "--results-directory:/TestResults", "--collect:\"XPlat Code Coverage\""]

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output out ./Program.StatsTracker/src/TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0.9 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.StatsTracker.Program.dll"]

