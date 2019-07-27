FROM microsoft/dotnet:2.2-sdk
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

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./TaylorBot.Net.EntityTracker.Program/TaylorBot.Net.EntityTracker.Program.csproj
ENTRYPOINT ["dotnet", "TaylorBot.Net.EntityTracker.Program/out/TaylorBot.Net.EntityTracker.Program.dll"]
