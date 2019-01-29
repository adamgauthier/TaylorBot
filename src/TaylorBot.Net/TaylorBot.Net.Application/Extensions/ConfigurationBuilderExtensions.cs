using Microsoft.Extensions.Configuration;
using TaylorBot.Net.Application.Environment;

namespace TaylorBot.Net.Application.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDefaultTaylorBotConfiguration(this IConfigurationBuilder builder, TaylorBotEnvironment environment)
        {
            return builder
                .AddJsonFile(path: $"Settings/discord.{environment}.json", optional: false);
        }
    }
}
