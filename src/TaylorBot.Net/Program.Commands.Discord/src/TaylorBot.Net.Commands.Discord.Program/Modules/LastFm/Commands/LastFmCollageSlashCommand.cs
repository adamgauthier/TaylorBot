using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Http;
using TaylorBot.Net.Core.Infrastructure.Extensions;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmCollageSlashCommand(
    ILogger<LastFmCollageSlashCommand> logger,
    ILastFmUsernameRepository lastFmUsernameRepository,
    LastFmEmbedFactory lastFmEmbedFactory,
    LastFmPeriodStringMapper lastFmPeriodStringMapper,
    IHttpClientFactory clientFactory
    ) : ISlashCommand<LastFmCollageSlashCommand.Options>
{
    public static string CommandName => "lastfm collage";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);
    public record Options(LastFmPeriod? period, ParsedOptionalInteger size, ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var period = options.period ?? LastFmPeriod.SevenDay;
                var size = options.size.Value.HasValue ? new(options.size.Value.Value) : new LastFmCollageSize(3);
                var user = options.user.User;

                var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);
                if (lastFmUsername == null)
                {
                    return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);
                }

                var queryString = new Dictionary<string, string>
                {
                    { "user", lastFmUsername.Username },
                    { "type", lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period) },
                    { "size", $"{size.Parsed}x{size.Parsed}" },
                }.ToUrlQueryString();

                using var client = clientFactory.CreateClient();
                var response = await client.GetAsync($"https://www.tapmusic.net/collage.php?{queryString}");
                await response.EnsureSuccessAsync(logger);

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
