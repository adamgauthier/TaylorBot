FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.Commands.Discord/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.Commands.Discord/src/${file%.*}/ && mv $file Program.Commands.Discord/src/${file%.*}/; done

RUN dotnet restore ./Program.Commands.Discord/src/TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj

# copy and publish app and libraries
COPY . .

RUN dotnet publish -c Release -o out ./Program.Commands.Discord/src/TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.Commands.Discord.Program.dll"]
