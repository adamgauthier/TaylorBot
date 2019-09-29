FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj ./TaylorBot.Net.EntityTracker.Program/
COPY ./TaylorBot.Net.QuickStart.Domain/TaylorBot.Net.QuickStart.Domain.csproj ./TaylorBot.Net.QuickStart.Domain/
COPY ./TaylorBot.Net.MemberLogging.Domain/TaylorBot.Net.MemberLogging.Domain.csproj ./TaylorBot.Net.MemberLogging.Domain/
COPY ./TaylorBot.Net.MemberLogging.Infrastructure/TaylorBot.Net.MemberLogging.Infrastructure.csproj ./TaylorBot.Net.MemberLogging.Infrastructure/
COPY ./TaylorBot.Net.EntityTracker.Domain/TaylorBot.Net.EntityTracker.Domain.csproj ./TaylorBot.Net.EntityTracker.Domain/
COPY ./TaylorBot.Net.EntityTracker.Infrastructure/TaylorBot.Net.EntityTracker.Infrastructure.csproj ./TaylorBot.Net.EntityTracker.Infrastructure/

RUN dotnet restore ./TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

# copy and publish app and libraries
COPY . ./src/
RUN dotnet publish -c Release -o out ./src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.EntityTracker.Program.dll"]
