using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;

public class UsernamesVisibilitySlashCommand(IUsernameHistoryRepository usernameHistoryRepository) : ISlashCommand<UsernamesVisibilitySlashCommand.Options>
{
    public static string CommandName => "usernames visibility";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString setting);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                switch (options.setting.Value)
                {
                    case "public":
                        await usernameHistoryRepository.UnhideUsernameHistoryFor(context.User);

                        return new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(
                                $"""
                                Your username history is now **public** (__can__ be viewed with {context.MentionCommand("usernames show")}) ✅
                                Use {context.MentionCommand("usernames visibility")} again to make it private 🕵️
                                """)
                            .Build());

                    case "private":
                        await usernameHistoryRepository.HideUsernameHistoryFor(context.User);

                        return new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(
                                $"""
                                Your username history is now **private** (__can't__ be viewed with {context.MentionCommand("usernames show")}) ✅
                                Use {context.MentionCommand("usernames visibility")} again to make it public 📢
                                """)
                            .Build());

                    default: throw new NotImplementedException();
                }
            }
        ));
    }
}
