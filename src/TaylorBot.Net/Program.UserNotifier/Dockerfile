FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.UserNotifier/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.UserNotifier/src/${file%.*}/ && mv $file Program.UserNotifier/src/${file%.*}/; done

RUN dotnet restore ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.UserNotifier/src ./Program.UserNotifier/src
RUN dotnet build -c Release --no-restore ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
RUN dotnet new sln --name Tests --output .

COPY ./Core/test ./Core/test
RUN dotnet sln ./Tests.sln add ./Core/test/**/*.csproj

COPY ./Program.UserNotifier/test ./Program.UserNotifier/test
RUN dotnet sln ./Tests.sln add ./Program.UserNotifier/test/**/*.csproj

ENTRYPOINT ["dotnet", "test", "Tests.sln", "--configuration:Release", "--logger:trx", "--results-directory:/TestResults", "--collect:\"XPlat Code Coverage\""]

FROM build AS publish
RUN dotnet publish -c Release --no-build --output out ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.UserNotifier.Program.dll"]
