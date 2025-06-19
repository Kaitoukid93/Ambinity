using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

public class LogStore : ILogEventSink
{
    private readonly object _lock = new();

    private readonly LinkedList<LogEvent> LinkedList = new();

    /// <summary>
    ///     Gets a list containing the last 500 log events.
    /// </summary>
    public List<LogEvent> Events
    {
        get
        {
            List<LogEvent> events;

            lock (_lock)
                events = LinkedList.ToList();

            return events;
        }
    }

    /// <summary>
    ///     Occurs when a new <see cref="LogEvent" /> was received.
    /// </summary>
    public  event EventHandler<LogEventEventArgs>? EventAdded;

    public void Emit(LogEvent logEvent)
    {
        lock (_lock)
        {
            LinkedList.AddLast(logEvent);
            while (LinkedList.Count > 500)
                LinkedList.RemoveFirst();
        }


        OnEventAdded(new LogEventEventArgs(logEvent));
    }

    private void OnEventAdded(LogEventEventArgs e)
    {
        EventAdded?.Invoke(null, e);
    }
}

/// <summary>
///     Contains log event related data
/// </summary>
public class LogEventEventArgs : EventArgs
{
    internal LogEventEventArgs(LogEvent logEvent)
    {
        LogEvent = logEvent;
    }

    /// <summary>
    ///     Gets the log event
    /// </summary>
    public LogEvent LogEvent { get; }
}
