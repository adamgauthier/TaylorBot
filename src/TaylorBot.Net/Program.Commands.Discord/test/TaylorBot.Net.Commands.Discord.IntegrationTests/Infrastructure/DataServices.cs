using System.Security.Cryptography;
using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Npgsql;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;

public sealed class DataServices : IAsyncLifetime
{
    private const string PostgresImage = "postgres:15@sha256:724292da1f2e50bdccfc3302ce75bbba7f4a6076701b588cc795fcac65683550";
    private const string RedisImage = "redis:7.4@sha256:c6eabf748fc7a61dbb5a705c78bcf3d6377b1127a97d0ce965c11c44ba46896f";
    private const string SqitchImage = "sqitch/sqitch:latest@sha256:f247ab0e0b66e9c2d09a400864f7314358893f5cf209cddcc4f213f7d5bfe4d3";
    private const string TemplateDatabase = "integration_template";
    private const string PostgresNetworkAlias = "taylorbot-integration-postgres";
    private const string RedisNetworkAlias = "taylorbot-integration-redis";

    private readonly string _password = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
    private readonly INetwork? _ownedNetwork;
    private readonly string _networkName;
    private readonly PostgreSqlContainer _postgres;
    private readonly RedisContainer _redis;
    private ConnectionMultiplexer? _redisConnection;
    private Exception? _failure;
    public string RedisHost => _ownedNetwork == null ? RedisNetworkAlias : _redis.Hostname;
    public ushort RedisPort => _ownedNetwork == null ? (ushort)6379 : _redis.GetMappedPublicPort(6379);
    private string PostgresConnectionString => new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
    {
        Host = _ownedNetwork == null ? PostgresNetworkAlias : _postgres.Hostname,
        Port = _ownedNetwork == null ? 5432 : _postgres.GetMappedPublicPort(5432),
        GssEncryptionMode = GssEncryptionMode.Disable,
    }.ConnectionString;
    internal void MarkUnusable(Exception failure) => _failure = failure;

    public DataServices()
    {
        var networkName = Environment.GetEnvironmentVariable("TAYLORBOT_TEST_NETWORK");
        if (networkName == null)
        {
            networkName = $"taylorbot-integration-{Guid.NewGuid():N}";
            _ownedNetwork = new NetworkBuilder().WithName(networkName).Build();
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(networkName);
        }
        _networkName = networkName;

        _postgres = new PostgreSqlBuilder(PostgresImage)
            .WithPassword(_password)
            .WithNetwork(networkName)
            .WithNetworkAliases(PostgresNetworkAlias)
            .Build();
        _redis = new RedisBuilder(RedisImage)
            .WithNetwork(networkName)
            .WithNetworkAliases(RedisNetworkAlias)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        using CancellationTokenSource deadline = new(TimeSpan.FromMinutes(5));
        if (_ownedNetwork != null)
        {
            await _ownedNetwork.CreateAsync(deadline.Token);
        }
        await Task.WhenAll(_postgres.StartAsync(deadline.Token), _redis.StartAsync(deadline.Token));

        await using NpgsqlConnection admin = new(PostgresConnectionString);
        await admin.OpenAsync(deadline.Token);
        // The role password is generated hex, never supplied by a caller.
        await admin.ExecuteAsync($"CREATE ROLE taylorbot LOGIN PASSWORD '{_password}';");
        await admin.ExecuteAsync($"CREATE DATABASE {TemplateDatabase} OWNER taylorbot;");

        await using (NpgsqlConnection templateAdmin = new(new NpgsqlConnectionStringBuilder(PostgresConnectionString)
        {
            Database = TemplateDatabase,
            Pooling = false,
        }.ConnectionString))
        {
            await templateAdmin.OpenAsync(deadline.Token);
            await templateAdmin.ExecuteAsync("CREATE EXTENSION pgcrypto; GRANT ALL ON SCHEMA public TO taylorbot;");
        }

        await using var sqitch = new ContainerBuilder(SqitchImage)
            .WithNetwork(_networkName)
            .WithResourceMapping(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Sqitch")), "/repo")
            .WithWorkingDirectory("/repo")
            .WithEntrypoint("sleep")
            .WithCommand("infinity")
            .Build();
        await sqitch.StartAsync(deadline.Token);
        var deployment = await sqitch.ExecAsync(
            ["sqitch", "deploy", $"db:pg://taylorbot:{_password}@{PostgresNetworkAlias}/{TemplateDatabase}"],
            deadline.Token);
        if (deployment.ExitCode != 0)
        {
            throw new InvalidOperationException($"Sqitch deployment failed:\n{deployment.Stdout}\n{deployment.Stderr}");
        }

        _redisConnection = await ConnectionMultiplexer.ConnectAsync($"{RedisHost}:{RedisPort},allowAdmin=true");
    }

    internal async Task<ScenarioDatabase> CreateDatabaseAsync()
    {
        if (_failure != null)
        {
            throw new InvalidOperationException("A previous scenario could not shut down safely. Refusing to reuse its Redis instance.", _failure);
        }
        var name = $"scenario_{Guid.NewGuid():N}";
        await using NpgsqlConnection admin = new(PostgresConnectionString);
        await admin.OpenAsync();
        await admin.ExecuteAsync($"CREATE DATABASE {name} WITH TEMPLATE {TemplateDatabase} OWNER taylorbot;");

        var redis = _redisConnection ?? throw new InvalidOperationException("Data services have not started.");
        await redis.GetServer(redis.GetEndPoints().Single()).FlushDatabaseAsync();

        return new(
            new NpgsqlConnectionStringBuilder(PostgresConnectionString)
            {
                Database = name,
                Username = "taylorbot",
                Password = _password,
                Pooling = false,
            }.ConnectionString,
            PostgresConnectionString,
            name,
            redis.GetDatabase());
    }

    public async ValueTask DisposeAsync()
    {
        if (_redisConnection != null)
        {
            await _redisConnection.DisposeAsync();
        }

        try
        {
            await _redis.DisposeAsync();
        }
        finally
        {
            try
            {
                await _postgres.DisposeAsync();
            }
            finally
            {
                if (_ownedNetwork != null)
                {
                    await _ownedNetwork.DisposeAsync();
                }
            }
        }
    }
}

internal sealed class ScenarioDatabase(
    string connectionString,
    string adminConnectionString,
    string databaseName,
    IDatabase redis) : IAsyncDisposable
{
    private bool _disposed;
    public string ConnectionString { get; } = connectionString;
    public IDatabase Redis { get; } = redis;

    public NpgsqlConnection CreateConnection() => new(ConnectionString);

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        if (!databaseName.StartsWith("scenario_", StringComparison.Ordinal) || !Guid.TryParseExact(databaseName["scenario_".Length..], "N", out _))
        {
            throw new InvalidOperationException("Refusing to drop a database that is not a generated scenario database.");
        }
        await using NpgsqlConnection connection = new(adminConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync($"DROP DATABASE {databaseName};");
    }
}
