FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.EntityTracker.sln ./
COPY ./src/TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./src/TaylorBot.Net.Core/
COPY ./src/TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./src/TaylorBot.Net.Core.Infrastructure/
COPY ./src/TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./src/TaylorBot.Net.Core.Program/
COPY ./src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj ./src/TaylorBot.Net.EntityTracker.Program/
COPY ./src/TaylorBot.Net.QuickStart.Domain/TaylorBot.Net.QuickStart.Domain.csproj ./src/TaylorBot.Net.QuickStart.Domain/
COPY ./src/TaylorBot.Net.MemberLogging.Domain/TaylorBot.Net.MemberLogging.Domain.csproj ./src/TaylorBot.Net.MemberLogging.Domain/
COPY ./src/TaylorBot.Net.MemberLogging.Infrastructure/TaylorBot.Net.MemberLogging.Infrastructure.csproj ./src/TaylorBot.Net.MemberLogging.Infrastructure/
COPY ./src/TaylorBot.Net.EntityTracker.Domain/TaylorBot.Net.EntityTracker.Domain.csproj ./src/TaylorBot.Net.EntityTracker.Domain/
COPY ./src/TaylorBot.Net.EntityTracker.Infrastructure/TaylorBot.Net.EntityTracker.Infrastructure.csproj ./src/TaylorBot.Net.EntityTracker.Infrastructure/

RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./src/TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj
ENTRYPOINT ["dotnet", "src/TaylorBot.Net.EntityTracker.Program/out/TaylorBot.Net.EntityTracker.Program.dll"]
