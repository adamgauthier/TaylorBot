using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("DiscordInfo")]
    public class DiscordInfoModule : TaylorBotModule
    {
        [Command("avatar")]
        [Alias("av", "avi")]
        [Summary("Displays the avatar of a user.")]
        public Task AvatarAsync(
            [Summary("What user would you like to see the avatar of?")]
            IUser user = null
        )
        {
            if (user == null)
                user = Context.User;

            return ReplyAsync(embed: new EmbedBuilder()
                .WithUserAsAuthorAndColor(user)
                .WithImageUrl(user.GetAvatarUrlOrDefault(size: 2048))
            .Build());
        }
    }
}
