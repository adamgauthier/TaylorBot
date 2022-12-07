using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Infrastructure
{
    public class ModMailBlockedUsersPostgresRepository : IModMailBlockedUsersRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public ModMailBlockedUsersPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<int> GetBlockedUserCountAsync(IGuild guild)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<int>(
                @"SELECT COUNT(*) FROM moderation.mod_mail_blocked_users
                WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );
        }

        public async ValueTask BlockAsync(IGuild guild, IUser user)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO moderation.mod_mail_blocked_users (guild_id, user_id)
                VALUES (@GuildId, @UserId)
                ON CONFLICT (guild_id, user_id) DO NOTHING;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    UserId = user.Id.ToString()
                }
            );
        }

        public async ValueTask UnblockAsync(IGuild guild, IUser user)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "DELETE FROM moderation.mod_mail_blocked_users WHERE guild_id = @GuildId AND user_id = @UserId;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    UserId = user.Id.ToString()
                }
            );
        }

        public async ValueTask<bool> IsBlockedAsync(IGuild guild, IUser user)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<bool>(
                @"SELECT EXISTS(
                    SELECT FROM moderation.mod_mail_blocked_users WHERE guild_id = @GuildId AND user_id = @UserId
                );",
                new
                {
                    GuildId = guild.Id.ToString(),
                    UserId = user.Id.ToString()
                }
            );
        }
    }
}
