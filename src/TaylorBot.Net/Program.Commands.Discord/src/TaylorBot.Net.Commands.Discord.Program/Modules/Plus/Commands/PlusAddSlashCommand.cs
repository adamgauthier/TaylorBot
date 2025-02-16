using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;

public class PlusAddSlashCommand(IPlusRepository plusRepository, IPlusUserRepository plusUserRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "plus add";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);

                var plusUser = await plusUserRepository.GetPlusUserAsync(context.User);
                ArgumentNullException.ThrowIfNull(plusUser);

                var embed = new EmbedBuilder();

                if (plusUser.ActivePlusGuilds.Count + 1 > plusUser.MaxPlusGuilds)
                {
                    embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(
                            $"""
                            Unfortunately you can't add more **TaylorBot Plus** servers with your current membership 😕
                            Use {context.MentionSlashCommand("plus show")} to see your plus servers and maybe remove some with {context.MentionSlashCommand("plus remove")}
                            """);
                }
                else
                {
                    await plusUserRepository.AddPlusGuildAsync(context.User, context.Guild);

                    embed
                        .WithColor(TaylorBotColors.DiamondBlueColor)
                        .WithDescription(
                            $"""
                            Successfully added {context.Guild.Fetched?.Name ?? "this server"} to your plus servers 😊
                            It now has access to **TaylorBot Plus** features 💎
                            """);

                    if (context.Guild.Fetched != null)
                    {
                        embed.WithThumbnailUrl(context.Guild.Fetched.IconUrl);
                    }
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [
                new InGuildPrecondition(),
                new PlusPrecondition(plusRepository, PlusRequirement.PlusUser),
            ]
        ));
    }
}
