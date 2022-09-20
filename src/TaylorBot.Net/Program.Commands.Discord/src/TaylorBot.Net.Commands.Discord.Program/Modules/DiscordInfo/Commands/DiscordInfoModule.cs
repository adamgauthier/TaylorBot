using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    [Name("DiscordInfo 💬")]
    public class DiscordInfoModule : TaylorBotModule
    {
        private static readonly Random _random = new();

        private readonly ICommandRunner _commandRunner;
        private readonly UserStatusStringMapper _userStatusStringMapper;
        private readonly ChannelTypeStringMapper _channelTypeStringMapper;
        private readonly IUserTracker _userTracker;

        public DiscordInfoModule(ICommandRunner commandRunner, UserStatusStringMapper userStatusStringMapper, ChannelTypeStringMapper channelTypeStringMapper, IUserTracker userTracker)
        {
            _commandRunner = commandRunner;
            _userStatusStringMapper = userStatusStringMapper;
            _channelTypeStringMapper = channelTypeStringMapper;
            _userTracker = userTracker;
        }

        [Command("avatar")]
        [Alias("av", "avi")]
        [Summary("Displays the avatar of a user.")]
        public async Task<RuntimeResult> AvatarAsync(
            [Summary("What user would you like to see the avatar of?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var u = user == null ?
                Context.User :
                await user.GetTrackedUserAsync();

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(
                new AvatarCommand().Avatar(u, AvatarType.Guild, "Use </avatar:832103922709692436> instead! 😊"),
                context
            );

            return new TaylorBotResult(result, context);
        }

        [Command("status")]
        [Summary("Displays the status of a user.")]
        public async Task<RuntimeResult> StatusAsync(
            [Summary("What user would you like to see the status of?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
            {
                var u = user == null ?
                    Context.User :
                    await user.GetTrackedUserAsync();

                var embed = new EmbedBuilder()
                    .WithUserAsAuthor(u)
                    .WithColor(GetColorFromStatus(u.Status));

                if (u.Activities.Count == 0)
                {
                    embed
                        .WithTitle("Status")
                        .WithDescription(_userStatusStringMapper.MapStatusToString(u.Status));
                }
                else
                {
                    var firstActivity = u.Activities.First();
                    switch (firstActivity)
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
                                    spotifyGame.TrackTitle.DiscordMdLink(spotifyGame.TrackUrl),
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
                            embed.WithDescription(firstActivity.Name);
                            break;
                    }
                }

                return new EmbedResult(embed.Build());
            });

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        private static Color GetColorFromStatus(UserStatus userStatus)
        {
            return userStatus switch
            {
                UserStatus.Offline or UserStatus.Invisible => new Color(116, 127, 141),
                UserStatus.Online => new Color(67, 181, 129),
                UserStatus.Idle or UserStatus.AFK => new Color(250, 166, 26),
                UserStatus.DoNotDisturb => new Color(240, 71, 71),
                _ => throw new ArgumentOutOfRangeException(nameof(userStatus)),
            };
        }

        [Command("userinfo")]
        [Alias("uinfo")]
        [Summary("Gets discord information about a user.")]
        public async Task<RuntimeResult> UserInfoAsync(
            [Summary("What user would you like to see the info of?")]
            [Remainder]
            IUserArgument<IGuildUser>? member = null
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var guildUser = member == null ?
                        (IGuildUser)Context.User :
                        await member.GetTrackedUserAsync();

                    var embed = new EmbedBuilder()
                        .WithUserAsAuthor(guildUser)
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithThumbnailUrl(guildUser.GetAvatarUrlOrDefault(size: 2048))
                        .AddField("Id", $"`{guildUser.Id}`", inline: true);

                    if (guildUser.JoinedAt.HasValue)
                        embed.AddField("Server Joined", guildUser.JoinedAt.Value.FormatFullUserDate(TaylorBotCulture.Culture));

                    embed.AddField("Account Created", guildUser.CreatedAt.FormatFullUserDate(TaylorBotCulture.Culture));

                    if (guildUser.RoleIds.Any())
                    {
                        embed.AddField(
                            "Role".ToQuantity(guildUser.RoleIds.Count),
                            string.Join(", ", guildUser.RoleIds.Take(4).Select(id => MentionUtils.MentionRole(id))) + (guildUser.RoleIds.Count > 4 ? ", ..." : string.Empty)
                        );
                    }

                    return new EmbedResult(embed.Build());
                },
                Preconditions: new[] { new InGuildPrecondition() }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("randomuserinfo")]
        [Alias("randomuser", "randomuinfo")]
        [Summary("Gets discord information about a random user in the server.")]
        public async Task<RuntimeResult> RandomUserInfoAsync()
        {
            var cachedUsers = await Context.Guild.GetUsersAsync(CacheMode.CacheOnly);
            var randomUser = cachedUsers.ElementAt(_random.Next(cachedUsers.Count));
            return await UserInfoAsync(new UserArgument<IGuildUser>(randomUser, _userTracker));
        }

        [Command("roleinfo")]
        [Alias("rinfo")]
        [Summary("Gets discord information about a role.")]
        public async Task<RuntimeResult> RoleInfoAsync(
            [Summary("What role would you like to see the info of?")]
            [Remainder]
            RoleArgument<IRole>? role = null
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    IRole GetRole()
                    {
                        if (role == null)
                        {
                            var guildUser = (IGuildUser)Context.User;
                            return guildUser.Guild.GetRole(guildUser.RoleIds.First());
                        }
                        else
                        {
                            return role.Role;
                        }
                    }

                    var r = GetRole();

                    var members = (await r.Guild.GetUsersAsync(CacheMode.CacheOnly)).Where(m => m.RoleIds.Contains(r.Id)).ToList();

                    var embed = new EmbedBuilder()
                        .WithColor(r.Color)
                        .WithAuthor(r.Name)
                        .AddField("Id", $"`{r.Id}`", inline: true)
                        .AddField("Color", $"`{r.Color}`", inline: true)
                        .AddField("Server Id", $"`{r.Guild.Id}`", inline: true)
                        .AddField("Hoisted", r.IsHoisted ? "✅" : "❌", inline: true)
                        .AddField("Managed", r.IsManaged ? "✅" : "❌", inline: true)
                        .AddField("Mentionable", r.IsMentionable ? "✅" : "❌", inline: true)
                        .AddField("Created", r.CreatedAt.FormatFullUserDate(TaylorBotCulture.Culture))
                        .AddField("Members",
                            $"**({members.Count}{(members.Any() ? $"+)** {string.Join(", ", members.Select(m => m.Nickname ?? m.Username)).Truncate(100)}" : ")**")}"
                        );

                    return new EmbedResult(embed.Build());
                },
                Preconditions: new[] { new InGuildPrecondition() }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("channelinfo")]
        [Alias("cinfo")]
        [Summary("Gets discord information about a channel.")]
        public async Task<RuntimeResult> ChannelInfoAsync(
            [Summary("What channel would you like to see the info of?")]
            [Remainder]
            IChannelArgument<IChannel>? channel = null
        )
        {
            var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
            {
                var c = channel == null ? Context.Channel : channel.Channel;

                var embed = new EmbedBuilder()
                    .WithAuthor(c is ITextChannel t && t.IsNsfw ? $"{c.Name} 🔞" : c.Name)
                    .AddField("Id", $"`{c.Id}`", inline: true)
                    .AddField("Type", _channelTypeStringMapper.MapChannelToTypeString(c), inline: true)
                    .AddField("Created", c.CreatedAt.FormatFullUserDate(TaylorBotCulture.Culture));

                if (channel is INestedChannel nested && nested.CategoryId.HasValue)
                {
                    var parent = await nested.Guild.GetChannelAsync(nested.CategoryId.Value);
                    embed.AddField("Category", $"{parent.Name} (`{parent.Id}`)", inline: true);
                }

                if (channel is IGuildChannel guildChannel)
                {
                    embed.AddField("Server", $"{guildChannel.Guild.Name} (`{guildChannel.GuildId}`)", inline: true);
                    switch (guildChannel)
                    {
                        case ITextChannel text:
                            embed.AddField("Topic", string.IsNullOrEmpty(text.Topic) ? "None" : text.Topic);
                            break;
                        case IVoiceChannel voice:
                            embed
                                .AddField("Bitrate", $"{voice.Bitrate} bps", inline: true)
                                .AddField("User Limit", voice.UserLimit.HasValue ? $"{voice.UserLimit.Value}" : "None", inline: true);
                            break;
                        case ICategoryChannel category:
                            var channels = await category.Guild.GetChannelsAsync();
                            var children = channels.OfType<INestedChannel>().Where(c => c.CategoryId.HasValue && c.CategoryId.Value == category.Id);
                            embed.AddField("Channels", string.Join(", ", children.Select(c => c.Name)).Truncate(75));
                            break;
                        default:
                            break;
                    }
                }

                return new EmbedResult(embed.Build());
            });

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("serverinfo")]
        [Alias("sinfo", "guildinfo", "ginfo")]
        [Summary("Gets discord information about the current server.")]
        public async Task<RuntimeResult> ServerInfoAsync()
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var guild = Context.Guild;
                    var channels = await guild.GetChannelsAsync();
                    var orderedRoles = guild.Roles.OrderByDescending(r => r.Position).ToList();

                    var embed = new EmbedBuilder()
                        .WithGuildAsAuthor(guild)
                        .WithColor(orderedRoles.First().Color)
                        .AddField("Id", $"`{guild.Id}`", inline: true)
                        .AddField("Owner", MentionUtils.MentionUser(guild.OwnerId), inline: true)
                        .AddField("Members", guild is SocketGuild socketGuild ? $"{socketGuild.MemberCount}" : "?", inline: true)
                        .AddField("Boosts", guild.PremiumSubscriptionCount, inline: true)
                        .AddField("Custom Emotes", guild.Emotes.Count, inline: true)
                        .AddField("Custom Stickers", guild.Stickers.Count, inline: true)
                        .AddField("Created", guild.CreatedAt.FormatFullUserDate(TaylorBotCulture.Culture));

                    if (!string.IsNullOrWhiteSpace(guild.Description))
                    {
                        embed.AddField("Description", guild.Description);
                    }

                    if (guild.BannerUrl != null)
                    {
                        embed.WithImageUrl(guild.BannerUrl);
                    }

                    embed
                        .AddField(
                            "Channel".ToQuantity(channels.Count),
                            $"{channels.OfType<ICategoryChannel>().Count()} Category, {channels.OfType<ITextChannel>().Count()} Text ({channels.OfType<IThreadChannel>().Count()} Thread), {channels.OfType<IVoiceChannel>().Count()} Voice ({channels.OfType<IStageChannel>().Count()} Stage)"
                        )
                        .AddField(
                            "Role".ToQuantity(orderedRoles.Count),
                            string.Join(", ", orderedRoles.Take(4).Select(r => r.Mention)) + (orderedRoles.Count > 4 ? ", ..." : string.Empty)
                        );

                    if (guild.IconUrl != null)
                    {
                        embed.WithThumbnailUrl(guild.IconUrl);
                    }

                    return new EmbedResult(embed.Build());
                },
                Preconditions: new[] { new InGuildPrecondition() }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
