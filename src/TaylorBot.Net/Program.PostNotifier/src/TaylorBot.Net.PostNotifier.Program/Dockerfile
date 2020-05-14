FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Core/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Core/src/${file%.*}/ && mv $file Core/src/${file%.*}/; done

COPY ./Program.PostNotifier/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p Program.PostNotifier/src/${file%.*}/ && mv $file Program.PostNotifier/src/${file%.*}/; done

RUN dotnet restore ./Program.PostNotifier/src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj

# copy and publish app and libraries
COPY . .

RUN dotnet publish -c Release -o out --no-restore ./Program.PostNotifier/src/TaylorBot.Net.PostNotifier.Program/TaylorBot.Net.PostNotifier.Program.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "TaylorBot.Net.PostNotifier.Program.dll"]
