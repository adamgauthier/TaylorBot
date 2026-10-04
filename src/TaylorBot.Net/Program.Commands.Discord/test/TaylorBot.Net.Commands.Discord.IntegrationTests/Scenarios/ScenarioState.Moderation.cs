using Dapper;
using System.Text.Json;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioState
{
    public async Task<string?> LogChannelAsync(ScenarioGuild guild, string kind)
    {
        var (table, column) = ModerationTables.Log(kind);
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<string>($"SELECT {column} FROM {table} WHERE guild_id = @Id;", new { guild.Id });
    }

    public async Task<bool> ServerCommandDisabledAsync(ScenarioGuild guild, string command)
    {
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleAsync<bool>(
            "SELECT EXISTS(SELECT FROM guilds.guild_commands WHERE guild_id = @Id AND command_name = @command AND disabled);", new { guild.Id, command });
    }

    public async Task<bool> ChannelCommandDisabledAsync(ScenarioGuild guild, string command)
    {
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleAsync<bool>(
            "SELECT EXISTS(SELECT FROM guilds.channel_commands WHERE guild_id = @Id AND channel_id = @ChannelId AND command_id = @command);", new { guild.Id, guild.ChannelId, command });
    }

    public async Task<bool> SpamChannelAsync(ScenarioGuild guild)
    {
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<bool>(
            "SELECT is_spam FROM guilds.text_channels WHERE guild_id = @Id AND channel_id = @ChannelId;", new { guild.Id, guild.ChannelId });
    }

    public async Task<bool> ModmailBlockedAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleAsync<bool>(
            "SELECT EXISTS(SELECT FROM moderation.mod_mail_blocked_users WHERE guild_id = @GuildId AND user_id = @Id);", new { GuildId = guild.Id, user.Id });
    }

    public async Task<DateTimeOffset> IgnoredUntilAsync(ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        var until = await connection.QuerySingleAsync<DateTime>("SELECT ignore_until FROM users.users WHERE user_id = @Id;", new { user.Id });
        return new(DateTime.SpecifyKind(until, DateTimeKind.Utc));
    }

    public async Task<JsonElement> YearbookAsync()
    {
        await using var connection = _database.CreateConnection();
        var json = await connection.QuerySingleAsync<string>("SELECT info_value FROM configuration.application_info WHERE info_key = 'rewardyearbook2025';");
        return JsonSerializer.Deserialize<JsonElement>(json);
    }
}
