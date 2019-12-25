FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj ./TaylorBot.Net.StatsTracker.Program/
COPY ./TaylorBot.Net.MinutesTracker.Domain/TaylorBot.Net.MinutesTracker.Domain.csproj ./TaylorBot.Net.MinutesTracker.Domain/
COPY ./TaylorBot.Net.MinutesTracker.Infrastructure/TaylorBot.Net.MinutesTracker.Infrastructure.csproj ./TaylorBot.Net.MinutesTracker.Infrastructure/
COPY ./TaylorBot.Net.MessagesTracker.Domain/TaylorBot.Net.MessagesTracker.Domain.csproj ./TaylorBot.Net.MessagesTracker.Domain/
COPY ./TaylorBot.Net.MessagesTracker.Infrastructure/TaylorBot.Net.MessagesTracker.Infrastructure.csproj ./TaylorBot.Net.MessagesTracker.Infrastructure/

RUN dotnet restore ./TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj

# copy and publish app and libraries
COPY . ./src/
RUN dotnet publish -c Release -o out ./src/TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.StatsTracker.Program.dll"]
