using Dapper;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public record GuildNameEntry(string GuildName, DateTimeOffset ChangedAt);

public interface IGuildNamesRepository
{
    ValueTask<List<GuildNameEntry>> GetHistoryAsync(IGuild guild, int limit);
}

public class GuildNamesPostgresRepository : IGuildNamesRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public GuildNamesPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    private record GuildNameDto(string guild_name, DateTime changed_at);

    public async ValueTask<List<GuildNameEntry>> GetHistoryAsync(IGuild guild, int limit)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var serverNames = await connection.QueryAsync<GuildNameDto>(
            """
            SELECT guild_name, changed_at
            FROM guilds.guild_names
            WHERE guild_id = @GuildId
            ORDER BY changed_at DESC
            LIMIT @MaxRows;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                MaxRows = limit,
            }
        );

        return serverNames.Select(n => new GuildNameEntry(n.guild_name, n.changed_at)).ToList();
    }
}

public class ServerNamesSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server names");

    private readonly IGuildNamesRepository _guildNamesRepository;

    public ServerNamesSlashCommand(IGuildNamesRepository guildNamesRepository)
    {
        _guildNamesRepository = guildNamesRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;

                var guildNames = await _guildNamesRepository.GetHistoryAsync(guild, 75);

                var guildNamesAsLines = guildNames.Select(n => $"{n.ChangedAt:MMMM dd, yyyy}: {n.GuildName}");

                var pages = guildNamesAsLines.Chunk(size: 15)
                    .Select(lines => string.Join('\n', lines))
                .ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithGuildAsAuthor(guild)
                    .WithTitle("Server Name History 🆔");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText: "No server name recorded. 😵")),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition()
            }
        ));
    }
}
