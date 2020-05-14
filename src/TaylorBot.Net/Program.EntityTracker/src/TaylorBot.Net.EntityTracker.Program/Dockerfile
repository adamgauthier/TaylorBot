FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.EntityTracker/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.EntityTracker/src/${file%.*}/ && mv $file Program.EntityTracker/src/${file%.*}/; done

RUN dotnet restore ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

# copy and publish app and libraries
COPY . .

RUN dotnet publish -c Release -o out ./Program.EntityTracker/src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.EntityTracker.Program.dll"]
