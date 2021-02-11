FROM mcr.microsoft.com/dotnet/sdk:5.0.103 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Directory.Build.props ./Directory.Build.props
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.EntityTracker/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.EntityTracker/src/${file%.*}/ && mv $file Program.EntityTracker/src/${file%.*}/; done

RUN dotnet restore ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.EntityTracker/src ./Program.EntityTracker/src
RUN dotnet build --configuration Release --no-restore ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
RUN dotnet new sln --name Tests --output .

COPY ./Core/test ./Core/test
RUN dotnet sln ./Tests.sln add ./Core/test/**/*.csproj

COPY ./Program.EntityTracker/test ./Program.EntityTracker/test
RUN dotnet sln ./Tests.sln add ./Program.EntityTracker/test/**/*.csproj

RUN dotnet build --configuration Release ./Tests.sln

ENTRYPOINT ["dotnet", "test", "Tests.sln", "--no-build", "--configuration:Release", "--logger:trx", "--results-directory:/TestResults", "--collect:\"XPlat Code Coverage\""]

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output out ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0.3 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.EntityTracker.Program.dll"]

