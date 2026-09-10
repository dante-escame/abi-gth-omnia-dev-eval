using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Integration.Outbox;

public sealed record RecordedLog(LogLevel Level, string Message);

public sealed class RecordingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<RecordedLog> _entries = new();

    public IEnumerable<RecordedLog> Entries => _entries;

    public void Clear() => _entries.Clear();

    public ILogger CreateLogger(string categoryName) => new RecordingLogger(_entries);

    public void Dispose()
    {
    }

    private sealed class RecordingLogger(ConcurrentQueue<RecordedLog> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Enqueue(new RecordedLog(logLevel, formatter(state, exception)));
        }
    }
}
