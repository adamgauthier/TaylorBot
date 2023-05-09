using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Core.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresConnection(this IServiceCollection services, IConfiguration configuration)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        return services
            .AddNpgsqlDataSource(CreateConnectionString(configuration))
            .AddTransient<PostgresConnectionFactory>();
    }

    private static string CreateConnectionString(IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection("DatabaseConnection");
        var host = section.GetValue<string>("Host");
        var port = section.GetValue<uint?>("Port");
        var username = section.GetValue<string>("Username");
        var password = section.GetValue<string>("Password");
        var database = section.GetValue<string>("Database");
        var applicationName = section.GetValue<string>("ApplicationName");
        var maxPoolSize = section.GetValue<uint?>("MaxPoolSize");

        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(port);
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(applicationName);
        ArgumentNullException.ThrowIfNull(maxPoolSize);

        var connectionString = string.Join(';', new[] {
            $"Server={host}",
            $"Port={port}",
            $"Username={username}",
            $"Password={password}",
            $"Database={database}",
            $"ApplicationName={applicationName}",
            $"Maximum Pool Size={maxPoolSize}",
            "SSL Mode=Prefer",
            "Trust Server Certificate=true",
        });
        return connectionString;
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
