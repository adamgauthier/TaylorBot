using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dapper;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events;

public interface IMemberActivityRepository
{
    Task<string> GetRecapCountAsync();
    Task<byte[]?> GetRecapImageForUserAsync(DiscordUser user);
}

public class PostgresMemberActivityRepository(PostgresConnectionFactory postgresConnectionFactory, IMemoryCache memoryCache) : IMemberActivityRepository
{
    public async Task<string> GetRecapCountAsync()
    {
        var key = "recap_2025_count";
        var count = await memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                await using var connection = postgresConnectionFactory.CreateConnection();

                return await connection.QuerySingleAsync<string>(
                    $"""
                    SELECT info_value FROM configuration.application_info WHERE info_key = '{key}';
                    """);
            });
        ArgumentNullException.ThrowIfNull(count);
        return count;
    }

    public async Task<byte[]?> GetRecapImageForUserAsync(DiscordUser user)
    {
        var key = $"recap_2025_{user.Id}.jpg.base64";
        var imageBytes = await memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);

                await using var connection = postgresConnectionFactory.CreateConnection();

                var base64 = await connection.QuerySingleOrDefaultAsync<string>(
                    $"""
                    SELECT info_value FROM configuration.application_info WHERE info_key = '{key}';
                    """);

                return base64 != null ? Convert.FromBase64String(base64) : null;
            });
        return imageBytes;
    }
}

public class RecapSlashCommand(
    IRateLimiter rateLimiter,
    CommandMentioner mention,
    IMemoryCache memoryCache,
    IMemberActivityRepository memberActivityRepository,
    [FromKeyedServices("SignatureContainer")] Lazy<BlobContainerClient> signatureContainer,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "recap";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "generate-recap");
                if (rateLimitResult != null)
                    return rateLimitResult;

                var user = context.User;

                var cacheKey = $"recap_2025_signature_{user.Id}";
                var hasSignature = memoryCache.TryGetValue(cacheKey, out bool _);

                if (!hasSignature)
                {
                    await foreach (var blob in signatureContainer.Value.GetBlobsAsync(BlobTraits.None, BlobStates.None, $"{user.Id}-", default))
                    {
                        hasSignature = true;
                        break;
                    }

                    if (hasSignature)
                    {
                        memoryCache.Set(cacheKey, true, TimeSpan.FromHours(1));
                    }
                }

                if (!hasSignature)
                {
                    var guild = context.Guild ?? throw new InvalidOperationException();
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You must submit your Yearbook 2025 signature before viewing your recap 📝
                        Use {mention.GuildSlashCommand("signature", guild.Id)} to upload your signature, then try again ✨
                        """));
                }

                var imageBytes = await memberActivityRepository.GetRecapImageForUserAsync(user);

                if (imageBytes == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                            Sorry, it looks like you were not part of the {await memberActivityRepository.GetRecapCountAsync()} most active members of 2025 😕
                            Maybe next year! 🙏
                            """));
                }

                const string filename = "recap.png";
                MemoryStream imageStream = new(imageBytes);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Taycord Recap 2025 ✨")
                    .WithDescription(
                        """
                        Here's your 2025 recap **designed by FullyCustom & Adam** 🖌️
                        Submit your [Yearbook](https://discord.com/channels/115332333745340416/123150327456333824/1467198708465795290) survey if you haven't 😊
                        """)
                    .WithImageUrl($"attachment://{filename}")
                    .Build();

                return new MessageResult(new(new MessageContent([embed], Attachments: [new(imageStream, filename)])));
            },
            Preconditions: [inGuild.Create()]
        ));
    }
}
