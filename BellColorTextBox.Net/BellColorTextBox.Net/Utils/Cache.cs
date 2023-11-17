﻿using System.Diagnostics;

namespace Bell.Utils;

internal class Cache<T>
{
    private readonly string _name;
    private T _value;
    
    private readonly Func<T, T> _updateFunc;
    private bool _isDirty;

    private DateTime _updateTime;
    private readonly int _updateIntervalMs;
    
    private static readonly Stopwatch UpdateStopwatch = new();

    internal Cache(string name, T initValue, Func<T, T> updateFunc, int updateIntervalMs = 0)
    {
        _name = name;
        _value = initValue;
        
        _updateFunc = updateFunc;
        _isDirty = true;

        _updateTime = DateTime.Now;;
        _updateIntervalMs = updateIntervalMs;
    }

    internal T Get()
    {
        if (_updateIntervalMs > 0)
        {
            if (_isDirty && DateTime.Now > _updateTime)
                Update();
        }
        else
        {
            if (_isDirty)
                Update();
        }
        
        if (Singleton.TextBox.IsDebugMode)
            Singleton.TextBox.CacheCounter.CountGet(_name);
        
        return _value;
    }

    internal void SetDirty()
    {
        if (Singleton.TextBox.IsDebugMode)
            Singleton.TextBox.CacheCounter.CountSetDirty(_name);
        
        _isDirty = true;

        if (_updateIntervalMs > 0)
            _updateTime = DateTime.Now.AddMilliseconds(_updateIntervalMs);
    }

    private void Update()
    {
        if (Singleton.TextBox.IsDebugMode)
        {
            Singleton.TextBox.CacheCounter.CountUpdate(_name);
            UpdateStopwatch.Restart();
        }
        
        _value = _updateFunc(_value);
        _isDirty = false;
        
        if (Singleton.TextBox.IsDebugMode)
        {
            UpdateStopwatch.Stop();
            Singleton.TextBox.CacheCounter.AddUpdateTime(_name, UpdateStopwatch.ElapsedMilliseconds);
        }
    }
}