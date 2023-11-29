using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaylorBot.Net.Core.Configuration;

public static class ConfigurationServiceCollectionExtensions
{
    public static IServiceCollection ConfigureRequired<TOptions>(this IServiceCollection services, IConfiguration config, string subSectionName) where TOptions : class
    {
        var subSection = config.GetSection(subSectionName);

        if (!subSection.Exists())
        {
            throw new ArgumentException($"Configuration section {subSectionName} does not exist.");
        }

        return services.Configure<TOptions>(subSection);
    }
}
