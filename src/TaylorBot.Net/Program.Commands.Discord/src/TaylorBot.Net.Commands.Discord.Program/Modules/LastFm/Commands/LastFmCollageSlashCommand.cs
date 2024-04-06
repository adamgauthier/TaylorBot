using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmCollageSlashCommand(ILastFmUsernameRepository lastFmUsernameRepository, LastFmEmbedFactory lastFmEmbedFactory, LastFmPeriodStringMapper lastFmPeriodStringMapper, HttpClient httpClient) : ISlashCommand<LastFmCollageSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("lastfm collage");
    public record Options(LastFmPeriod? period, ParsedOptionalInteger size, ParsedFetchedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var period = options.period ?? LastFmPeriod.SevenDay;
                var size = options.size.Value.HasValue ? new(options.size.Value.Value) : new LastFmCollageSize(3);
                DiscordUser user = new(options.user.User);

                var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

                if (lastFmUsername == null)
                    return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

                var queryString = new[] {
                    $"user={lastFmUsername.Username}",
                    $"type={lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period)}",
                    $"size={size.Parsed}x{size.Parsed}",
                };

                var response = await httpClient.GetAsync($"https://www.tapmusic.net/collage.php?{string.Join('&', queryString)}");
                response.EnsureSuccessStatusCode();

                var collage = await response.Content.ReadAsStreamAsync();
                const string filename = "collage.png";

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithAuthor(
                        name: lastFmUsername.Username,
                        iconUrl: user.GetGuildAvatarUrlOrDefault(),
                        url: lastFmUsername.LinkToProfile
                    )
                    .WithTitle($"Collage {size.Parsed}x{size.Parsed} | {lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period)}")
                    .WithImageUrl($"attachment://{filename}");

                return new MessageResult(new([embed.Build()], Attachments: [new Attachment(collage, filename)]));
            }
        ));
    }
}
