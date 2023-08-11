using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Infrastructure;

public class LocationPostgresRepository : ILocationRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public LocationPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    private record LocationDto(string latitude, string longitude, string formatted_address, string timezone_id);

    public async ValueTask<StoredLocation?> GetLocationAsync(IUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var location = await connection.QuerySingleOrDefaultAsync<LocationDto?>(
            """
            SELECT latitude, longitude, formatted_address, timezone_id
            FROM attributes.location_attributes
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );

        return location != null ? new StoredLocation(
            new(
                Latitude: location.latitude,
                Longitude: location.longitude,
                FormattedAddress: location.formatted_address),
            TimeZoneId: location.timezone_id
        ) : null;
    }

    public async ValueTask SetLocationAsync(IUser user, StoredLocation location)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO attributes.location_attributes (user_id, formatted_address, longitude, latitude, timezone_id)
            VALUES (@UserId, @FormattedAddress, @Longitude, @Latitude, @TimezoneId)
            ON CONFLICT (user_id) DO UPDATE SET
              formatted_address = excluded.formatted_address,
              longitude = excluded.longitude,
              latitude = excluded.latitude,
              timezone_id = excluded.timezone_id;
            """,
            new
            {
                UserId = $"{user.Id}",
                location.Location.FormattedAddress,
                location.Location.Longitude,
                location.Location.Latitude,
                TimezoneId = location.TimeZoneId,
            }
        );
    }

    public async ValueTask ClearLocationAsync(IUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM attributes.location_attributes WHERE user_id = @UserId;",
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }
}
