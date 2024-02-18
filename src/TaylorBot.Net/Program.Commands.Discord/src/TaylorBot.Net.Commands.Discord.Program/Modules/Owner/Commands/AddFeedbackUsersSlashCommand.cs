using Dapper;
using Discord;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerAddFeedbackUsersSlashCommand(ILogger<OwnerAddFeedbackUsersSlashCommand> logger, ITaylorBotClient client, PostgresConnectionFactory postgresConnectionFactory)
    : ISlashCommand<OwnerAddFeedbackUsersSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("owner addfeedbackusers");

    public record Options(ParsedOptionalBoolean whatif);

    private const long FeedbackRoleId = 482738450722848809;
    private static readonly int CommandTimeout = (int)TimeSpan.FromMinutes(2).TotalSeconds;

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var whatIf = options.whatif.Value ?? false;
                var stopwatch = Stopwatch.StartNew();
                var guild = context.Guild?.Id == 115332333745340416 ? context.Guild : throw new InvalidOperationException("Unexpected guild.");

                await using var connection = postgresConnectionFactory.CreateConnection();

                var members = (await connection.QueryAsync<MemberDto>(
                    """
                    SELECT user_id
                    FROM guilds.guild_members
                    WHERE guild_id = '115332333745340416'
                    AND message_count > 1300
                    AND minute_count > 1300
                    AND first_joined_at <= (CURRENT_TIMESTAMP - INTERVAL '30 day')
                    AND last_spoke_at >= (CURRENT_TIMESTAMP - INTERVAL '365 day')
                    """,
                    commandTimeout: CommandTimeout
                )).ToList();

                logger.LogDebug("Processing {Count} eligible users to add them to feedback.", members.Count);

                List<IGuildUser> roleAdded = [];
                List<MemberDto> unresolvedGuildMember = [];
                List<IGuildUser> alreadyHaveRole = [];
                List<MemberDto> unexpectedError = [];

                foreach (var member in members)
                {
                    logger.LogDebug("Processing {Member} to add them to feedback.", member);

                    try
                    {
                        await AddMemberToFeedback(context, whatIf, guild, member, roleAdded, unresolvedGuildMember, alreadyHaveRole);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Exception occurred when attempting to add {Member} to feedback:", member);
                        unexpectedError.Add(member);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(5));
                }

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        {(whatIf ? "[SIMULATION] " : "")}Added **{roleAdded.Count}** members to feedback. 👍
                        Considered a total of **{members.Count}** members who met requirements. ✅
                        **{alreadyHaveRole.Count}** members already had the role. 🧓
                        Unexpected errors happened with **{unexpectedError.Count}** members. 🐛
                        {string.Join(", ", roleAdded.Select(r => r.FormatTagAndMention()))}
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

    private async Task AddMemberToFeedback(RunContext context, bool whatIf, IGuild guild, MemberDto member, List<IGuildUser> roleAdded, List<MemberDto> unresolvedGuildMember, List<IGuildUser> alreadyHaveRole)
    {
        var guildUser = await client.ResolveGuildUserAsync(guild, new(member.user_id));
        if (guildUser != null)
        {
            if (!guildUser.RoleIds.Any(i => i == FeedbackRoleId))
            {
                if (!whatIf)
                {
                    await guildUser.AddRoleAsync(FeedbackRoleId, new()
                    {
                        AuditLogReason = $"Feedback command triggered by {context.User.Username} ({context.User.Id})"
                    });

                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                    // It may be considered spam to send the same DM to a lot of users at once...
                    if (ShouldSendDM())
                    {
                        try
                        {
                            await guildUser.SendMessageAsync(embed: EmbedFactory.CreateSuccess(
                                """
                                You now meet the requirements for r/TaylorSwift Discord #feedback, a channel meant to get feedback from our community ⭐
                                Make sure to read the guidelines on how to use the channel here: https://discord.com/channels/115332333745340416/1087004541322534993/1087045463959674991 😊
                                """
                            ));
                            await Task.Delay(TimeSpan.FromMilliseconds(50));
                        }
                        catch (Exception exception)
                        {
                            logger.LogWarning(exception, "Exception occurred when attempting to DM {Member} about adding them to feedback:", guildUser.FormatLog());
                        }
                    }
                }

                roleAdded.Add(guildUser);
            }
            else
            {
                alreadyHaveRole.Add(guildUser);
            }
        }
        else
        {
            unresolvedGuildMember.Add(member);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }

    private static bool ShouldSendDM()
    {
        return false;
    }

    private record MemberDto
    {
        public string user_id { get; set; } = null!;
    }
}
