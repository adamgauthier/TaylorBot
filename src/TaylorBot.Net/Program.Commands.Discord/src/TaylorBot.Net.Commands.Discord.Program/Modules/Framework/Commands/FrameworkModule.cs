using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Framework.Commands;

[Name("Framework")]
public class FrameworkModule(
    ICommandRunner commandRunner,
    ICommandPrefixRepository commandPrefixRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : TaylorBotModule
{
    [Command("prefix")]
    [Alias("setprefix")]
    [Summary("Gets or changes the command prefix for this server.")]
    public async Task<RuntimeResult> PrefixAsync(
        [Remainder]
        [Summary("What would you like to set the prefix to?")]
        Word? prefix = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);

                if (prefix != null)
                {
                    await commandPrefixRepository.ChangeGuildPrefixAsync(Context.Guild, prefix.Value);
                    embed.WithDescription(
                        $"""
                        The command prefix for this server has been set to `{prefix.Value}`.
                        TaylorBot will now recognize commands starting with that prefix in this server, for example `{prefix.Value}help`
                        """);
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        The command prefix for this server is `{Context.CommandPrefix}`.
                        TaylorBot recognizes commands starting with that prefix in this server, for example `{Context.CommandPrefix}help`.
                        """);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [userHasPermission.Create(GuildPermission.ManageGuild)]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
