using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Infrastructure
{
    public class LocationPostgresRepository : ILocationRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public LocationPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private record LocationDto(string latitude, string longitude, string formatted_address);

        public async ValueTask<Location?> GetLocationAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var location = await connection.QuerySingleOrDefaultAsync<LocationDto?>(
                @"SELECT latitude, longitude, formatted_address
                FROM attributes.location_attributes
                WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString()
                }
            );

            return location != null ? new Location(
                Latitude: location.latitude,
                Longitude: location.longitude,
                FormattedAddress: location.formatted_address
            ) : null;
        }
    }
}
