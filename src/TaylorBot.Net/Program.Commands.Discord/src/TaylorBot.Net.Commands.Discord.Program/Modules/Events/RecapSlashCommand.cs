using Dapper;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events;

public interface IMemberActivityRepository
{
    Task<string> GetRecapCountAsync();
    Task<byte[]?> GetRecapImageForUserAsync(DiscordUser user);
}

public record MemberActivity(string user_id, string username, int messageCountThisYear, int minuteCountThisYear, string first_joined_at_formatted, float messagesPerDay, float minutesPerDay, int serverRank);

public class PostgresMemberActivityRepository(PostgresConnectionFactory postgresConnectionFactory, IMemoryCache memoryCache) : IMemberActivityRepository
{
    public async Task<string> GetRecapCountAsync()
    {
        var key = "recap_2024_count";
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
        var key = $"recap_2024_{user.Id}.jpg.base64";
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

public class RecapSlashCommand(IRateLimiter rateLimiter, IMemberActivityRepository memberActivityRepository) : ISlashCommand<NoOptions>
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
                var imageBytes = await memberActivityRepository.GetRecapImageForUserAsync(user);

                if (imageBytes == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                            Sorry, it looks like you were not part of the {await memberActivityRepository.GetRecapCountAsync()} most active members of 2024 😕
                            Maybe next year! 🙏
                            """));
                }

                const string filename = "recap.png";
                var imageStream = new MemoryStream(imageBytes);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Taycord Recap 2024 ✨")
                    .WithDescription(
                        """
                        Here's your 2024 recap **designed by Adam & FullyCustom** 🖌️
                        Submit your [Yearbook](https://discord.com/channels/115332333745340416/123150327456333824/1312535164714483793) signature with **/signature** if you haven't 😊
                        """)
                    .WithImageUrl($"attachment://{filename}");

                return new MessageResult(new([embed.Build()], Attachments: [new Attachment(imageStream, filename)]));
            },
            Preconditions: [new InGuildPrecondition()]
        ));
    }
}
