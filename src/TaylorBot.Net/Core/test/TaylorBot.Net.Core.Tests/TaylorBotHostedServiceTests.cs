using Discord.WebSocket;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Options;
using TaylorBot.Net.Core.Tasks;
using Xunit;

namespace TaylorBot.Net.Core.Tests;

public sealed class TaylorBotHostedServiceTests
{
    [Fact]
    public async Task StopAsync_StopsClientThenWaitsForFiniteWork()
    {
        var client = A.Fake<ITaylorBotClient>();
        BackgroundTasks tasks = new(NullLogger<BackgroundTasks>.Instance);
        using var host = CreateHost(client, tasks);
        await host.StartAsync(TestContext.Current.CancellationToken);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var work = tasks.Queue(() => release.Task, "pending write");

        var shutdown = host.StopAsync(TestContext.Current.CancellationToken);

        A.CallTo(() => client.StopAsync()).MustHaveHappenedOnceExactly();
        shutdown.IsCompleted.Should().BeFalse();
        release.SetResult();
        await shutdown;
        await work;
    }

    [Fact]
    public async Task StopAsync_RespectsDeadlineWithoutClaimingWorkWasCancelled()
    {
        var client = A.Fake<ITaylorBotClient>();
        BackgroundTasks tasks = new(NullLogger<BackgroundTasks>.Instance);
        using var host = CreateHost(client, tasks);
        await host.StartAsync(TestContext.Current.CancellationToken);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var work = tasks.Queue(() => release.Task, "pending write");
        using CancellationTokenSource deadline = new();
        var shutdown = host.StopAsync(deadline.Token);

        await deadline.CancelAsync();

        await new Func<Task>(() => shutdown).Should().ThrowAsync<OperationCanceledException>();
        work.IsCompleted.Should().BeFalse();
        release.SetResult();
        await work;
        await tasks.DrainAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void DiscordOptions_PreservesProductionBootLoopDelay()
    {
        DiscordOptions options = new();

        options.StartupDelay.Should().Be(TimeSpan.FromSeconds(5));
    }

    private static IHost CreateHost(ITaylorBotClient client, BackgroundTasks tasks)
    {
        return new HostBuilder()
            .ConfigureServices(services => services
                .AddLogging()
                .AddSingleton(client)
                .AddSingleton(tasks)
                .AddSingleton<DiscordSocketConfig>()
                .AddSingleton<TaskExceptionLogger>()
                .AddSingleton(A.Fake<IInteractionCreatedHandler>())
                .Configure<DiscordOptions>(options => options.StartupDelay = TimeSpan.Zero)
                .AddHostedService<TaylorBotHostedService>())
            .Build();
    }
}
