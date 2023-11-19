﻿using System.Text;
using Bell.Actions;
using Bell.Data;
using Bell.Languages;
using Bell.Utils;

namespace Bell;

public partial class TextBox
{
    internal readonly IBackend Backend;

    internal readonly ActionManager ActionManager = new();
    internal readonly CaretManager CaretManager = new();
    internal readonly FontManager FontManager = new();
    internal readonly LineManager LineManager = new();
    internal readonly RowManager RowManager = new();
    internal readonly Logger Logger = new ();
    internal readonly CacheCounter CacheCounter = new();
    
    internal readonly ObjectPool<LineSub> LineSubPool = new();
    internal readonly ObjectPool<Row> RowPool = new();
    
    private readonly StringBuilder _sb = new();

    public int Id;
    private static int _idCounter = 0;

    private static readonly ThreadLocal<TextBox> ThreadLocalTextBox = new();
    internal static TextBox Ins
    {
        get => ThreadLocalTextBox.Value ?? throw new Exception("No TextBox set");
        set => ThreadLocalTextBox.Value = value;
    }
    
    public TextBox(IBackend backend)
    {
        Id = Interlocked.Increment(ref _idCounter);

        Backend = backend;
        _tabStringCache = new Cache<string>("TabString", string.Empty, UpdateTabString);
        Text = "";
    }
    
    public string Text
    {
        get
        {
            Ins = this;
        
            _sb.Clear();
            foreach (Line line in LineManager.Lines)
            {
                _sb.Append(line.String);
                _sb.Append(TextBox.Ins.GetEolString());
            }
            return _sb.ToString();
        }
        set
        {
            string text = value;
            
            Ins = this;
        
            text = Ins.ReplaceTab(text);
            text = Ins.ReplaceEol(text);

            LineManager.Lines.Clear();
            int i = 0;
            foreach (string lineText in text.Split('\n'))
            {
                Line line = new Line(i++);
                LineManager.Lines.Add(line);
            
                line.InsertChars(0, lineText.ToCharArray());
            }
            CaretManager.ClearCarets();
            RowManager.SetRowCacheDirty();
        }
    }
    

    public string GetDebugString()
    {
        Ins = this;
        
        var sb = new StringBuilder();
        sb.AppendLine(CaretManager.GetDebugString());
        sb.AppendLine(CacheCounter.GetDebugString());
        sb.AppendLine(ActionManager.GetDebugString());
        return sb.ToString();
    }

    public List<string> GetLogs()
    {
        return Logger.GetLogs().Select(i => $"[{i.Item1}] {i.Item3}: ({i.Item2})").ToList();
    }
}