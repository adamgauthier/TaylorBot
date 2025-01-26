using Dapper;
using Discord;
using Discord.Net;
using Humanizer;
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

public record ActiveMembers(IList<ActiveMembers.Member> members, IList<string> usersIdWhoSubmittedSignatures)
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

public class OwnerRewardYearbookActiveMembersSlashCommand(ILogger<OwnerRewardYearbookActiveMembersSlashCommand> logger, ITaylorBotClient client, PostgresConnectionFactory postgresConnectionFactory)
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
                    SELECT info_value FROM configuration.application_info WHERE info_key = 'rewardyearbook2024';
                    """
                )) ?? throw new NotImplementedException();

                var membersToProcess = activeMembers.members.Where(m => m.processedInfo.completed != true).ToList();
                logger.LogDebug("Processing {Count} members.", membersToProcess.Count);

                List<IGuildUser> successful = [];
                List<string> unresolvedGuildMembers = [];
                List<string> cantMessageGuildMembers = [];
                List<string> unexpectedError = [];

                foreach (var member in membersToProcess.Take(options.count.Value))
                {
                    logger.LogDebug("Processing user {UserId}.", member.userId);

                    try
                    {
                        await ProcessMemberAsync(connection, guild, member, successful, cantMessageGuildMembers, unresolvedGuildMembers, activeMembers.usersIdWhoSubmittedSignatures);
                        member.processedInfo.completed = true;

                        await connection.ExecuteAsync(
                            "UPDATE configuration.application_info SET info_value = @InfoValue WHERE info_key = 'rewardyearbook2024';",
                            new
                            {
                                InfoValue = JsonSerializer.Serialize(activeMembers),
                            });
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Exception occurred for user {UserId}:", member.userId);
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
                new TaylorBotOwnerPrecondition(),
                new InGuildPrecondition(),
            ]
        ));
    }

    private async Task ProcessMemberAsync(NpgsqlConnection connection, IGuild guild, ActiveMembers.Member member, List<IGuildUser> successful, List<string> cantMessageGuildMembers, List<string> unresolvedGuildMembers, IList<string> usersIdWhoSubmittedSignatures)
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
            var description = member.isMod
                ? $"""
                ## Thank You 💖
                Thank you for your dedication this year as a Taylor Swift Discord moderator 🛡️
                Your commitment to keep our community safe is essential to our continued growth 🥺
                I know it's not always easy, especially when dealing with challenging situations and negative feedback from members 🙏
                As a small token of appreciation, I've gifted you {"taypoint".ToQuantity(taypointReward, TaylorBotFormats.BoldReadable)}! 🎁
                """
                : $"""
                ## Congratulations 🎉
                You were in the Taylor Swift Discord's **top 100 most active members** this year 🏆
                Thank you for being a part of our community and contributing to it 💝
                I just gave you {"taypoint".ToQuantity(taypointReward, TaylorBotFormats.BoldReadable)} as a gift! 🎁
                """;

            if (!usersIdWhoSubmittedSignatures.Contains($"{guildUser.Id}"))
            {
                description +=
                    $"""

                    ## Yearbook Signature 🖊️
                    It seems like you haven't submitted **your signature for Yearbook 2024** yet ⚠️
                    Please take a minute to submit it using the **/signature** command in #bots. 😊
                    Click here for more details: https://discord.com/channels/115332333745340416/123150327456333824/1312535164714483793 ✨
                    """;
            }

            try
            {
                await guildUser.SendMessageAsync(embed: new EmbedBuilder()
                    .WithColor(TaylorBotColors.GoldColor)
                    .WithDescription(description)
                    .Build());
                member.processedInfo.messaged = true;

                logger.LogDebug("Sent a message to {User}.", ((IUser)guildUser).FormatLog());
            }
            catch (HttpException httpException)
            {
                if (httpException.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    member.processedInfo.cantMessage = true;
                    logger.LogError("Can't DM member {UserId}", member.userId);
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
            logger.LogError("Can't resolve member {UserId}", member.userId);
            unresolvedGuildMembers.Add(member.userId);
        }
    }
}
