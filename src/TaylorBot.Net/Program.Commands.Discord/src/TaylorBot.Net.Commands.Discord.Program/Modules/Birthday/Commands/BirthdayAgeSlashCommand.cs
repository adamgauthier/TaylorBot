using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdayAgeSlashCommand(IBirthdayRepository birthdayRepository, AgeCalculator ageCalculator, CommandMentioner mention) : ISlashCommand<BirthdayAgeSlashCommand.Options>
{
    public static string CommandName => "birthday age";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = options.user.User;
                var birthday = await birthdayRepository.GetBirthdayAsync(user);

                if (birthday != null && birthday.IsSet)
                {
                    if (birthday.Date.Year != UserBirthday.NoYearValue)
                    {
                        var age = AgeCalculator.GetCurrentAge(context.CreatedAt, birthday.Date);
                        ageCalculator.TryAddAgeRolesInBackground(context, user, age);

                        var embed = new EmbedBuilder()
                            .WithUserAsAuthor(user)
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithTitle("Age")
                            .WithDescription($"{user.Username} is **{age}** years old.");

                        return new EmbedResult(embed.Build());
                    }
                    else
                    {
                        return new EmbedResult(EmbedFactory.CreateError(
                            $"""
                            {user.Mention}'s birthday was set without a year so I can't tell how old they are. 🚫
                            Use {mention.SlashCommand("birthday set", context)} with the **year** option, your age will automatically update and you will get taypoints on your birthday every year! 🎈
                            Don't want to share your exact birthday, but want the points, horoscope and age commands? Use the **privately** option. 🕵️
                            """));
                    }
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        {user.Mention}'s birthday is not set so I can't tell how old they are. 🚫
                        They need to use {mention.SlashCommand("birthday set", context)} to set it first.
                        """));
                }
            }
        ));
    }
}
