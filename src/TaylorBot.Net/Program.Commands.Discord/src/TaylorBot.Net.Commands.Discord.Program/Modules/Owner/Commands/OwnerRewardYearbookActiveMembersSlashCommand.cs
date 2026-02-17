using Azure.Storage.Blobs;
using Dapper;
using Discord;
using Discord.Net;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Diagnostics;
using System.Text.Json;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public record ActiveMembers(IList<ActiveMembers.Member> members)
{
    public class Member
    {
        public string userId { get; set; } = null!;
        public ProcessedInfo processedInfo { get; set; } = new();
        public bool isMod { get; set; }
    }

    public class ProcessedInfo
    {
        public bool rewarded { get; set; }
        public bool unresolvedMember { get; set; }
        public bool cantMessage { get; set; }
        public bool messaged { get; set; }
        public bool completed { get; set; }
    }
}

public partial class OwnerRewardYearbookActiveMembersSlashCommand(
    ILogger<OwnerRewardYearbookActiveMembersSlashCommand> logger,
    ITaylorBotClient client,
    PostgresConnectionFactory postgresConnectionFactory,
    TaylorBotOwnerPrecondition ownerPrecondition,
    InGuildPrecondition.Factory inGuild,
    [FromKeyedServices("SignatureContainer")] Lazy<BlobContainerClient> signatureContainer)
    : ISlashCommand<OwnerRewardYearbookActiveMembersSlashCommand.Options>
{
    public static string CommandName => "owner rewardyearbook";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedPositiveInteger count);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                ArgumentNullException.ThrowIfNull(context.Guild);
                var guild = context.Guild.Fetched ?? throw new InvalidOperationException();

                await using var connection = postgresConnectionFactory.CreateConnection();
                var activeMembers = JsonSerializer.Deserialize<ActiveMembers>(await connection.QuerySingleAsync<string>(
                    """
                    SELECT info_value FROM configuration.application_info WHERE info_key = 'rewardyearbook2025';
                    """
                )) ?? throw new NotImplementedException();

                var membersToProcess = activeMembers.members.Where(m => m.processedInfo.completed != true).ToList();
                LogProcessingMembers(membersToProcess.Count);

                List<IGuildUser> successful = [];
                List<string> unresolvedGuildMembers = [];
                List<string> cantMessageGuildMembers = [];
                List<string> unexpectedError = [];

                foreach (var member in membersToProcess.Take(options.count.Value))
                {
                    LogProcessingUser(member.userId);

                    try
                    {
                        await ProcessMemberAsync(connection, guild, member, successful, cantMessageGuildMembers, unresolvedGuildMembers);
                        member.processedInfo.completed = true;

                        await connection.ExecuteAsync(
                            "UPDATE configuration.application_info SET info_value = @InfoValue WHERE info_key = 'rewardyearbook2025';",
                            new
                            {
                                InfoValue = JsonSerializer.Serialize(activeMembers),
                            });
                    }
                    catch (Exception exception)
                    {
                        LogExceptionProcessingUser(exception, member.userId);
                        unexpectedError.Add(member.userId);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Messaged **{successful.Count}** members 👍
                        Couldn't resolve **{unresolvedGuildMembers.Count}** members ❓
                        Couldn't message **{cantMessageGuildMembers.Count}** members 🚫
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

    private async Task ProcessMemberAsync(NpgsqlConnection connection, IGuild guild, ActiveMembers.Member member, List<IGuildUser> successful, List<string> cantMessageGuildMembers, List<string> unresolvedGuildMembers)
    {
        var taypointReward = member.isMod ? 25_000 : 10_000;

        if (!member.processedInfo.rewarded)
        {
            await TaypointPostgresUtil.AddTaypointsAsync(connection, member.userId, taypointReward);
            member.processedInfo.rewarded = true;
        }

        var guildUser = await client.ResolveGuildUserAsync(guild, member.userId);
        if (guildUser != null)
        {
            var hasSubmittedSignature = await HasSubmittedSignatureAsync($"{guildUser.Id}", guildUser.Username);

            var description = member.isMod
                ? $"""
                ## Thank You 💖
                Thank you for your dedication in 2025 as a Taylor Swift Discord moderator 🛡️
                Your commitment to keep our community safe is essential to our continued growth 🥺
                I know it's not always easy, especially when dealing with challenging situations and negative feedback from members 🙏
                As a small token of appreciation, I've gifted you {"taypoint".ToQuantity(taypointReward, TaylorBotFormats.BoldReadable)}! 🎁
                """
                : $"""
                ## Congratulations 🎉
                You were in the Taylor Swift Discord's **top 100 most active members** in 2025 🏆
                Thank you for being a part of our community and contributing to it 💝
                I just gave you {"taypoint".ToQuantity(taypointReward, TaylorBotFormats.BoldReadable)} as a gift! 🎁
                """;

            if (!hasSubmittedSignature)
            {
                description +=
                    $"""

                    ## ⚠️⚠️🖊️ Yearbook Signature MISSING 🖊️⚠️⚠️
                    You **still haven't submitted your signature for Yearbook 2025**! 🚨
                    You are running out of time, we are making the yearbook! ⏳
                    Please submit it **in the next day** using the **/signature** command in #bots!
                    Click here for more details: https://discord.com/channels/115332333745340416/123150327456333824/1467198708465795290 ✨
                    """;
            }

            try
            {
                await guildUser.SendMessageAsync(embed: new EmbedBuilder()
                    .WithColor(TaylorBotColors.GoldColor)
                    .WithDescription(description)
                    .Build());
                member.processedInfo.messaged = true;

                LogSentMessage(((IUser)guildUser).FormatLog());
            }
            catch (HttpException httpException)
            {
                if (httpException.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    member.processedInfo.cantMessage = true;
                    LogCantDmMember(member.userId);
                    cantMessageGuildMembers.Add(member.userId);
                    return;
                }
                else
                {
                    throw;
                }
            }

            successful.Add(guildUser);
        }
        else
        {
            member.processedInfo.unresolvedMember = true;
            LogCantResolveMember(member.userId);
            unresolvedGuildMembers.Add(member.userId);
        }
    }

    private async Task<bool> HasSubmittedSignatureAsync(string userId, string username)
    {
        var blobs = signatureContainer.Value.GetBlobsAsync(
            Azure.Storage.Blobs.Models.BlobTraits.None,
            Azure.Storage.Blobs.Models.BlobStates.None,
            $"{userId}-",
            CancellationToken.None);
        await foreach (var blob in blobs)
        {
            return true;
        }
        return false;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing {Count} members.")]
    private partial void LogProcessingMembers(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing user {UserId}.")]
    private partial void LogProcessingUser(string userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred for user {UserId}:")]
    private partial void LogExceptionProcessingUser(Exception exception, string userId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Sent a message to {User}.")]
    private partial void LogSentMessage(string user);

    [LoggerMessage(Level = LogLevel.Error, Message = "Can't DM member {UserId}")]
    private partial void LogCantDmMember(string userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Can't resolve member {UserId}")]
    private partial void LogCantResolveMember(string userId);
}
