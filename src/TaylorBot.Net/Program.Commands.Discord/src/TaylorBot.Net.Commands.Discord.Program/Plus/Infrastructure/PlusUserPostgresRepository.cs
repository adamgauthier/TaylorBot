using Dapper;
using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Plus.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Plus.Infrastructure
{
    public class PlusUserPostgresRepository : IPlusUserRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public PlusUserPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class PlusGuildDtoDto
        {
            public bool active { get; set; }
            public int max_plus_guilds { get; set; }
            public string? guild_name { get; set; }
        }

        public async ValueTask<PlusUser?> GetPlusUserAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var plusGuilds = (await connection.QueryAsync<PlusGuildDtoDto>(
                @"SELECT active, max_plus_guilds, guild_name
                FROM plus.plus_users
                LEFT JOIN plus.plus_guilds ON user_id = plus_user_id AND state = 'enabled'
                LEFT JOIN guilds.guilds ON plus.plus_guilds.guild_id = guilds.guilds.guild_id
                WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString()
                }
            )).ToList();

            return plusGuilds.Count > 0 ?
                new PlusUser(
                    IsActive: plusGuilds[0].active,
                    MaxPlusGuilds: plusGuilds[0].max_plus_guilds,
                    ActivePlusGuilds: plusGuilds[0].guild_name != null ? plusGuilds.Select(g => g.guild_name!).ToList() : Array.Empty<string>()
                ) :
                null;
        }

        public async ValueTask AddPlusGuildAsync(IUser user, IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO plus.plus_guilds (guild_id, plus_user_id, state)
                VALUES (@GuildId, @UserId, 'enabled')
                ON CONFLICT (guild_id, plus_user_id)
                DO UPDATE SET state = excluded.state;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    UserId = user.Id.ToString()
                }
            );
        }

        public async ValueTask DisablePlusGuildAsync(IUser user, IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE plus.plus_guilds
                SET state = 'user_disabled'
                WHERE guild_id = @GuildId AND plus_user_id = @UserId;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    UserId = user.Id.ToString()
                }
            );
        }
    }
}
