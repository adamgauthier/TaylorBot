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

public class BirthdayShowSlashCommand(IBirthdayRepository birthdayRepository, AgeCalculator ageCalculator, CommandMentioner mention) : ISlashCommand<BirthdayShowSlashCommand.Options>
{
    public static string CommandName => "birthday show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public Command Birthday(DiscordUser user, DateTimeOffset createdAt, RunContext context) => new(
        new(Info.Name, IsSlashCommand: context.SlashCommand != null),
        async () =>
        {
            var birthday = await birthdayRepository.GetBirthdayAsync(user);

            if (birthday != null && birthday.IsSet)
            {
                if (birthday.Date.Year != UserBirthday.NoYearValue)
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

                    if (context.SlashCommand == null)
                    {
                        embed.Description += $"\nPlease use {mention.SlashCommand("birthday show", context)} instead! 😊";
                    }

                    return new EmbedResult(embed.Build());
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        {user.Mention}'s birthday is private 🙅
                        To set your birthday privately, use {mention.SlashCommand("birthday set", context)} with the **privately** option.
                        """));
                }
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s birthday is not set 🚫
                    They need to use {mention.SlashCommand("birthday set", context)} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Birthday(options.user.User, context.CreatedAt, context));
    }
}
