using Discord;
using Discord.WebSocket;
using Humanizer;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class InspectServerSlashCommand : ISlashCommand<NoOptions>
{
    public static string CommandName => "inspect server";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

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
                    .AddField("Created", guild.CreatedAt.FormatDetailedWithRelative());

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
            Preconditions: [new InGuildPrecondition(botMustBeInGuild: true)]
        ));
    }
}
