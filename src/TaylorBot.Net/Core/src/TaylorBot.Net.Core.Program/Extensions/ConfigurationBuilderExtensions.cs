using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TaylorBot.Net.Core.Program.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddTaylorBotApplication(this IConfigurationBuilder builder, IHostEnvironment environment)
        {
            return builder
                .AddJsonFile(path: "Settings/logging.json", optional: false)
                .AddJsonFile(path: "Settings/discord.json", optional: false)
                .AddJsonFile(path: $"Settings/discord.{environment.EnvironmentName}.json", optional: true);
        }

        public static IConfigurationBuilder AddDatabaseConnection(this IConfigurationBuilder builder, IHostEnvironment environment)
        {
            return builder
                .AddJsonFile(path: "Settings/databaseConnection.json", optional: false)
                .AddJsonFile(path: $"Settings/databaseConnection.{environment.EnvironmentName}.json", optional: true);
        }

        public static IConfigurationBuilder AddRedisConnection(this IConfigurationBuilder builder, IHostEnvironment environment)
        {
            return builder
                .AddJsonFile(path: "Settings/redisCommandsConnection.json", optional: false);
        }
    }
}
