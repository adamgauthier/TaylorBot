﻿using Discord;
using System.Net.Http;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmCollageSlashCommand : ISlashCommand<LastFmCollageSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("lastfm collage");
        public record Options(LastFmPeriod? period, ParsedOptionalInteger size, ParsedUserOrAuthor user);

        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;
        private readonly LastFmEmbedFactory _lastFmEmbedFactory;
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper;
        private readonly HttpClient _httpClient;

        public LastFmCollageSlashCommand(ILastFmUsernameRepository lastFmUsernameRepository, LastFmEmbedFactory lastFmEmbedFactory, LastFmPeriodStringMapper lastFmPeriodStringMapper, HttpClient httpClient)
        {
            _lastFmUsernameRepository = lastFmUsernameRepository;
            _lastFmEmbedFactory = lastFmEmbedFactory;
            _lastFmPeriodStringMapper = lastFmPeriodStringMapper;
            _httpClient = httpClient;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var period = options.period ?? LastFmPeriod.SevenDay;
                    var size = options.size.Value.HasValue ? new(options.size.Value.Value) : new LastFmCollageSize(3);
                    var user = options.user.User;

                    var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(user);

                    if (lastFmUsername == null)
                        return _lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

                    var queryString = new[] {
                        $"user={lastFmUsername.Username}",
                        $"type={_lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period)}",
                        $"size={size.Parsed}x{size.Parsed}",
                    };

                    var response = await _httpClient.GetAsync($"https://www.tapmusic.net/collage.php?{string.Join('&', queryString)}");
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
                        .WithTitle($"Collage {size.Parsed}x{size.Parsed} | {_lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period)}")
                        .WithImageUrl($"attachment://{filename}");

                    return new MessageResult(new(new[] { embed.Build() }, Attachments: new[] { new Attachment(collage, filename) }));
                }
            ));
        }
    }
}
