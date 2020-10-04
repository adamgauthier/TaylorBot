using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Framework")]
    public class FrameworkModule : TaylorBotModule
    {
        private readonly ICommandPrefixRepository _commandPrefixRepository;

        public FrameworkModule(ICommandPrefixRepository commandPrefixRepository)
        {
            _commandPrefixRepository = commandPrefixRepository;
        }

        [RequireInGuild]
        [RequireUserPermissionOrOwner(GuildPermission.ManageGuild)]
        [Command("prefix")]
        [Alias("setprefix")]
        [Summary("Gets or changes the command prefix for this server.")]
        public async Task<RuntimeResult> PrefixAsync(
            [Remainder]
            [Summary("What would you like to set the prefix to?")]
            Word? prefix = null
        )
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

            return new TaylorBotEmbedResult(embed.Build());
        }
    }
}
