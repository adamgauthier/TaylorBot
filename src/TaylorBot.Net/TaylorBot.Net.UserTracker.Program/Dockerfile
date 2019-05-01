FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.UserTracker.sln ./
COPY ./TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./TaylorBot.Net.UserTracker.Program/TaylorBot.Net.UserTracker.Program.csproj ./TaylorBot.Net.UserTracker.Program/
COPY ./TaylorBot.Net.QuickStart.Domain/TaylorBot.Net.QuickStart.Domain.csproj ./TaylorBot.Net.QuickStart.Domain/
COPY ./TaylorBot.Net.UsernameTracker.Domain/TaylorBot.Net.UsernameTracker.Domain.csproj ./TaylorBot.Net.UsernameTracker.Domain/
COPY ./TaylorBot.Net.UsernameTracker.Infrastructure/TaylorBot.Net.UsernameTracker.Infrastructure.csproj ./TaylorBot.Net.UsernameTracker.Infrastructure/

RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./TaylorBot.Net.UserTracker.Program/TaylorBot.Net.UserTracker.Program.csproj
ENTRYPOINT ["dotnet", "TaylorBot.Net.UserTracker.Program/out/TaylorBot.Net.UserTracker.Program.dll"]
