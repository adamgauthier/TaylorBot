﻿using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Infrastructure;

public class LastFmUsernamePostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ILastFmUsernameRepository
{
    public async ValueTask<LastFmUsername?> GetLastFmUsernameAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var username = await connection.QuerySingleOrDefaultAsync<string?>(
            "SELECT attribute_value FROM attributes.text_attributes WHERE user_id = @UserId AND attribute_id = 'lastfm';",
            new
            {
                UserId = $"{user.Id}",
            }
        );

        return username == null ? null : new LastFmUsername(username);
    }

    public async ValueTask SetLastFmUsernameAsync(DiscordUser user, LastFmUsername lastFmUsername)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO attributes.text_attributes (user_id, attribute_id, attribute_value) VALUES (@UserId, 'lastfm', @LastFmUsername)
            ON CONFLICT (user_id, attribute_id) DO UPDATE SET attribute_value = excluded.attribute_value;
            """,
            new
            {
                UserId = $"{user.Id}",
                LastFmUsername = lastFmUsername.Username,
            }
        );
    }

    public async ValueTask ClearLastFmUsernameAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"DELETE FROM attributes.text_attributes WHERE user_id = @UserId AND attribute_id = 'lastfm';",
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }
}
