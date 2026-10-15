# syntax=docker/dockerfile:1
# check=skip=InvalidDefaultArgInFrom
# The required SDK argument is supplied from global.json by build entrypoints.
ARG DOTNET_SDK_VERSION
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS build
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
COPY ./global.json ./global.json
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.UserNotifier/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.UserNotifier/src/${file%.*}/ && mv $file Program.UserNotifier/src/${file%.*}/; done

RUN --mount=type=secret,id=FEED_ACCESS_TOKEN \
    if test "${USE_TAYLORBOT_FEED}" = "true"; then \
        test -s /run/secrets/FEED_ACCESS_TOKEN || { echo "Private-feed builds require the FEED_ACCESS_TOKEN secret." >&2; exit 1; }; \
        export VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/adamgauthier/TaylorBot/_packaging/taylorbot-feed/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"$(cat /run/secrets/FEED_ACCESS_TOKEN)\"}]}"; \
    fi && \
    dotnet restore -p:TreatWarningsAsErrors=true -p:UseTaylorBotFeed=${USE_TAYLORBOT_FEED} ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.UserNotifier/src ./Program.UserNotifier/src
RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true --no-restore ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output out ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.UserNotifier.Program.dll"]
