using System;
using System.IO;
using System.Linq;
using System.Text;
using Ambinity.ViewModels;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Ambinity.Views.Debug;

public class DebugWindowViewModel : ViewModelBase
{
    private readonly LogStore _logStore;
    private readonly MessageTemplateTextFormatter _formatter;
    public InlineCollection Lines { get; } = new InlineCollection();

    private const int MAX_ENTRIES = 1000;

    public DebugWindowViewModel(LogStore logStore)
    {
        _logStore = logStore;
        _formatter = new MessageTemplateTextFormatter(
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        );


        //only subscribe to event when windows is open
    }
    public void Init()
    {
        foreach (LogEvent logEvent in _logStore.Events)
            AddLogEvent(logEvent);
        _logStore.EventAdded += OnLogEventAdded;
    }
    public void CloseWindow()
    {
        _logStore.EventAdded -= OnLogEventAdded;
        Lines?.Clear();
    }
    private void OnLogEventAdded(object? sender, LogEventEventArgs e)
    {
        Dispatcher.UIThread.Post(() => { AddLogEvent(e.LogEvent); });
    }

    private void AddLogEvent(LogEvent? logEvent)
    {
        if (logEvent is null)
            return;

        using StringWriter writer = new();
        _formatter.Format(logEvent, writer);
        string line = writer.ToString();

        Lines.Add(new Run(line.TrimEnd('\r', '\n') + '\n')
        {
            Foreground = logEvent.Level switch
            {
                LogEventLevel.Verbose => new SolidColorBrush(Colors.White),
                LogEventLevel.Debug => new SolidColorBrush(Color.FromRgb(216, 216, 216)),
                LogEventLevel.Information => new SolidColorBrush(Color.FromRgb(93, 201, 255)),
                LogEventLevel.Warning => new SolidColorBrush(Color.FromRgb(255, 177, 53)),
                LogEventLevel.Error => new SolidColorBrush(Color.FromRgb(255, 63, 63)),
                LogEventLevel.Fatal => new SolidColorBrush(Colors.Red),
                _ => throw new ArgumentOutOfRangeException()
            }
        });
        LimitLines();
    }

    private void LimitLines()
    {
        if (Lines.Count > MAX_ENTRIES)
            Lines.RemoveRange(0, Lines.Count - MAX_ENTRIES);
    }
}


