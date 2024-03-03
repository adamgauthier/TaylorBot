using System.Diagnostics;
using TaylorBot.Net.Core.Program.Instrumentation;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Instrumentation;

public enum CommandType
{
    Unknown = 0,
    Slash,
    Prefix,
}

public class CommandActivityFactory(TaylorBotInstrumentation instrumentation)
{
    public CommandActivity Create(CommandType type)
    {
        var inner = instrumentation.ActivitySource.StartActivity("TaylorBotCommand", ActivityKind.Server);

        // Commands are not http requests, but they are incoming user requests (through Discord WebSockets)
        // By disguising the activity as an http request, we can benefit from Azure Monitor features that
        // are built for incoming http requests (resultCode, pre-built charts/queries)
        inner?.SetTag("http.request.method", "_OTHER");
        inner?.SetTag("http.response.status_code", "Success");

        return new(inner)
        {
            Type = type,
        };
    }
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
        var exceptionName = e?.GetType().Name;

        activity?.SetStatus(ActivityStatusCode.Error, exceptionName);
        activity?.SetTag("http.response.status_code", exceptionName);
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
