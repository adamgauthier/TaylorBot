using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.IntegrationTests.Shared.Hosting;

public sealed class ScenarioLogs : ILoggerProvider
{
    private readonly Lock _lock = new();
    private readonly Queue<string> _messages = [];
    private readonly List<string> _errors = [];
    private readonly List<ExpectedError> _expected = [];

    public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);
    public void Dispose() { }

    public void ExpectError<T>(string message, int count = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        lock (_lock)
        {
            _expected.Add(new(typeof(T).FullName!, message, count));
        }
    }

    public void EnsureExpectationsMet()
    {
        EnsureNoErrors();

        lock (_lock)
        {
            var unused = _expected.Where(expected => expected.Remaining != 0).ToArray();
            if (unused.Length != 0)
            {
                throw new InvalidOperationException($"Expected application errors did not occur:\n{string.Join('\n', unused.Select(expected => $"{expected.Category}: {expected.Message} ({expected.Remaining} remaining)"))}");
            }
        }
    }

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
                    var expected = owner._expected.FirstOrDefault(expected => expected.Remaining > 0 &&
                        expected.Category == category && formatter(state, exception).Contains(expected.Message, StringComparison.Ordinal));
                    if (expected != null)
                    {
                        expected.Remaining--;
                    }
                    else
                    {
                        owner._errors.Add(message);
                    }
                }
            }
        }
    }

    private sealed class ExpectedError(string category, string message, int remaining)
    {
        public string Category { get; } = category;
        public string Message { get; } = message;
        public int Remaining { get; set; } = remaining;
    }
}
