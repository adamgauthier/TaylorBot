FROM mcr.microsoft.com/dotnet/sdk:7.0.103 AS build
WORKDIR /app

ARG FEED_ACCESS_TOKEN
RUN if test -n "${FEED_ACCESS_TOKEN}" ; then \
        curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh ; \
    fi
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/louistio/TaylorBot/_packaging/taylorbot-feed/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESS_TOKEN}\"}]}"

# copy csproj and restore as distinct layers
COPY ./Directory.Build.props ./Directory.Build.props
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.UserNotifier/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.UserNotifier/src/${file%.*}/ && mv $file Program.UserNotifier/src/${file%.*}/; done

RUN dotnet restore -p:TreatWarningsAsErrors=true ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.UserNotifier/src ./Program.UserNotifier/src
RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true --no-restore ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
RUN dotnet new sln --name Tests --output .

COPY ./Core/test ./Core/test
RUN dotnet sln ./Tests.sln add ./Core/test/**/*.csproj

COPY ./Program.UserNotifier/test ./Program.UserNotifier/test
RUN dotnet sln ./Tests.sln add ./Program.UserNotifier/test/**/*.csproj

RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true ./Tests.sln

ENTRYPOINT ["dotnet", "test", "Tests.sln", "--no-build", "--configuration:Release", "--logger:trx", "--results-directory:/TestResults", "--collect:\"XPlat Code Coverage\""]

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output out ./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

FROM mcr.microsoft.com/dotnet/runtime:7.0.3 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.UserNotifier.Program.dll"]
