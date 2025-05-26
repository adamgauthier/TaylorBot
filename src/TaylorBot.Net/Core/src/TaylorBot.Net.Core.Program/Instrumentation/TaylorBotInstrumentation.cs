using System.Diagnostics;

namespace TaylorBot.Net.Core.Program.Instrumentation;

public sealed class TaylorBotInstrumentation : IDisposable
{
    private bool disposed;

    public const string ActivitySourceName = $"{nameof(TaylorBot)}.{nameof(Net)}.{nameof(Core)}.{nameof(Program)}.{nameof(Instrumentation)}.{nameof(TaylorBotInstrumentation)}";

    public ActivitySource ActivitySource { get; } = new(ActivitySourceName, "1.0.0");

    public void Dispose()
    {
        if (!disposed)
        {
            ActivitySource.Dispose();
            disposed = true;
        }
    }
}
