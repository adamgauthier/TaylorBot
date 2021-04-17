using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Framework.Commands
{
    [Name("Framework")]
    public class FrameworkModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly ICommandPrefixRepository _commandPrefixRepository;

        public FrameworkModule(ICommandRunner commandRunner, ICommandPrefixRepository commandPrefixRepository)
        {
            _commandRunner = commandRunner;
            _commandPrefixRepository = commandPrefixRepository;
        }

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
                    var embed = new EmbedBuilder()
                        .WithUserAsAuthor(Context.User)
                        .WithColor(TaylorBotColors.SuccessColor);

                    if (prefix != null)
                    {
                        await _commandPrefixRepository.ChangeGuildPrefixAsync(Context.Guild, prefix.Value);
                        embed
                            .WithDescription(string.Join('\n', new[] {
                                $"The command prefix for this server has been set to `{prefix.Value}`.",
                                $"TaylorBot will now recognize commands starting with that prefix in this server, for example `{prefix.Value}help`."
                            }));
                    }
                    else
                    {
                        embed
                            .WithDescription(string.Join('\n', new[] {
                                $"The command prefix for this server is `{Context.CommandPrefix}`.",
                                $"TaylorBot recognizes commands starting with that prefix in this server, for example `{Context.CommandPrefix}help`."
                            }));
                    }

                    return new EmbedResult(embed.Build());
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
