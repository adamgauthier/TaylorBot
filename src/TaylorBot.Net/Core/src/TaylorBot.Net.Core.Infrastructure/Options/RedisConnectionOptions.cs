namespace TaylorBot.Net.Core.Infrastructure.Options;

public class RedisConnectionOptions
{
    public string Host { get; set; } = null!;
    public uint Port { get; set; }
    public string Password { get; set; } = null!;
}
