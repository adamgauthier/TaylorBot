﻿using System.Collections.Immutable;

namespace TaylorBot.Net.Core.Events;

public class AsyncEvent<T> where T : class
{
    private readonly Lock _subLock = new();
    private ImmutableArray<T> _subscriptions;

    public bool HasSubscribers => _subscriptions.Length != 0;
    public IReadOnlyList<T> Subscriptions => _subscriptions;

    public AsyncEvent()
    {
        _subscriptions = [];
    }

    public void Add(T subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        lock (_subLock)
            _subscriptions = _subscriptions.Add(subscriber);
    }

    public void Remove(T subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        lock (_subLock)
            _subscriptions = _subscriptions.Remove(subscriber);
    }
}

public static class EventExtensions
{
    public static async Task InvokeAsync(this AsyncEvent<Func<Task>> eventHandler)
    {
        var subscribers = eventHandler.Subscriptions;
        for (int i = 0; i < subscribers.Count; i++)
            await subscribers[i].Invoke().ConfigureAwait(false);
    }

    public static async Task InvokeAsync<T>(this AsyncEvent<Func<T, Task>> eventHandler, T arg)
    {
        var subscribers = eventHandler.Subscriptions;
        for (int i = 0; i < subscribers.Count; i++)
            await subscribers[i].Invoke(arg).ConfigureAwait(false);
    }

    public static async Task InvokeAsync<T1, T2>(this AsyncEvent<Func<T1, T2, Task>> eventHandler, T1 arg1, T2 arg2)
    {
        var subscribers = eventHandler.Subscriptions;
        for (int i = 0; i < subscribers.Count; i++)
            await subscribers[i].Invoke(arg1, arg2).ConfigureAwait(false);
    }

    public static async Task InvokeAsync<T1, T2, T3>(this AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3)
    {
        var subscribers = eventHandler.Subscriptions;
        for (int i = 0; i < subscribers.Count; i++)
            await subscribers[i].Invoke(arg1, arg2, arg3).ConfigureAwait(false);
    }

    public static async Task InvokeAsync<T1, T2, T3, T4>(this AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        var subscribers = eventHandler.Subscriptions;
        for (int i = 0; i < subscribers.Count; i++)
            await subscribers[i].Invoke(arg1, arg2, arg3, arg4).ConfigureAwait(false);
    }

    public static async Task InvokeAsync<T1, T2, T3, T4, T5>(this AsyncEvent<Func<T1, T2, T3, T4, T5, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        var subscribers = eventHandler.Subscriptions;
        for (int i = 0; i < subscribers.Count; i++)
            await subscribers[i].Invoke(arg1, arg2, arg3, arg4, arg5).ConfigureAwait(false);
    }
}
