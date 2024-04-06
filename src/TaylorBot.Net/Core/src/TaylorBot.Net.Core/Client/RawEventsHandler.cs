using Discord.WebSocket;
using System.Reflection;

namespace TaylorBot.Net.Core.Client;

public class RawEventsHandler
{
    private static readonly PropertyInfo ApiClientProperty = typeof(DiscordSocketClient)
        .GetProperty("ApiClient", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!;

    private static readonly MethodInfo Handler = typeof(RawEventsHandler)
        .GetMethod(nameof(ProcessMessageAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Dictionary<string, Func<string, Task>> Callbacks { get; } = [];

    public void HandleRawEvent(DiscordSocketClient client, string eventName, Func<string, Task> callback)
    {
        var apiClient = ApiClientProperty.GetGetMethod(nonPublic: true)!.Invoke(client, [])!;

        var receivedEvent = apiClient.GetType().GetEvent("ReceivedGatewayEvent");
        var delegateInstance = Delegate.CreateDelegate(receivedEvent!.EventHandlerType!, this, Handler);

        var addHandler = receivedEvent.GetAddMethod();
        addHandler!.Invoke(apiClient, new[] { delegateInstance });

        Callbacks.TryAdd(eventName, callback);
    }

    private const byte DispatchGatewayOpCode = 0;

    private async Task ProcessMessageAsync(byte opCode, int? seq, string type, object payload)
    {
        if (opCode == DispatchGatewayOpCode && Callbacks.TryGetValue(type, out var callback))
        {
            await callback(payload.ToString()!);
        }
    }
}
