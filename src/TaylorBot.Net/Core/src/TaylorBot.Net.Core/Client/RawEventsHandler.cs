using Discord.WebSocket;
using System.Reflection;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Client;

public class RawEventsHandler(TaskExceptionLogger taskExceptionLogger)
{
    private static readonly MethodInfo GetApiClientProperty = typeof(DiscordSocketClient)
        .GetProperty("ApiClient", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        ?.GetGetMethod(nonPublic: true)
        ?? throw new ArgumentNullException(nameof(GetApiClientProperty));

    private static readonly MethodInfo Handler = typeof(RawEventsHandler)
        .GetMethod(nameof(ProcessMessageAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Dictionary<string, Func<string, Task>> Callbacks { get; } = [];

    public void HandleRawEvent(DiscordSocketClient client, string eventName, Func<string, Task> callback)
    {
        var apiClient = GetApiClientProperty.Invoke(client, []);
        ArgumentNullException.ThrowIfNull(apiClient);

        var receivedEvent = apiClient.GetType().GetEvent("ReceivedGatewayEvent");
        ArgumentNullException.ThrowIfNull(receivedEvent);
        ArgumentNullException.ThrowIfNull(receivedEvent.EventHandlerType);

        var delegateInstance = Delegate.CreateDelegate(receivedEvent.EventHandlerType, this, Handler);

        var addHandler = receivedEvent.GetAddMethod();
        ArgumentNullException.ThrowIfNull(addHandler);

        addHandler.Invoke(apiClient, [delegateInstance]);

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
