using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Jail.Domain;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [RequireInGuild]
    [Name("Jail 👮")]
    [Group("jail")]
    public class JailModule : TaylorBotModule
    {
        private readonly IJailRepository _jailRepository;

        public JailModule(IJailRepository jailRepository)
        {
            _jailRepository = jailRepository;
        }

        [RequireUserPermissionOrOwner(GuildPermission.ManageGuild)]
        [Command("set")]
        [Summary("Sets the jail role for this server.")]
        public async Task<RuntimeResult> SetAsync(
            [Summary("What role would you like to set to the jail role?")]
            [Remainder]
            IRole role
        )
        {
            await _jailRepository.SetJailRoleAsync(Context.Guild, role);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription($"Role '{role.Name}' was successfully set as the jail role for this server.")
            .Build());
        }
    }
}
