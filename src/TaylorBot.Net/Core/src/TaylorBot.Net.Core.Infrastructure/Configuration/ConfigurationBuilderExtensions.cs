using Microsoft.Extensions.Configuration;
using TaylorBot.Net.Core.Environment;

namespace TaylorBot.Net.Core.Infrastructure.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDatabaseConnection(this IConfigurationBuilder builder, TaylorBotEnvironment environment)
        {
            return builder
                .AddJsonFile(path: $"Settings/databaseConnection.{environment}.json", optional: false);
        }

        public static IConfigurationBuilder AddRedisConnection(this IConfigurationBuilder builder, TaylorBotEnvironment environment)
        {
            return builder
                .AddJsonFile(path: $"Settings/redisCommandsConnection.{environment}.json", optional: false);
        }
    }
}
