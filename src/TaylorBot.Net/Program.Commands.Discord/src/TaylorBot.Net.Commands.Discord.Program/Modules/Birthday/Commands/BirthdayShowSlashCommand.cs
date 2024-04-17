using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdayShowSlashCommand(IBirthdayRepository birthdayRepository, AgeCalculator ageCalculator) : ISlashCommand<BirthdayShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("birthday show");

    public record Options(ParsedFetchedUserOrAuthor user);

    public Command Birthday(DiscordUser user, DateTimeOffset createdAt, RunContext context, bool isPrefix = false) => new(
        new(Info.Name),
        async () =>
        {
            var birthday = await birthdayRepository.GetBirthdayAsync(user);

            if (birthday != null)
            {
                if (birthday.Date.Year != IBirthdayRepository.Birthday.NoYearValue)
                {
                    var age = AgeCalculator.GetCurrentAge(createdAt, birthday.Date);
                    ageCalculator.TryAddAgeRolesInBackground(context, user, age);
                }

                if (!birthday.IsPrivate)
                {
                    var now = createdAt;
                    var nextBirthday = birthday.Date.AddYears(now.Year - birthday.Date.Year);
                    if (nextBirthday < DateOnly.FromDateTime(now.DateTime))
                    {
                        nextBirthday = nextBirthday.AddYears(1);
                    }

                    var embed = new EmbedBuilder()
                        .WithUserAsAuthor(user)
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithTitle("Birthday")
                        .WithDescription($"{birthday.Date.ToString("MMMM d", TaylorBotCulture.Culture)} ({nextBirthday.ToDateTime(TimeOnly.MinValue).Humanize(culture: TaylorBotCulture.Culture)})");

                    if (isPrefix)
                    {
                        embed.Description += "\nPlease use </birthday show:1016938623880400907> instead! 😊";
                    }

                    return new EmbedResult(embed.Build());
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        {user.Mention}'s birthday is private. 🙅
                        To set your birthday privately, use {(!isPrefix ? context.MentionCommand("birthday set") : "</birthday set:1016938623880400907>")} with the **privately** option.
                        """));
                }
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s birthday is not set. 🚫
                    They need to use {(!isPrefix ? context.MentionCommand("birthday set") : "</birthday set:1016938623880400907>")} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Birthday(new(options.user.User), context.CreatedAt, context));
    }
}
