FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj ./TaylorBot.Net.PostNotifier.Program/
COPY ./TaylorBot.Net.RedditNotifier.Domain/TaylorBot.Net.RedditNotifier.Domain.csproj ./TaylorBot.Net.RedditNotifier.Domain/
COPY ./TaylorBot.Net.RedditNotifier.Infrastructure/TaylorBot.Net.RedditNotifier.Infrastructure.csproj ./TaylorBot.Net.RedditNotifier.Infrastructure/
COPY ./TaylorBot.Net.YoutubeNotifier.Domain/TaylorBot.Net.YoutubeNotifier.Domain.csproj ./TaylorBot.Net.YoutubeNotifier.Domain/
COPY ./TaylorBot.Net.YoutubeNotifier.Infrastructure/TaylorBot.Net.YoutubeNotifier.Infrastructure.csproj ./TaylorBot.Net.YoutubeNotifier.Infrastructure/
COPY ./TaylorBot.Net.TumblrNotifier.Domain/TaylorBot.Net.TumblrNotifier.Domain.csproj ./TaylorBot.Net.TumblrNotifier.Domain/
COPY ./TaylorBot.Net.TumblrNotifier.Infrastructure/TaylorBot.Net.TumblrNotifier.Infrastructure.csproj ./TaylorBot.Net.TumblrNotifier.Infrastructure/
COPY ./TaylorBot.Net.InstagramNotifier.Domain/TaylorBot.Net.InstagramNotifier.Domain.csproj ./TaylorBot.Net.InstagramNotifier.Domain/
COPY ./TaylorBot.Net.InstagramNotifier.Infrastructure/TaylorBot.Net.InstagramNotifier.Infrastructure.csproj ./TaylorBot.Net.InstagramNotifier.Infrastructure/

RUN dotnet restore ./TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj

# copy and publish app and libraries
COPY . ./src/
RUN dotnet publish -c Release -o out ./src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.PostNotifier.Program.dll"]
