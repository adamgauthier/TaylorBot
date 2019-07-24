FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./src/TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./src/TaylorBot.Net.Core/
COPY ./src/TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./src/TaylorBot.Net.Core.Infrastructure/
COPY ./src/TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./src/TaylorBot.Net.Core.Program/
COPY ./src/TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj ./src/TaylorBot.Net.StatsTracker.Program/
COPY ./src/TaylorBot.Net.MinutesTracker.Domain/TaylorBot.Net.MinutesTracker.Domain.csproj ./src/TaylorBot.Net.MinutesTracker.Domain/
COPY ./src/TaylorBot.Net.MinutesTracker.Infrastructure/TaylorBot.Net.MinutesTracker.Infrastructure.csproj ./src/TaylorBot.Net.MinutesTracker.Infrastructure/
COPY ./src/TaylorBot.Net.MessagesTracker.Domain/TaylorBot.Net.MessagesTracker.Domain.csproj ./src/TaylorBot.Net.MessagesTracker.Domain/
COPY ./src/TaylorBot.Net.MessagesTracker.Infrastructure/TaylorBot.Net.MessagesTracker.Infrastructure.csproj ./src/TaylorBot.Net.MessagesTracker.Infrastructure/

RUN dotnet restore ./src/TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./src/TaylorBot.Net.StatsTracker.Program/TaylorBot.Net.StatsTracker.Program.csproj
ENTRYPOINT ["dotnet", "src/TaylorBot.Net.StatsTracker.Program/out/TaylorBot.Net.StatsTracker.Program.dll"]
