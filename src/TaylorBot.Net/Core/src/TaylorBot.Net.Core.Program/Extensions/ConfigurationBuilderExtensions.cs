using Microsoft.Extensions.Configuration;
using TaylorBot.Net.Core.Environment;

namespace TaylorBot.Net.Core.Program.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddTaylorBotApplication(this IConfigurationBuilder builder, TaylorBotEnvironment environment)
        {
            return builder
                .AddJsonFile(path: $"Settings/logging.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/discord.{environment}.json", optional: false);
        }
    }
}
