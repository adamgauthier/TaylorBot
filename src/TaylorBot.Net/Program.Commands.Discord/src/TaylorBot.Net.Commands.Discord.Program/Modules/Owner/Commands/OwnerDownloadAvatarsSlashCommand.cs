using Azure.Storage.Blobs;
using Discord;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerDownloadAvatarsSlashCommand(ILogger<OwnerAddFeedbackUsersSlashCommand> logger, IOptionsMonitor<SignatureOptions> signatureOptions, ITaylorBotClient client, HttpClient httpClient)
    : ISlashCommand<OwnerDownloadAvatarsSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("owner downloadavatars");

    public record Options(ParsedString userids);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var guild = context.Guild!;
                var userIds = options.userids.Value.Split(',').Select(i => i.Trim()).ToList();

                List<IGuildUser> successful = [];
                List<string> unresolvedGuildMember = [];
                List<string> unexpectedError = [];

                logger.LogDebug("Processing {Count} user ids to download their avatars.", userIds.Count);

                foreach (var userId in userIds)
                {
                    logger.LogDebug("Processing user {UserId}.", userId);

                    try
                    {
                        await DownloadAvatarAsync(guild, userId, successful, unresolvedGuildMember);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Exception occurred for user {UserId}:", userId);
                        unexpectedError.Add(userId);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(250));
                }

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Downloaded **{successful.Count}** avatars. 👍
                        Couldn't resolve **{unresolvedGuildMember.Count}** members. ❓
                        Unexpected errors happened with **{unexpectedError.Count}** members. 🐛
                        """.Truncate(EmbedBuilder.MaxDescriptionLength))
                    .WithFooter($"Took {stopwatch.Elapsed.Humanize()}")
                .Build());
            },
            Preconditions:
            [
                new TaylorBotOwnerPrecondition(),
                new InGuildPrecondition(),
            ]
        ));
    }

    private async Task DownloadAvatarAsync(IGuild guild, string userId, List<IGuildUser> successful, List<string> unresolvedGuildMember)
    {
        var guildUser = await client.ResolveGuildUserAsync(guild, userId);
        if (guildUser != null)
        {
            var url = guildUser.GetGuildAvatarUrlOrDefault(size: 2048);
            BlobContainerClient container = new(signatureOptions.CurrentValue.BlobConnectionString, blobContainerName: "avatars");

            var fileExtension = Path.GetExtension(new Uri(url).AbsolutePath);
            var blob = container.GetBlobClient($"{guildUser.Id}-{guildUser.Username}{fileExtension}");

            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            await blob.UploadAsync(stream);

            successful.Add(guildUser);
        }
        else
        {
            logger.LogError("Can't resolve member {UserId}:", userId);
            unresolvedGuildMember.Add(userId);
            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }
    }
}
