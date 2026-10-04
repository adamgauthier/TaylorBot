using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;

internal sealed class ScenarioLogs : ILoggerProvider
{
    private readonly Lock _lock = new();
    private readonly Queue<string> _messages = [];
    private readonly List<string> _errors = [];

    public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);
    public void Dispose() { }

    public void EnsureNoErrors()
    {
        lock (_lock)
        {
            if (_errors.Count != 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, _errors));
            }
        }
    }

    public override string ToString()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine, _messages);
        }
    }

    private sealed class Logger(ScenarioLogs owner, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = $"{logLevel} {category}: {formatter(state, exception)} {exception}";
            lock (owner._lock)
            {
                owner._messages.Enqueue(message);
                if (owner._messages.Count > 200)
                {
                    owner._messages.Dequeue();
                }
                if (logLevel >= LogLevel.Error)
                {
                    owner._errors.Add(message);
                }
            }
        }
    }
}
