FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.UserNotifier.sln ./
COPY ./src/TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./src/TaylorBot.Net.Core/
COPY ./src/TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./src/TaylorBot.Net.Core.Infrastructure/
COPY ./src/TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./src/TaylorBot.Net.Core.Program/
COPY ./src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj ./src/TaylorBot.Net.UserNotifier.Program/
COPY ./src/TaylorBot.Net.BirthdayReward.Domain/TaylorBot.Net.BirthdayReward.Domain.csproj ./src/TaylorBot.Net.BirthdayReward.Domain/
COPY ./src/TaylorBot.Net.BirthdayReward.Infrastructure/TaylorBot.Net.BirthdayReward.Infrastructure.csproj ./src/TaylorBot.Net.BirthdayReward.Infrastructure/
COPY ./src/TaylorBot.Net.Reminder.Domain/TaylorBot.Net.Reminder.Domain.csproj ./src/TaylorBot.Net.Reminder.Domain/
COPY ./src/TaylorBot.Net.Reminder.Infrastructure/TaylorBot.Net.Reminder.Infrastructure.csproj ./src/TaylorBot.Net.Reminder.Infrastructure/
COPY ./src/TaylorBot.Net.MemberLogging.Domain/TaylorBot.Net.MemberLogging.Domain.csproj ./src/TaylorBot.Net.MemberLogging.Domain/
COPY ./src/TaylorBot.Net.MemberLogging.Infrastructure/TaylorBot.Net.MemberLogging.Infrastructure.csproj ./src/TaylorBot.Net.MemberLogging.Infrastructure/

RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./src/TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj
ENTRYPOINT ["dotnet", "src/TaylorBot.Net.UserNotifier.Program/out/TaylorBot.Net.UserNotifier.Program.dll"]
