# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
WORKDIR /app

ARG USE_TAYLORBOT_FEED=false
RUN if test "${USE_TAYLORBOT_FEED}" = "true"; then \
        curl -fsSL https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh -o /tmp/installcredprovider.sh && \
        sh /tmp/installcredprovider.sh && rm /tmp/installcredprovider.sh; \
    fi
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED=false

# copy csproj and restore as distinct layers
COPY ./Directory.Build.props ./Directory.Build.props
COPY ./Directory.Packages.props ./Directory.Packages.props
COPY ./NuGet.Config ./NuGet.Config
COPY ./.editorconfig ./.editorconfig
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.Commands.Discord/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.Commands.Discord/src/${file%.*}/ && mv $file Program.Commands.Discord/src/${file%.*}/; done

RUN --mount=type=secret,id=FEED_ACCESS_TOKEN \
    if test "${USE_TAYLORBOT_FEED}" = "true"; then \
        test -s /run/secrets/FEED_ACCESS_TOKEN || { echo "Private-feed builds require the FEED_ACCESS_TOKEN secret." >&2; exit 1; }; \
        export VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/adamgauthier/TaylorBot/_packaging/taylorbot-feed/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"$(cat /run/secrets/FEED_ACCESS_TOKEN)\"}]}"; \
    fi && \
    dotnet restore -p:TreatWarningsAsErrors=true -p:UseTaylorBotFeed=${USE_TAYLORBOT_FEED} ./Program.Commands.Discord/src/TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.Commands.Discord/src ./Program.Commands.Discord/src
RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true --no-restore ./Program.Commands.Discord/src/TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
RUN dotnet new sln --name Tests --output .

COPY ./Core/test ./Core/test
RUN dotnet sln ./Tests.slnx add ./Core/src/**/*.csproj ./Core/test/**/*.csproj ./Program.Commands.Discord/src/**/*.csproj

RUN --mount=type=secret,id=FEED_ACCESS_TOKEN \
    if test "${USE_TAYLORBOT_FEED}" = "true"; then \
        test -s /run/secrets/FEED_ACCESS_TOKEN || { echo "Private-feed builds require the FEED_ACCESS_TOKEN secret." >&2; exit 1; }; \
        export VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/adamgauthier/TaylorBot/_packaging/taylorbot-feed/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"$(cat /run/secrets/FEED_ACCESS_TOKEN)\"}]}"; \
    fi && \
    dotnet build --configuration Release -p:TreatWarningsAsErrors=true -p:UseTaylorBotFeed=${USE_TAYLORBOT_FEED} ./Tests.slnx
RUN dotnet format style ./Tests.slnx --no-restore --verify-no-changes --diagnostics IDE0001 IDE0002 IDE2002 --severity warn --verbosity quiet

ENTRYPOINT ["dotnet", "test", "Tests.slnx", "--no-build", "--configuration:Release", "--logger:trx", "--results-directory:/TestResults", "--collect:XPlat Code Coverage"]

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output out ./Program.Commands.Discord/src/TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.Commands.Discord.Program.dll"]
