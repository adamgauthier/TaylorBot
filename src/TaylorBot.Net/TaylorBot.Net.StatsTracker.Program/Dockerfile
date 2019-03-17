FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.StatsTracker.sln ./
COPY ./TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj ./TaylorBot.Net.StatsTracker.Program/
COPY ./TaylorBot.Net.MinutesTracker.Domain/TaylorBot.Net.MinutesTracker.Domain.csproj ./TaylorBot.Net.MinutesTracker.Domain/
COPY ./TaylorBot.Net.MinutesTracker.Infrastructure/TaylorBot.Net.MinutesTracker.Infrastructure.csproj ./TaylorBot.Net.MinutesTracker.Infrastructure/

RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj
ENTRYPOINT ["dotnet", "TaylorBot.Net.StatsTracker.Program/out/TaylorBot.Net.StatsTracker.Program.dll"]
