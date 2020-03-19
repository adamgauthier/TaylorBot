using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.EntityTracker.Domain.Guild;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Guild
{
    public class GuildRepository : PostgresRepository, IGuildRepository
    {
        public GuildRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        private class GuildAddedOrUpdatedDto
        {
            public bool was_inserted { get; set; }
            public bool guild_name_changed { get; set; }
            public string previous_guild_name { get; set; }
        }

        public async ValueTask<GuildAddedResult> AddGuildIfNotAddedAsync(IGuild guild)
        {
            using var connection = Connection;

            var guildAddedOrUpdatedDto = await connection.QuerySingleAsync<GuildAddedOrUpdatedDto>(
                @"INSERT INTO guilds.guilds (guild_id, guild_name, previous_guild_name) VALUES (@GuildId, @GuildName, NULL)
                ON CONFLICT (guild_id) DO UPDATE SET
                    previous_guild_name = guilds.guilds.guild_name,
                    guild_name = excluded.guild_name
                RETURNING previous_guild_name IS NULL AS was_inserted, previous_guild_name IS DISTINCT FROM guild_name AS guild_name_changed, previous_guild_name;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    GuildName = guild.Name
                }
            );

            return new GuildAddedResult(
                wasAdded: guildAddedOrUpdatedDto.was_inserted,
                wasGuildNameChanged: guildAddedOrUpdatedDto.guild_name_changed,
                previousGuildName: guildAddedOrUpdatedDto.previous_guild_name
            );
        }
    }
}
