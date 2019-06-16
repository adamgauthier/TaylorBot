using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Core.Infrastructure.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .ConfigureRequired<DatabaseConnectionOptions>(configuration, "DatabaseConnection");
        }
    }
}
