using Discord.WebSocket;
using System.Reflection;

namespace TaylorBot.Net.Core.Client;

public static class ApiClientAccessor
{
    private static readonly MethodInfo GetApiClientProperty = typeof(DiscordSocketClient)
        .GetProperty("ApiClient", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        ?.GetGetMethod(nonPublic: true)
        ?? throw new ArgumentNullException(nameof(GetApiClientProperty));

    public static void AddReceivedGatewayEventHandler(DiscordSocketClient client, object target, MethodInfo handler)
    {
        var apiClient = GetApiClientProperty.Invoke(client, []);
        ArgumentNullException.ThrowIfNull(apiClient);

        var receivedEvent = apiClient.GetType().GetEvent("ReceivedGatewayEvent");
        ArgumentNullException.ThrowIfNull(receivedEvent);
        ArgumentNullException.ThrowIfNull(receivedEvent.EventHandlerType);

        var delegateInstance = Delegate.CreateDelegate(receivedEvent.EventHandlerType, target, handler);

        var addHandler = receivedEvent.GetAddMethod();
        ArgumentNullException.ThrowIfNull(addHandler);

        addHandler.Invoke(apiClient, [delegateInstance]);
    }
}
