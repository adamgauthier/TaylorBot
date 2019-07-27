FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./TaylorBot.Net.Core/TaylorBot.Net.Core.csproj ./TaylorBot.Net.Core/
COPY ./TaylorBot.Net.Core.Infrastructure/TaylorBot.Net.Core.Infrastructure.csproj ./TaylorBot.Net.Core.Infrastructure/
COPY ./TaylorBot.Net.Core.Program/TaylorBot.Net.Core.Program.csproj ./TaylorBot.Net.Core.Program/
COPY ./TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj ./TaylorBot.Net.UserNotifier.Program/
COPY ./TaylorBot.Net.BirthdayReward.Domain/TaylorBot.Net.BirthdayReward.Domain.csproj ./TaylorBot.Net.BirthdayReward.Domain/
COPY ./TaylorBot.Net.BirthdayReward.Infrastructure/TaylorBot.Net.BirthdayReward.Infrastructure.csproj ./TaylorBot.Net.BirthdayReward.Infrastructure/
COPY ./TaylorBot.Net.Reminder.Domain/TaylorBot.Net.Reminder.Domain.csproj ./TaylorBot.Net.Reminder.Domain/
COPY ./TaylorBot.Net.Reminder.Infrastructure/TaylorBot.Net.Reminder.Infrastructure.csproj ./TaylorBot.Net.Reminder.Infrastructure/
COPY ./TaylorBot.Net.MemberLogging.Domain/TaylorBot.Net.MemberLogging.Domain.csproj ./TaylorBot.Net.MemberLogging.Domain/
COPY ./TaylorBot.Net.MemberLogging.Infrastructure/TaylorBot.Net.MemberLogging.Infrastructure.csproj ./TaylorBot.Net.MemberLogging.Infrastructure/

RUN dotnet restore ./TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out ./TaylorBot.Net.UserNotifier.Program/TaylorBot.Net.UserNotifier.Program.csproj
ENTRYPOINT ["dotnet", "TaylorBot.Net.UserNotifier.Program/out/TaylorBot.Net.UserNotifier.Program.dll"]
