using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdaySetSlashCommand(IBirthdayRepository birthdayRepository, AgeCalculator ageCalculator) : ISlashCommand<BirthdaySetSlashCommand.Options>
{
    public static string CommandName => "birthday set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public record Options(ParsedPositiveInteger day, ParsedPositiveInteger month, ParsedOptionalInteger year, ParsedOptionalBoolean privately);

    private const int MinAge = 13;
    private const int MaxAge = 115;

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var isPrivate = options.privately.Value ?? false;

                DateOnly birthday;
                try
                {
                    birthday = new(options.year.Value ?? IBirthdayRepository.Birthday.NoYearValue, options.month.Value, options.day.Value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        The date you chose is not valid 🤔
                        Make sure you pick the right day/month, dates like February 31st are not real 🤭
                        """));
                }

                var setBirthday = await birthdayRepository.GetBirthdayAsync(context.User);

                if (setBirthday != null &&
                    (setBirthday.Date.Month != birthday.Month || setBirthday.Date.Day != birthday.Day))
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            You are changing your existing birthday to a different day 🤔

                            ⚠️ If you are doing this to get:
                            - More than 1 **taypoint birthday gift** per year 🎁
                            - **Birthday roles** for more than 1 day per year 🎂

                            This will **NOT** work! 🚫
                            TaylorBot only ever gives these **once a year**, regardless how many times you change your birthday ⛔
                            You will lose access to these on your real birthday 😢

                            Are you sure you want to change your birthday?
                            """)),
                        confirm: async () =>
                            new MessageContent(await SetBirthdayAsync(context, isPrivate, birthday))
                    );

                }
                else
                {
                    return new EmbedResult(await SetBirthdayAsync(context, isPrivate, birthday));
                }
            }
        ));
    }

    private async ValueTask<Embed> SetBirthdayAsync(RunContext context, bool isPrivate, DateOnly birthday)
    {
        if (birthday.Year != IBirthdayRepository.Birthday.NoYearValue)
        {
            int age = AgeCalculator.GetCurrentAge(context.CreatedAt, birthday);

            if (age < MinAge)
            {
                return EmbedFactory.CreateError($"Age must be higher or equal to {MinAge} years old.");
            }

            if (age > MaxAge)
            {
                return EmbedFactory.CreateError($"Age must be lower or equal to {MaxAge} years old.");
            }

            ageCalculator.TryAddAgeRolesInBackground(context, context.User, age);
        }

        await birthdayRepository.SetBirthdayAsync(context.User, new(birthday, isPrivate));

        var embed = new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithDescription(
                $"""
                Your birthday has been set **{birthday.ToString("MMMM d", TaylorBotCulture.Culture)}** ✅
                {(birthday.Year == IBirthdayRepository.Birthday.NoYearValue
                    ? $"Consider setting your birthday with the **year** option for {context.MentionSlashCommand("birthday age")} to work ❓"
                    : $"You can now use {context.MentionSlashCommand("birthday age")} to display your age 🔢")}
                {(isPrivate
                    ? $"Since your birthday is private, it won't show up in {context.MentionSlashCommand("birthday calendar")} 🙈"
                    : $"Your birthday will show up in {context.MentionSlashCommand("birthday calendar")} 📅")}
                You can now use {context.MentionSlashCommand("birthday horoscope")} to get your horoscope ✨
                You will get taypoints on your birthday every year 🎁
                """);

        return embed.Build();
    }
}
