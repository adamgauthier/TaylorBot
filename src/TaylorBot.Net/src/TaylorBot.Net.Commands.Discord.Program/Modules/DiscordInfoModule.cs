using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("DiscordInfo")]
    public class DiscordInfoModule : TaylorBotModule
    {
        public readonly UserStatusStringMapper _userStatusStringMapper;

        public DiscordInfoModule(UserStatusStringMapper userStatusStringMapper)
        {
            _userStatusStringMapper = userStatusStringMapper;
        }

        [Command("avatar")]
        [Alias("av", "avi")]
        [Summary("Displays the avatar of a user.")]
        public Task<RuntimeResult> AvatarAsync(
            [Summary("What user would you like to see the avatar of?")]
            IUser user = null
        )
        {
            if (user == null)
                user = Context.User;

            var embed = new EmbedBuilder()
                .WithUserAsAuthorAndColor(user)
                .WithImageUrl(user.GetAvatarUrlOrDefault(size: 2048))
            .Build();

            return Task.FromResult<RuntimeResult>(new TaylorBotEmbedResult(embed));
        }

        [Command("status")]
        [Summary("Displays the status of a user.")]
        public Task<RuntimeResult> StatusAsync(
            [Summary("What user would you like to see the status of?")]
            IUser user = null
        )
        {
            if (user == null)
                user = Context.User;

            var embed = new EmbedBuilder()
                .WithUserAsAuthorAndColor(user);

            if (user.Activity == null)
            {
                embed
                    .WithTitle("Status")
                    .WithDescription(_userStatusStringMapper.MapStatusToString(user.Status));
            }
            else
            {
                switch (user.Activity)
                {
                    case CustomStatusGame customStatus:
                        embed
                            .WithTitle("Custom Status")
                            .WithDescription($"{(customStatus.Emote != null ? $"{customStatus.Emote} " : string.Empty)}{customStatus.State}");
                        break;

                    case SpotifyGame spotifyGame:
                        embed
                            .WithTitle("Listening on Spotify")
                            .WithThumbnailUrl(spotifyGame.AlbumArtUrl)
                            .WithDescription(string.Join('\n', new[] {
                                $"[{spotifyGame.TrackTitle}]({spotifyGame.TrackUrl})",
                                $"by {string.Join(", ", spotifyGame.Artists.Select(a => $"**{a}**"))}",
                                $"on **{spotifyGame.AlbumTitle}**"
                            }));
                        break;

                    case Game game:
                        embed
                            .WithTitle("Playing")
                            .WithDescription(game.Name);
                        break;

                    default:
                        embed.WithDescription(user.Activity.Name);
                        break;
                }
            }

            return Task.FromResult<RuntimeResult>(new TaylorBotEmbedResult(embed.Build()));
        }
    }
}
