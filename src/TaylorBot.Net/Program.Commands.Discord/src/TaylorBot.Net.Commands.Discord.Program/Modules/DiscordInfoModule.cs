using Discord;
using Discord.Commands;
using Humanizer;
using System;
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
        public static readonly Random _random = new Random();
        public readonly UserStatusStringMapper _userStatusStringMapper;
        public readonly IUserTracker _userTracker;

        public DiscordInfoModule(UserStatusStringMapper userStatusStringMapper, IUserTracker userTracker)
        {
            _userStatusStringMapper = userStatusStringMapper;
            _userTracker = userTracker;
        }

        [Command("avatar")]
        [Alias("av", "avi")]
        [Summary("Displays the avatar of a user.")]
        public async Task<RuntimeResult> AvatarAsync(
            [Summary("What user would you like to see the avatar of?")]
            [Remainder]
            IUserArgument<IUser> user = null
        )
        {
            var u = user == null ?
                Context.User :
                await user.GetTrackedUserAsync();

            var embed = new EmbedBuilder()
                .WithUserAsAuthorAndColor(u)
                .WithImageUrl(u.GetAvatarUrlOrDefault(size: 2048))
            .Build();

            return new TaylorBotEmbedResult(embed);
        }

        [Command("status")]
        [Summary("Displays the status of a user.")]
        public async Task<RuntimeResult> StatusAsync(
            [Summary("What user would you like to see the status of?")]
            [Remainder]
            IUserArgument<IUser> user = null
        )
        {
            var u = user == null ?
                Context.User :
                await user.GetTrackedUserAsync();

            var embed = new EmbedBuilder().WithUserAsAuthorAndColor(u);

            if (u.Activity == null)
            {
                embed
                    .WithTitle("Status")
                    .WithDescription(_userStatusStringMapper.MapStatusToString(u.Status));
            }
            else
            {
                switch (u.Activity)
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
                        embed.WithDescription(u.Activity.Name);
                        break;
                }
            }

            return new TaylorBotEmbedResult(embed.Build());
        }

        [RequireInGuild]
        [Command("userinfo")]
        [Alias("uinfo")]
        [Summary("Gets discord information about a user.")]
        public async Task<RuntimeResult> UserInfoAsync(
            [Summary("What user would you like to see the info of?")]
            [OverrideTypeReader(typeof(CustomUserTypeReader<IGuildUser>))]
            [Remainder]
            IUserArgument<IGuildUser> member = null
        )
        {
            var guildUser = member == null ?
                (IGuildUser)Context.User :
                await member.GetTrackedUserAsync();

            var embed = new EmbedBuilder()
                .WithUserAsAuthorAndColor(guildUser)
                .WithThumbnailUrl(guildUser.GetAvatarUrlOrDefault(size: 2048))
                .AddField("Id", $"`{guildUser.Id}`", inline: true);

            if (guildUser.Activity != null && !string.IsNullOrWhiteSpace(guildUser.Activity.Name))
                embed.AddField("Activity", guildUser.Activity.Name, inline: true);

            if (guildUser.JoinedAt.HasValue)
                embed.AddField("Server Joined", guildUser.JoinedAt.Value.FormatFullUserDate(TaylorBotCulture.Culture));

            embed.AddField("Account Created", guildUser.CreatedAt.FormatFullUserDate(TaylorBotCulture.Culture));

            if (guildUser.RoleIds.Any())
            {
                embed.AddField(
                    "Role".ToQuantity(guildUser.RoleIds.Count),
                    string.Join(", ", guildUser.RoleIds.Select(id => MentionUtils.MentionRole(id))).Truncate(75)
                );
            }

            return new TaylorBotEmbedResult(embed.Build());
        }

        [RequireInGuild]
        [Command("randomuserinfo")]
        [Alias("randomuser", "randomuinfo")]
        [Summary("Gets discord information about a random user in the server.")]
        public async Task<RuntimeResult> RandomUserInfoAsync()
        {
            var cachedUsers = await Context.Guild.GetUsersAsync(CacheMode.CacheOnly);
            var randomUser = cachedUsers.ElementAt(_random.Next(cachedUsers.Count));
            return await UserInfoAsync(new UserArgument<IGuildUser>(randomUser, _userTracker));
        }
    }
}
