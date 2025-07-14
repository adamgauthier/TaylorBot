﻿using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmArtistsSlashCommand(
    LastFmEmbedFactory lastFmEmbedFactory,
    ILastFmUsernameRepository lastFmUsernameRepository,
    ILastFmClient lastFmClient,
    LastFmPeriodStringMapper lastFmPeriodStringMapper
) : ISlashCommand<LastFmArtistsSlashCommand.Options>
{
    public static string CommandName => "lastfm artists";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("lastfm artists", ["fm artists", "np artists"]);

    public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

    public Command Artists(LastFmPeriod? period, DiscordUser user, RunContext context) => new(
        context.SlashCommand == null ? Metadata with { IsSlashCommand = false } : Metadata,
        async () =>
        {
            period ??= LastFmPeriod.SevenDay;

            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user, context);

            var result = await lastFmClient.GetTopArtistsAsync(lastFmUsername.Username, period.Value);

            switch (result)
            {
                case TopArtistsResult success:
                    if (success.TopArtists.Count > 0)
                    {
                        var formattedArtists = success.TopArtists.Select((a, index) =>
                            $"{index + 1}. {a.Name.DiscordMdLink(a.ArtistUrl.ToString())}: {"play".ToQuantity(a.PlayCount, TaylorBotFormats.BoldReadable)}"
                        );

                        var embed = lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user)
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithTitle($"Top artists | {lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period.Value)}")
                            .WithDescription(formattedArtists.CreateEmbedDescriptionWithMaxAmountOfLines());

                        if (context.SlashCommand == null)
                        {
                            embed.WithFooter("⭐ Type /lastfm artists for an improved command experience!");
                        }

                        return new EmbedResult(embed.Build());
                    }
                    else
                    {
                        return lastFmEmbedFactory.CreateLastFmNoScrobbleErrorEmbedResult(lastFmUsername, user, period.Value);
                    }

                case LastFmGenericErrorResult error:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(error, context);

                default: throw new NotImplementedException();
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            Artists(
                options.period,
                options.user.User,
                context
            )
        );
    }
}
