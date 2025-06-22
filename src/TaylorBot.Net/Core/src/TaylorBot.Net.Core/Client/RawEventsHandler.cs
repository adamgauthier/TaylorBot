using Discord.WebSocket;
using System.Reflection;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Client;

public class RawEventsHandler(TaskExceptionLogger taskExceptionLogger)
{
    private static readonly MethodInfo Handler = typeof(RawEventsHandler)
        .GetMethod(nameof(ProcessMessageAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Dictionary<string, Func<string, Task>> Callbacks { get; } = [];

    public void HandleRawEvent(DiscordSocketClient client, string eventName, Func<string, Task> callback)
    {
        ApiClientAccessor.AddReceivedGatewayEventHandler(client, this, Handler);

        Callbacks.TryAdd(eventName, callback);
    }

    private const byte DispatchGatewayOpCode = 0;

    private Task ProcessMessageAsync(byte opCode, int? seq, string type, object payload)
    {
        _ = taskExceptionLogger.LogOnError(
            async () =>
            {
                if (opCode == DispatchGatewayOpCode && Callbacks.TryGetValue(type, out var callback))
                {
                    var stringPayload = payload.ToString();
                    ArgumentNullException.ThrowIfNull(stringPayload);

                    await callback(stringPayload);
                }
            },
            nameof(ProcessMessageAsync)
        );
        return Task.CompletedTask;
    }
}
