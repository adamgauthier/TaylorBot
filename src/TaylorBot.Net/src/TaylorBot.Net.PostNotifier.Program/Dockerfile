FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.PostNotifier.sln ./
COPY ./TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj ./TaylorBot.Net.PostNotifier.Program/
COPY ./TaylorBot.Net.RedditNotifier.Domain/TaylorBot.Net.RedditNotifier.Domain.csproj ./TaylorBot.Net.RedditNotifier.Domain/
COPY ./TaylorBot.Net.RedditNotifier.Infrastructure/TaylorBot.Net.RedditNotifier.Infrastructure.csproj ./TaylorBot.Net.RedditNotifier.Infrastructure/

RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj
ENTRYPOINT ["dotnet", "TaylorBot.Net.PostNotifier.Program/out/TaylorBot.Net.PostNotifier.Program.dll"]
