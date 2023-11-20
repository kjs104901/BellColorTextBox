using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Bell.Utils;

public class Logger
{
    private const int HistoryCapacity = 1000;

    public enum Level
    {
        Debug,
        Warning,
        Error,
    }

    private readonly Queue<ValueTuple<Level, string, string>> _logs = new();

    [Conditional("DEBUG")]
    public static void Debug(string message, [CallerMemberName] string callerMemberName = "")
    {
        TextBox.Ins.Logger.AddLog(Level.Debug, callerMemberName, message);
    }

    [Conditional("DEBUG")]
    public static void Warning(string message, [CallerMemberName] string callerMemberName = "")
    {
        TextBox.Ins.Logger.AddLog(Level.Warning, callerMemberName, message);
    }

    [Conditional("DEBUG")]
    public static void Error(string message, [CallerMemberName] string callerMemberName = "")
    {
        TextBox.Ins.Logger.AddLog(Level.Error, callerMemberName, message);
        if (TextBox.Ins.IsDebugMode)
        {
            Debugger.Break();
        }
    }
    
    private void AddLog(Level level, string message, string callerMemberName)
    {
        _logs.Enqueue(new(level, callerMemberName, message));
        if (_logs.Count > HistoryCapacity)
        {
            _logs.Dequeue();
        }
    }

    public List<ValueTuple<Level, string, string>> GetLogs()
    {
        var list = _logs.ToList();
        list.Reverse();
        return list;
    }
}