using Discord;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public record ServerJoined(DateTime? first_joined_at);

public record JoinedTimelineEntry(string user_id, string username, DateTime first_joined_at, long rank);

public interface IServerJoinedRepository
{
    Task<ServerJoined> GetRankedJoinedAsync(IGuildUser guildUser);
    Task FixMissingJoinedDateAsync(IGuildUser guildUser);
    Task<IList<JoinedTimelineEntry>> GetTimelineAsync(IGuild guild);
}

public class ServerJoinedSlashCommand : ISlashCommand<ServerJoinedSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server joined");

    public record Options(ParsedUserOrAuthor user);

    private readonly IServerJoinedRepository _serverJoinedRepository;

    public ServerJoinedSlashCommand(IServerJoinedRepository serverJoinedRepository)
    {
        _serverJoinedRepository = serverJoinedRepository;
    }

    private async Task<ServerJoined> GetServerJoinedAsync(IGuildUser guildUser)
    {
        var joined = await _serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        if (joined.first_joined_at == null && guildUser.JoinedAt is not null)
        {
            await _serverJoinedRepository.FixMissingJoinedDateAsync(guildUser);
            joined = await _serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        }
        return joined;
    }

    public Command Joined(IUser user, bool isLegacyCommand) => new(
        new("joined"),
        async () =>
        {
            var guildUser = (IGuildUser)user;
            ServerJoined joined = await GetServerJoinedAsync(guildUser);
            DateTimeOffset joinedAt = joined.first_joined_at ?? throw new InvalidOperationException();

            var sinceCreation = joinedAt - guildUser.Guild.CreatedAt;

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    {guildUser.Mention} first joined on {joinedAt.FormatDetailedWithRelative()} 🚪
                    This was roughly **{sinceCreation.Humanize(culture: TaylorBotCulture.Culture)}** after the server was created 📆

                    Check out </server timeline:1137547317549998130> for a history of who joined first! 📃
                    """);

            if (isLegacyCommand)
            {
                embed.WithUserAsAuthor(guildUser);
            }

            return new EmbedResult(embed.Build());
        },
        new ICommandPrecondition[] { new InGuildPrecondition() }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Joined(options.user.User, isLegacyCommand: false));
    }
}
