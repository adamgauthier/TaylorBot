using Microsoft.Extensions.Configuration;
using TaylorBot.Net.Core.Environment;

namespace TaylorBot.Net.Core.Infrastructure.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDatabaseConnectionConfiguration(this IConfigurationBuilder builder, TaylorBotEnvironment environment)
        {
            return builder
                .AddJsonFile(path: $"Settings/databaseConnection.{environment}.json", optional: false);
        }
    }
}
