using Microsoft.Extensions.Configuration;
using TaylorBot.Net.Application.Environment;

namespace TaylorBot.Net.Application.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddTaylorBotApplicationConfiguration(this IConfigurationBuilder builder, TaylorBotEnvironment environment)
        {
            return builder
                .AddJsonFile(path: $"Settings/logging.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/discord.{environment}.json", optional: false);
        }
    }
}
