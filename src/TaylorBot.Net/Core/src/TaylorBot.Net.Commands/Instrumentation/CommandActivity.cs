using System.Diagnostics;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Instrumentation;

public class CommandActivityFactory(TaylorBotInstrumentation instrumentation)
{
    public CommandActivity Create() => new(instrumentation.ActivitySource.StartActivity("TaylorBotCommand"));
}

public enum CommandType
{
    Unknown = 0,
    Slash,
    Prefix,
}

public sealed class CommandActivity(Activity? activity) : IDisposable
{
    public SnowflakeId UserId { set => activity?.SetTag("user.id", value.Id); }

    public SnowflakeId ChannelId { set => activity?.SetTag("channel.id", value.Id); }

    public SnowflakeId? GuildId { set => activity?.SetTag("guild.id", value?.Id); }

    public string CommandName { set => activity?.SetTag("command.name", value); }

    public CommandType Type { set => activity?.SetTag("command.type", value); }

    public void SetError(Exception? e = null)
    {
        activity?.SetStatus(ActivityStatusCode.Error, e?.GetType().Name);
    }

    private bool disposed;

    public void Dispose()
    {
        if (!disposed)
        {
            activity?.Dispose();
            disposed = true;
        }
    }
}
