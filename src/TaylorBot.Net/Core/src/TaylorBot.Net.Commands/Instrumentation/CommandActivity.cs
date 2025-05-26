using System.Diagnostics;
using TaylorBot.Net.Core.Program.Instrumentation;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Instrumentation;

public enum CommandType
{
    Unknown = 0,
    Slash,
    Prefix,
    MessageComponent,
    ModalSubmit,
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

        CommandActivity activity = new(inner);
        activity.SetType(type);
        return activity;
    }
}

public sealed class CommandActivity(Activity? activity) : IDisposable
{
    public void SetUserId(SnowflakeId value)
    {
        activity?.SetTag("user.id", value.Id);
    }

    public void SetChannelId(SnowflakeId value)
    {
        activity?.SetTag("channel.id", value.Id);
    }

    public void SetGuildId(SnowflakeId? value)
    {
        activity?.SetTag("guild.id", value?.Id);
    }

    public void SetCommandName(string? value)
    {
        if (value != null)
        {
            activity?.SetTag("command.name", value);
        }
    }

    public void SetType(CommandType value)
    {
        activity?.SetTag("command.type", value);
    }

    public void SetOption(string name, string value)
    {
        activity?.SetTag($"command.options.{name}", value);
    }

    public void SetError(Exception? e = null)
    {
        if (activity == null)
        {
            return;
        }

        var errorCode = e?.GetType().Name ?? "Unknown";

        activity.SetStatus(ActivityStatusCode.Error, errorCode);
        activity.SetTag("http.response.status_code", errorCode);
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
