using System.Diagnostics;

namespace TaylorBot.Net.Core.Program.Instrumentation;

public sealed class TaylorBotInstrumentation(string activitySourceName) : IDisposable
{
    private bool disposed;

    public ActivitySource ActivitySource { get; } = new ActivitySource(activitySourceName, "1.0.0");

    public void Dispose()
    {
        if (!disposed)
        {
            ActivitySource.Dispose();
            disposed = true;
        }
    }
}
