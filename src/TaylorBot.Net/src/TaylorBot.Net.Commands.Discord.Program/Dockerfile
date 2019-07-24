FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./src/TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./src/TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./src/TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./src/TaylorBot.Net.Commands/TaylorBot.Net.Commands.csproj ./TaylorBot.Net.Commands/
COPY ./src/TaylorBot.Net.Commands.Infrastructure/TaylorBot.Net.Commands.Infrastructure.csproj ./TaylorBot.Net.Commands.Infrastructure/
COPY ./src/TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj ./TaylorBot.Net.Commands.Discord.Program/
COPY ./NuGet.config ./

RUN dotnet restore ./TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj

# copy and publish app and libraries
COPY ./src/TaylorBot.Net.Core ./TaylorBot.Net.Core
COPY ./src/TaylorBot.Net.Core.Infrastructure ./TaylorBot.Net.Core.Infrastructure
COPY ./src/TaylorBot.Net.Core.Program ./TaylorBot.Net.Core.Program
COPY ./src/TaylorBot.Net.Commands ./TaylorBot.Net.Commands
COPY ./src/TaylorBot.Net.Commands.Infrastructure ./TaylorBot.Net.Commands.Infrastructure
COPY ./src/TaylorBot.Net.Commands.Discord.Program ./TaylorBot.Net.Commands.Discord.Program

RUN dotnet publish -c Release -o out ./TaylorBot.Net.Commands.Discord.Program/TaylorBot.Net.Commands.Discord.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.Commands.Discord.Program.dll"]
