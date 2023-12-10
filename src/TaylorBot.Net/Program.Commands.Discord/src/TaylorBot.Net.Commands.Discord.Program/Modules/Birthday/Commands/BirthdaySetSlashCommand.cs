using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdaySetSlashCommand(IBirthdayRepository birthdayRepository, TaskExceptionLogger taskExceptionLogger) : ISlashCommand<BirthdaySetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("birthday set", IsPrivateResponse: true);

    public record Options(ParsedPositiveInteger day, ParsedPositiveInteger month, ParsedOptionalInteger year, ParsedOptionalBoolean privately);

    private const int MinAge = 13;
    private const int MaxAge = 115;

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                DateOnly birthday = new(options.year.Value ?? IBirthdayRepository.Birthday.NoYearValue, options.month.Value, options.day.Value);

                if (birthday.Year != IBirthdayRepository.Birthday.NoYearValue)
                {
                    int age = AgeCalculator.GetCurrentAge(context.CreatedAt, birthday);

                    if (age < MinAge)
                    {
                        return new EmbedResult(EmbedFactory.CreateError($"Age must be higher or equal to {MinAge} years old."));
                    }

                    if (age > MaxAge)
                    {
                        return new EmbedResult(EmbedFactory.CreateError($"Age must be lower or equal to {MaxAge} years old."));
                    }

                    _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                        AgeCalculator.TryAddAgeRolesAsync(birthdayRepository, context.User, age),
                        nameof(AgeCalculator.TryAddAgeRolesAsync))
                    );
                }

                var isPrivate = options.privately.Value ?? false;

                await birthdayRepository.SetBirthdayAsync(context.User, new(birthday, isPrivate));

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"Your birthday has been set **{birthday.ToString("MMMM d", TaylorBotCulture.Culture)}**. ✅",
                        birthday.Year == IBirthdayRepository.Birthday.NoYearValue ?
                            $"Please consider setting your birthday with the **year** option for {context.MentionCommand("birthday age")} to work. ❓" :
                            $"You can now use {context.MentionCommand("birthday age")} to display your age. 🔢",
                        isPrivate ?
                            $"Since your birthday is private, it won't show up in {context.MentionCommand("birthday calendar")}. 🙈" :
                            $"Your birthday will show up in {context.MentionCommand("birthday calendar")}. 📅",
                        $"You can now use {context.MentionCommand("birthday horoscope")} to get your horoscope. ✨",
                        "You will get taypoints on your birthday every year. 🎁",
                    }));

                return new EmbedResult(embed.Build());
            }
        ));
    }
}
