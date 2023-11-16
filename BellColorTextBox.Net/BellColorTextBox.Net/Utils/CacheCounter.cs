using System.Text;

namespace Bell.Utils;

internal class CacheCounter
{
    private class Status
    {
        internal long GetCount;
        internal long SetDirtyCount;
        internal long UpdateCount;
        internal long UpdateTimeMs;
    }
    
    private Dictionary<string, Status> _counter = new();
    
    internal void CountGet(string name)
    {
        if (false == _counter.ContainsKey(name))
            _counter.TryAdd(name, new Status());
        _counter[name].GetCount ++;
    }
    
    internal void CountSetDirty(string name)
    {
        if (false == _counter.ContainsKey(name))
            _counter.TryAdd(name, new Status());
        _counter[name].SetDirtyCount ++;
    }
    
    internal void CountUpdate(string name)
    {
        if (false == _counter.ContainsKey(name))
            _counter.TryAdd(name, new Status());
        _counter[name].UpdateCount ++;
    }
    
    internal void AddUpdateTime(string name, long ms)
    {
        if (false == _counter.ContainsKey(name))
            _counter.TryAdd(name, new Status());
        _counter[name].UpdateTimeMs += ms;
    }
    
    internal string GetDebugString()
    {
        var sb = new StringBuilder();
        foreach (var (name, status) in _counter)
        {
            sb.AppendLine($"{name}:\n\tGet {status.GetCount}, SetDirty {status.SetDirtyCount}\n\tUpdate {status.UpdateCount}, UpdateTime {status.UpdateTimeMs}ms");
        }
        return sb.ToString();
    }
}