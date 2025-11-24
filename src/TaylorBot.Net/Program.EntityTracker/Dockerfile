FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /app

ARG FEED_ACCESS_TOKEN
RUN if test -n "${FEED_ACCESS_TOKEN}" ; then \
        curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh ; \
    fi
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/adamgauthier/TaylorBot/_packaging/taylorbot-feed/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESS_TOKEN}\"}]}"

# copy csproj and restore as distinct layers
COPY ./Directory.Build.props ./Directory.Build.props
COPY ./.editorconfig ./.editorconfig
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.EntityTracker/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.EntityTracker/src/${file%.*}/ && mv $file Program.EntityTracker/src/${file%.*}/; done

RUN dotnet restore -p:TreatWarningsAsErrors=true -p:UseTaylorBotFeed=$(test -n "${FEED_ACCESS_TOKEN}" && echo "true" || echo "false") ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.EntityTracker/src ./Program.EntityTracker/src
RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true --no-restore ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
RUN dotnet new sln --name Tests --output .

COPY ./Core/test ./Core/test
RUN dotnet sln ./Tests.slnx add ./Core/test/**/*.csproj

COPY ./Program.EntityTracker/test ./Program.EntityTracker/test
RUN dotnet sln ./Tests.slnx add ./Program.EntityTracker/test/**/*.csproj

RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true ./Tests.slnx

ENTRYPOINT ["dotnet", "test", "Tests.slnx", "--no-build", "--configuration:Release", "--logger:trx", "--results-directory:/TestResults", "--collect:XPlat Code Coverage"]

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output out ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0.0 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.EntityTracker.Program.dll"]
