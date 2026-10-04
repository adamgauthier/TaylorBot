using Dapper;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioState
{
    private readonly ScenarioDatabase _database;

    internal ScenarioState(ScenarioDatabase database) => _database = database;

    public async Task<long?> LastKnownTaypointsAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleAsync<long?>(
            "SELECT last_known_taypoint_count FROM guilds.guild_members WHERE guild_id = @Id AND user_id = @UserId;",
            new { guild.Id, UserId = user.Id });
    }

    public async Task<string?> CachedDisabledReasonAsync(string commandName) =>
        await _database.Redis.HashGetAsync("disabled-command-messages", commandName);

    public Task<long> BalanceAsync(ScenarioUser user) =>
        ReadAsync<long>("SELECT taypoint_count FROM users.users WHERE user_id = @Id;", new { user.Id });

    public Task<string?> SuccessorAsync(ScenarioUser user) =>
        ReadAsync<string?>("SELECT beneficiary_user_id FROM users.taypoint_wills WHERE owner_user_id = @Id;", new { user.Id });

    public Task<bool> UsernameHistoryHiddenAsync(ScenarioUser user) =>
        ReadAsync<bool>("SELECT is_hidden FROM users.username_history_configuration WHERE user_id = @Id;", new { user.Id });

    public Task<string?> LastFmAsync(ScenarioUser user) =>
        ReadAsync<string?>("SELECT attribute_value FROM attributes.text_attributes WHERE user_id = @Id AND attribute_id = 'lastfm';", new { user.Id });

    public Task<string?> PlusGuildStateAsync(ScenarioGuild guild, ScenarioUser user) =>
        ReadAsync<string?>("SELECT state FROM plus.plus_guilds WHERE guild_id = @GuildId AND plus_user_id = @Id;", new { GuildId = guild.Id, user.Id });

    public Task<bool> RoleAccessibleAsync(ScenarioGuild guild, ScenarioRole role) =>
        ReadAsync<bool>("SELECT accessible FROM guilds.guild_accessible_roles WHERE guild_id = @GuildId AND role_id = @Id;", new { GuildId = guild.Id, role.Id });

    public Task<string?> RoleGroupAsync(ScenarioGuild guild, ScenarioRole role) =>
        ReadAsync<string?>("SELECT group_name FROM guilds.guild_accessible_roles WHERE guild_id = @GuildId AND role_id = @Id;", new { GuildId = guild.Id, role.Id });

    public Task<string?> JailRoleAsync(ScenarioGuild guild) =>
        ReadAsync<string?>("SELECT jail_role_id FROM guilds.jail_roles WHERE guild_id = @Id;", new { guild.Id });

    public Task<string?> PrefixAsync(ScenarioGuild guild) =>
        ReadAsync<string?>("SELECT prefix FROM guilds.guilds WHERE guild_id = @Id;", new { guild.Id });

    public Task<string?> DisabledReasonAsync(string command) =>
        ReadAsync<string?>("SELECT disabled_message FROM commands.commands WHERE name = @command;", new { command });

    private async Task<T?> ReadAsync<T>(string sql, object parameters)
    {
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
    }
}
