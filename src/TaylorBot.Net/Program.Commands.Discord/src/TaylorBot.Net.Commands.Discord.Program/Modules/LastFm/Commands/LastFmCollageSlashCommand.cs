using System.Net.Http.Json;
using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Http;
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

    private sealed record CollageRequest(string username, string period, string rowNum, string colNum, string type, string showName, string hideMissing);
    private sealed record CollageResponse(string downloadPath);

    private static string MapPeriodToCollageApi(LastFmPeriod period) => period switch
    {
        LastFmPeriod.SevenDay => "1week",
        LastFmPeriod.OneMonth => "1month",
        LastFmPeriod.ThreeMonth => "3month",
        LastFmPeriod.SixMonth => "6month",
        LastFmPeriod.TwelveMonth => "1year",
        LastFmPeriod.Overall => "forever",
        _ => throw new ArgumentOutOfRangeException(nameof(period)),
    };

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
                    return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user, context);
                }

                using var client = clientFactory.CreateClient();

                CollageRequest request = new(
                    username: lastFmUsername.Username,
                    period: MapPeriodToCollageApi(period),
                    rowNum: $"{size.Parsed}",
                    colNum: $"{size.Parsed}",
                    type: "albums",
                    showName: "true",
                    hideMissing: "false"
                );

                return await client.ReadJsonWithErrorLogging<CollageResponse, ICommandResult>(
                    async c => await c.PostAsJsonAsync("https://lastcollage.io/api/collage", request),
                    async success =>
                    {
                        var response = await client.GetAsync($"https://lastcollage.io/{success.Parsed.downloadPath}");
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
                            .WithImageUrl($"attachment://{filename}")
                            .Build();

                        return new MessageResult(new(new MessageContent([embed], Attachments: [new(collage, filename)])));
                    },
                    error => Task.FromResult<ICommandResult>(new EmbedResult(EmbedFactory.CreateError(
                        "The collage service is currently unavailable. Please try again later 😕"))),
                    logger
                );
            }
        ));
    }
}
