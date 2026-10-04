using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaylorBot.Net.Core.Tasks;
using Xunit;

namespace TaylorBot.Net.Core.Tests;

public sealed class BackgroundTasksTests
{
    [Fact]
    public async Task Run_StartsInline()
    {
        BackgroundTasks tasks = new(NullLogger<BackgroundTasks>.Instance);
        var started = false;

        var work = tasks.Run(() =>
        {
            started = true;
            return Task.CompletedTask;
        }, "inline");

        started.Should().BeTrue();
        await work;
    }

    [Fact]
    public async Task Queue_RegistersWorkBeforeReturning()
    {
        BackgroundTasks tasks = new(NullLogger<BackgroundTasks>.Instance);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var work = tasks.Queue(() => release.Task, "queued");

        var drain = tasks.DrainAsync(TestContext.Current.CancellationToken);

        drain.IsCompleted.Should().BeFalse();
        release.SetResult();
        await drain;
        await work;
    }

    [Fact]
    public async Task DrainAsync_WaitsForWorkCreatedWhileDraining()
    {
        BackgroundTasks tasks = new(NullLogger<BackgroundTasks>.Instance);
        TaskCompletionSource releaseParent = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseChild = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var parent = tasks.Run(async () =>
        {
            await releaseParent.Task;
            _ = tasks.Queue(() => releaseChild.Task, "child");
        }, "parent");

        var drain = tasks.DrainAsync(TestContext.Current.CancellationToken);
        releaseParent.SetResult();
        await parent;

        drain.IsCompleted.Should().BeFalse();
        releaseChild.SetResult();
        await drain;
    }

    [Fact]
    public async Task DrainAsync_WaitsForOtherWorkAfterFailure()
    {
        var logger = CreateLogger();
        BackgroundTasks tasks = new(logger);
        TaskCompletionSource releaseFailure = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseSibling = new(TaskCreationOptions.RunContinuationsAsynchronously);
        InvalidOperationException failure = new("intentional failure");
        var work = tasks.Run(async () =>
        {
            await releaseFailure.Task;
            throw failure;
        }, "failure");
        var sibling = tasks.Queue(() => releaseSibling.Task, "sibling");

        var drain = tasks.DrainAsync(TestContext.Current.CancellationToken);
        releaseFailure.SetResult();
        await new Func<Task>(() => work).Should().ThrowAsync<InvalidOperationException>().WithMessage(failure.Message);

        drain.IsCompleted.Should().BeFalse();
        releaseSibling.SetResult();
        await drain;
        await sibling;
        A.CallTo(logger).Where(call => call.Method.Name == nameof(ILogger.Log)
            && call.Arguments.Get<LogLevel>(0) == LogLevel.Error
            && call.Arguments.Get<Exception>(3) == failure
            && call.Arguments.Get<object>(2)!.ToString()!.Contains("failure", StringComparison.Ordinal))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Run_LogsSynchronousFailureBeforeDrain()
    {
        var logger = CreateLogger();
        BackgroundTasks tasks = new(logger);
        InvalidOperationException failure = new("synchronous failure");

        var work = tasks.Run(() => throw failure, "synchronous");

        await new Func<Task>(() => work).Should().ThrowAsync<InvalidOperationException>().WithMessage(failure.Message);
        await tasks.DrainAsync(TestContext.Current.CancellationToken);
        A.CallTo(logger).Where(call => call.Method.Name == nameof(ILogger.Log)
            && call.Arguments.Get<LogLevel>(0) == LogLevel.Error
            && call.Arguments.Get<Exception>(3) == failure)
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DrainAsync_CancellationDoesNotCancelWorkOrPreventLaterDrain()
    {
        var logger = CreateLogger();
        BackgroundTasks tasks = new(logger);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenSource deadline = new();
        var work = tasks.Queue(() => release.Task, "unfinished");
        var drain = tasks.DrainAsync(deadline.Token);

        await deadline.CancelAsync();

        await new Func<Task>(() => drain).Should().ThrowAsync<OperationCanceledException>();
        work.IsCompleted.Should().BeFalse();
        release.SetResult();
        await work;
        await tasks.DrainAsync(TestContext.Current.CancellationToken);
        A.CallTo(logger).Where(call => call.Method.Name == nameof(ILogger.Log)
            && call.Arguments.Get<LogLevel>(0) == LogLevel.Warning
            && call.Arguments.Get<object>(2)!.ToString()!.Contains("unfinished", StringComparison.Ordinal))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Run_PreservesTaskCancellation()
    {
        BackgroundTasks tasks = new(NullLogger<BackgroundTasks>.Instance);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        var work = tasks.Run(() => Task.FromCanceled(cancellation.Token), "cancelled");

        work.IsCanceled.Should().BeTrue();
        await new Func<Task>(() => work).Should().ThrowAsync<OperationCanceledException>();
        await tasks.DrainAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task FiniteSingletonTask_IsTrackedAndExecutedOnce()
    {
        BackgroundTasks tasks = new(NullLogger<BackgroundTasks>.Instance);
        SingletonTaskRunner runner = new(NullLogger<SingletonTaskRunner>.Instance, new(NullLogger<TaskExceptionLogger>.Instance), tasks);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = runner.RunFiniteTaskIfNotRan(() => release.Task, "once");
        var second = runner.RunFiniteTaskIfNotRan(() => throw new InvalidOperationException("ran twice"), "once");

        var drain = tasks.DrainAsync(TestContext.Current.CancellationToken);

        second.Should().BeSameAs(first);
        drain.IsCompleted.Should().BeFalse();
        release.SetResult();
        await drain;
        await first;
    }

    private static ILogger<BackgroundTasks> CreateLogger()
    {
        var logger = A.Fake<ILogger<BackgroundTasks>>();
        A.CallTo(() => logger.IsEnabled(A<LogLevel>._)).Returns(true);
        return logger;
    }
}
