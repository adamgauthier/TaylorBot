using Discord;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Infrastructure;

public record TaypointAmount(AbsoluteTaypointAmount? Absolute, RelativeTaypointAmount? Relative)
{
    public static TaypointAmount From(ITaypointAmount amount)
    {
        return amount switch
        {
            AbsoluteTaypointAmount absolute => new(absolute, null),
            RelativeTaypointAmount relative => new(null, relative),
            _ => throw new NotImplementedException(),
        };
    }
}

public class HeistRedisRepository(ConnectionMultiplexer connectionMultiplexer) : IHeistRepository
{
    private string GetKey(IGuild guild) => $"heist:guild:{guild.Id}";

    public async Task<List<HeistPlayer>> EndHeistAsync(IGuild guild)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(guild);

        var transaction = redis.CreateTransaction();
        var hashGetAllTask = transaction.HashGetAllAsync(key);
        var keyDeleteTask = transaction.KeyDeleteAsync(key);
        await transaction.ExecuteAsync();

        await keyDeleteTask;
        var hash = await hashGetAllTask;

        return hash.Select(entry =>
        {
            string? userId = entry.Name;
            string? json = entry.Value;

            var deserialized = JsonSerializer.Deserialize<TaypointAmount>(json ?? throw new ArgumentNullException(nameof(entry.Value)))
                ?? throw new ArgumentNullException(nameof(json));

            ITaypointAmount amount = deserialized.Absolute != null
                ? deserialized.Absolute
                : deserialized.Relative ?? throw new NotImplementedException();

            return new HeistPlayer(
                userId ?? throw new ArgumentNullException(nameof(userId)),
                amount);
        }).ToList();
    }

    private static readonly LuaScript EnterHeistScript = LuaScript.Prepare(
        """
        local exists = redis.call('exists', @key)
        local wasAdded = redis.call('hset', @key, @userid, @amount)
        if exists == 0 then
            redis.call('expire', @key, @expiryseconds)
        end
        return {exists, wasAdded}
        """);

    public async Task<IEnterHeistResult> EnterHeistAsync(IGuildUser user, ITaypointAmount amount, TimeSpan heistDelay)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(user.Guild);

        var result = await redis.ScriptEvaluateAsync(EnterHeistScript, new
        {
            key = (RedisKey)key,
            userid = $"{user.Id}",
            amount = JsonSerializer.Serialize(TaypointAmount.From(amount)),
            expiryseconds = heistDelay.Add(TimeSpan.FromMinutes(1)).TotalSeconds,
        });

        var exists = (bool)result[0];
        var wasAdded = (bool)result[1];

        if (!exists)
        {
            return new HeistCreated();
        }
        else if (wasAdded)
        {
            return new HeistEntered();
        }
        else
        {
            return new InvestmentUpdated();
        }
    }
}

public class HeistInMemoryRepository : IHeistRepository
{
    private readonly ConcurrentDictionary<ulong, Heist> heistsByGuild = [];

    private record Heist(ConcurrentDictionary<ulong, ITaypointAmount> Players);

    public Task<List<HeistPlayer>> EndHeistAsync(IGuild guild)
    {
        if (!heistsByGuild.TryRemove(guild.Id, out var heist))
        {
            throw new ArgumentNullException(nameof(heist));
        }

        return Task.FromResult(heist.Players.Select(p => new HeistPlayer($"{p.Key}", p.Value)).ToList());
    }

    public Task<IEnterHeistResult> EnterHeistAsync(IGuildUser user, ITaypointAmount amount, TimeSpan heistDelay)
    {
        var wasUpdated = false;
        var newHeist = new Heist(new([new(user.Id, amount)]));

        var result = heistsByGuild.AddOrUpdate(
            user.Guild.Id,
            newHeist,
            (_, heist) =>
            {
                heist.Players.AddOrUpdate(user.Id, amount, (_, _) =>
                {
                    wasUpdated = true;
                    return amount;
                });
                return heist;
            });

        if (result == newHeist)
        {
            return Task.FromResult<IEnterHeistResult>(new HeistCreated());
        }
        else if (wasUpdated)
        {
            return Task.FromResult<IEnterHeistResult>(new InvestmentUpdated());
        }
        else
        {
            return Task.FromResult<IEnterHeistResult>(new HeistEntered());
        }
    }
}
