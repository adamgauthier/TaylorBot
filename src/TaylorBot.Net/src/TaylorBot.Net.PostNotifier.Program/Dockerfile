FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.PostNotifier.sln ./
COPY ./src/TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./src/TaylorBot.Net.Core/
COPY ./src/TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./src/TaylorBot.Net.Core.Infrastructure/
COPY ./src/TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./src/TaylorBot.Net.Core.Program/
COPY ./src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj ./src/TaylorBot.Net.PostNotifier.Program/
COPY ./src/TaylorBot.Net.RedditNotifier.Domain/TaylorBot.Net.RedditNotifier.Domain.csproj ./src/TaylorBot.Net.RedditNotifier.Domain/
COPY ./src/TaylorBot.Net.RedditNotifier.Infrastructure/TaylorBot.Net.RedditNotifier.Infrastructure.csproj ./src/TaylorBot.Net.RedditNotifier.Infrastructure/
COPY ./src/TaylorBot.Net.YoutubeNotifier.Domain/TaylorBot.Net.YoutubeNotifier.Domain.csproj ./src/TaylorBot.Net.YoutubeNotifier.Domain/
COPY ./src/TaylorBot.Net.YoutubeNotifier.Infrastructure/TaylorBot.Net.YoutubeNotifier.Infrastructure.csproj ./src/TaylorBot.Net.YoutubeNotifier.Infrastructure/
COPY ./src/TaylorBot.Net.TumblrNotifier.Domain/TaylorBot.Net.TumblrNotifier.Domain.csproj ./src/TaylorBot.Net.TumblrNotifier.Domain/
COPY ./src/TaylorBot.Net.TumblrNotifier.Infrastructure/TaylorBot.Net.TumblrNotifier.Infrastructure.csproj ./src/TaylorBot.Net.TumblrNotifier.Infrastructure/

RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj
ENTRYPOINT ["dotnet", "src/TaylorBot.Net.PostNotifier.Program/out/TaylorBot.Net.PostNotifier.Program.dll"]
