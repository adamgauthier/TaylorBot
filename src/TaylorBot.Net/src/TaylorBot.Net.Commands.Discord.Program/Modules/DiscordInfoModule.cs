using Discord;
using Discord.Commands;
using Humanizer;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("DiscordInfo")]
    public class DiscordInfoModule : TaylorBotModule
    {
        public static readonly CultureInfo _culture = new CultureInfo("en-US");
        public static readonly Random _random = new Random();
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

        [RequireInGuild]
        [Command("userinfo")]
        [Alias("uinfo")]
        [Summary("Gets discord information about a user.")]
        public Task<RuntimeResult> UserInfoAsync(
            [Summary("What user would you like to see the info of?")]
            [OverrideTypeReader(typeof(CustomUserTypeReader<IGuildUser>))]
            IGuildUser member = null
        )
        {
            if (member == null)
                member = (IGuildUser)Context.User;

            var embed = new EmbedBuilder()
                .WithUserAsAuthorAndColor(member)
                .WithThumbnailUrl(member.GetAvatarUrlOrDefault(size: 2048))
                .AddField("Id", $"`{member.Id}`", inline: true);

            if (member.Activity != null && !string.IsNullOrWhiteSpace(member.Activity.Name))
                embed.AddField("Activity", member.Activity.Name, inline: true);

            if (member.JoinedAt.HasValue)
                embed.AddField("Server Joined", member.JoinedAt.Value.FormatFullUserDate(_culture));

            embed.AddField("Account Created", member.CreatedAt.FormatFullUserDate(_culture));

            if (member.RoleIds.Any())
            {
                embed.AddField(
                    "Role".ToQuantity(member.RoleIds.Count),
                    string.Join(", ", member.RoleIds.Select(id => $"<@&{id}>")).Truncate(75)
                );
            }

            return Task.FromResult<RuntimeResult>(new TaylorBotEmbedResult(embed.Build()));
        }

        [RequireInGuild]
        [Command("randomuserinfo")]
        [Alias("randomuser", "randomuinfo")]
        [Summary("Gets discord information about a random user in the server.")]
        public async Task<RuntimeResult> RandomUserInfoAsync()
        {
            var cachedUsers = await Context.Guild.GetUsersAsync(CacheMode.CacheOnly);
            var randomUser = cachedUsers.ElementAt(_random.Next(cachedUsers.Count));
            return await UserInfoAsync(randomUser);
        }
    }
}
