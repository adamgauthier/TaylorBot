FROM mcr.microsoft.com/dotnet/sdk:5.0.400 AS build
WORKDIR /app

RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh

# copy csproj and restore as distinct layers
COPY ./Directory.Build.props ./Directory.Build.props
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.PostNotifier/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.PostNotifier/src/${file%.*}/ && mv $file Program.PostNotifier/src/${file%.*}/; done

ARG FEED_ACCESS_TOKEN
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/louistio/TaylorBot/_packaging/taylorbot-feed/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESS_TOKEN}\"}]}"
RUN dotnet restore ./Program.PostNotifier/src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj

# copy and build app and libraries
COPY ./Core/src ./Core/src
COPY ./Program.PostNotifier/src ./Program.PostNotifier/src
RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true --no-restore ./Program.PostNotifier/src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
RUN dotnet new sln --name Tests --output .

COPY ./Core/test ./Core/test
RUN dotnet sln ./Tests.sln add ./Core/test/**/*.csproj

COPY ./Program.PostNotifier/test ./Program.PostNotifier/test
RUN dotnet sln ./Tests.sln add ./Program.PostNotifier/test/**/*.csproj

RUN dotnet build --configuration Release -p:TreatWarningsAsErrors=true ./Tests.sln

ENTRYPOINT ["dotnet", "test", "Tests.sln", "--no-build", "--configuration:Release", "--logger:trx", "--results-directory:/TestResults", "--collect:\"XPlat Code Coverage\""]

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output out ./Program.PostNotifier/src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0.9 AS runtime
WORKDIR /app
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.PostNotifier.Program.dll"]
