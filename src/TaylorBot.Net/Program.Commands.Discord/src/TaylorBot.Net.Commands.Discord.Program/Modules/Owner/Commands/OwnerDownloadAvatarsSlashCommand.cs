using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Discord;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public partial class OwnerDownloadAvatarsSlashCommand(
    ILogger<OwnerDownloadAvatarsSlashCommand> logger,
    [FromKeyedServices("AvatarsContainer")]
    Lazy<BlobContainerClient> avatarsContainer,
    ITaylorBotClient client,
    IHttpClientFactory httpClientFactory,
    TaylorBotOwnerPrecondition ownerPrecondition,
    InGuildPrecondition.Factory inGuild)
    : ISlashCommand<OwnerDownloadAvatarsSlashCommand.Options>
{
    public static string CommandName => "owner downloadavatars";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString userids);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var userIds = options.userids.Value.Split(',').Select(i => i.Trim()).ToList();

                List<IGuildUser> successful = [];
                List<string> unresolvedGuildMember = [];
                List<string> unexpectedError = [];

                LogProcessingUserIds(userIds.Count);

                foreach (var userId in userIds)
                {
                    LogProcessingUser(userId);

                    try
                    {
                        await DownloadAvatarAsync(guild, userId, successful, unresolvedGuildMember);
                    }
                    catch (Exception exception)
                    {
                        LogExceptionForUser(exception, userId);
                        unexpectedError.Add(userId);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(250));
                }

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Downloaded **{successful.Count}** avatars 👍
                        Couldn't resolve **{unresolvedGuildMember.Count}** members ❓
                        Unexpected errors happened with **{unexpectedError.Count}** members 🐛
                        """.Truncate(EmbedBuilder.MaxDescriptionLength))
                    .WithFooter($"Took {stopwatch.Elapsed.Humanize()}")
                .Build());
            },
            Preconditions:
            [
                ownerPrecondition,
                inGuild.Create(),
            ]
        ));
    }

    private async Task DownloadAvatarAsync(IGuild guild, string userId, List<IGuildUser> successful, List<string> unresolvedGuildMember)
    {
        var guildUser = await client.ResolveGuildUserAsync(guild, userId);
        if (guildUser != null)
        {
            var url = new DiscordUser(guildUser).GetGuildAvatarUrlOrDefault(size: 2048);

            var fileExtension = Path.GetExtension(new Uri(url).AbsolutePath);
            var blob = avatarsContainer.Value.GetBlobClient($"{guildUser.Id}-{guildUser.Username}{fileExtension}");

            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            var contentType = response.Content.Headers.ContentType?.ToString();
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new() { ContentType = contentType } });
            }
            else
            {
                await blob.UploadAsync(stream);
            }

            successful.Add(guildUser);
        }
        else
        {
            LogCantResolveMember(userId);
            unresolvedGuildMember.Add(userId);
            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing {Count} user ids to download their avatars.")]
    private partial void LogProcessingUserIds(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing user {UserId}.")]
    private partial void LogProcessingUser(string userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred for user {UserId}:")]
    private partial void LogExceptionForUser(Exception exception, string userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Can't resolve member {UserId}:")]
    private partial void LogCantResolveMember(string userId);
}
