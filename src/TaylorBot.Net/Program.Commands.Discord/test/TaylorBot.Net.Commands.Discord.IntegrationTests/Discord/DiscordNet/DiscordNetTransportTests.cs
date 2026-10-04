using Discord;
using Discord.WebSocket;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Tasks;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord.DiscordNet;

public sealed class DiscordNetTransportTests(ITestOutputHelper output)
{
    [Fact]
    public async Task RawGatewayDispatch_ReachesTaylorBotInteractionEvent()
    {
        List<InMemoryWebSocket> sockets = [];
        List<LogMessage> errors = [];
        using DiscordShardedClient client = new(new DiscordSocketConfig
        {
            TotalShards = 1,
            GatewayIntents = GatewayIntents.Guilds,
            LogLevel = LogSeverity.Debug,
            RestClientProvider = _ => new InMemoryRestClient(new DiscordApi()),
            WebSocketProvider = () =>
            {
                InMemoryWebSocket socket = new();
                sockets.Add(socket);
                return socket;
            },
        });
        client.Log += message =>
        {
            output.WriteLine(message.ToString());
            if (message.Severity <= LogSeverity.Error)
            {
                errors.Add(message);
            }
            return Task.CompletedTask;
        };
        TaylorBotClient bot = new(
            NullLogger<TaylorBotClient>.Instance,
            new LogSeverityToLogLevelMapper(),
            new TaylorBotToken("MTAwMDAwMDAwMDAwMDAwMDAx.synthetic.synthetic-test-token-never-a-real-credential"),
            new RawEventsHandler(new BackgroundTasks(NullLogger<BackgroundTasks>.Instance)),
            client);
        TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<Interaction> received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ShardReady += _ => { ready.SetResult(); return Task.CompletedTask; };
        bot.InteractionCreated += interaction => { received.SetResult(interaction); return Task.CompletedTask; };

        try
        {
            await bot.StartAsync();
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
            await sockets[^1].ReceiveAsync("""
                {
                  "op":0,"s":2,"t":"INTERACTION_CREATE",
                  "d":{
                    "id":"100000000000000010","application_id":"100000000000000001","type":2,
                    "token":"synthetic-interaction","version":1,"entitlements":[],"app_permissions":"562949953863680",
                    "context":1,"authorizing_integration_owners":{"0":"0"},"attachment_size_limit":10485760,"locale":"en-US",
                    "user":{"id":"100000000000000003","username":"Alice","discriminator":"0","avatar":null},
                    "channel_id":"100000000000000004",
                    "channel":{"id":"100000000000000004","type":1,"recipients":[{"id":"100000000000000001","username":"IntegrationBot","discriminator":"0","avatar":null,"bot":true}]},
                    "data":{"id":"100000000000000005","name":"taypoints","type":1,"options":[{"name":"balance","type":1,"options":[]}]}
                  }
                }
                """);

            (await received.Task.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken)).data!.name.Should().Be("taypoints");
            errors.Should().BeEmpty();
        }
        finally
        {
            await bot.StopAsync();
        }
    }
}
