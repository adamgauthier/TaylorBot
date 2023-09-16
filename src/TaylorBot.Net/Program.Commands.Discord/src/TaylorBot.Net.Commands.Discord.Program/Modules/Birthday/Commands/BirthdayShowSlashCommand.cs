using Discord;
using Humanizer;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdayShowSlashCommand : ISlashCommand<BirthdayShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("birthday show");

    public record Options(ParsedUserOrAuthor user);

    private readonly IBirthdayRepository _birthdayRepository;
    private readonly TaskExceptionLogger _taskExceptionLogger;

    public BirthdayShowSlashCommand(IBirthdayRepository birthdayRepository, TaskExceptionLogger taskExceptionLogger)
    {
        _birthdayRepository = birthdayRepository;
        _taskExceptionLogger = taskExceptionLogger;
    }

    public Command Birthday(IUser user, DateTimeOffset createdAt, RunContext? context) => new(
        new(Info.Name),
        async () =>
        {
            var birthday = await _birthdayRepository.GetBirthdayAsync(user);

            if (birthday != null)
            {
                if (birthday.Date.Year != IBirthdayRepository.Birthday.NoYearValue)
                {
                    var age = AgeCalculator.GetCurrentAge(createdAt, birthday.Date);

                    _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                        AgeCalculator.TryAddAgeRolesAsync(_birthdayRepository, user, age),
                        nameof(AgeCalculator.TryAddAgeRolesAsync))
                    );
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

                    if (context == null)
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
                        To set your birthday privately, use {context?.MentionCommand("birthday set") ?? "</birthday set:1016938623880400907>"} with the **privately** option.
                        """));
                }
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s birthday is not set. 🚫
                    They need to use {context?.MentionCommand("birthday set") ?? "</birthday set:1016938623880400907>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Birthday(options.user.User, context.CreatedAt, context));
    }
}
