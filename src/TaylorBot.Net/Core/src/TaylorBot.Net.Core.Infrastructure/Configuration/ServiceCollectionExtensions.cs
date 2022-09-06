using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Core.Infrastructure.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPostgresConnection(this IServiceCollection services, IConfiguration configuration)
        {
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

            return services
                .ConfigureRequired<DatabaseConnectionOptions>(configuration, "DatabaseConnection")
                .AddTransient<PostgresConnectionFactory>();
        }

        public static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .ConfigureRequired<RedisConnectionOptions>(configuration, "RedisConnection")
                .AddSingleton(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<RedisConnectionOptions>>().CurrentValue;
                    return ConnectionMultiplexer.Connect($"{options.Host}:{options.Port},password={options.Password}");
                });
        }
    }
}
