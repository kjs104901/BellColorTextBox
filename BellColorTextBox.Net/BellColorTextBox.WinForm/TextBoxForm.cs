using System.Numerics;
using System.Windows.Forms;
using System.Drawing;
using Timer = System.Windows.Forms.Timer;
using Bell.Languages;
using System.Runtime.InteropServices;

namespace Bell;

public class TextBoxControl : UserControl
{
    private TextBox _textBox;
    private WinFormBackend _backend;
    private Timer _timer;

    internal Graphics? Graphics;

    // Options
    public bool ReadOnly { get => _textBox.ReadOnly; set => _textBox.ReadOnly = value; }
    public bool AutoIndent { get => _textBox.AutoIndent; set => _textBox.AutoIndent = value; }
    public WrapMode WrapMode { get => _textBox.WrapMode; set => _textBox.WrapMode = value; }
    public bool WordWrapIndent { get => _textBox.WordWrapIndent; set => _textBox.WordWrapIndent = value; }
    public EolMode EolMode { get => _textBox.EolMode; set => _textBox.EolMode = value; }
    public bool SyntaxHighlight { get => _textBox.SyntaxHighlight; set => _textBox.SyntaxHighlight = value; }
    public bool SyntaxFolding { get => _textBox.SyntaxFolding; set => _textBox.SyntaxFolding = value; }
    public bool ShowingSpace { get => _textBox.ShowingSpace; set => _textBox.ShowingSpace = value; }
    public bool ShowingTab { get => _textBox.ShowingTab; set => _textBox.ShowingTab = value; }
    public float LeadingHeight { get => _textBox.LeadingHeight; set => _textBox.LeadingHeight = value; }
    public TabMode TabMode { get => _textBox.TabMode; set => _textBox.TabMode = value; }
    public int TabSize { get => _textBox.TabSize; set => _textBox.TabSize = value; }
    public Language Language { get => _textBox.Language; set => _textBox.Language = value; }
    override public string Text { get => _textBox.Text; set => _textBox.Text = value; }
    public bool IsDebugMode { get => _textBox.IsDebugMode; set => _textBox.IsDebugMode = value; }


    public TextBoxControl()
    {
        _backend = new WinFormBackend(this);
        _textBox = new TextBox(_backend);

        DoubleBuffered = true;
        AutoScroll = true;
        ImeMode = ImeMode.On;

        _timer = new Timer();
        _timer.Interval = 100;
        _timer.Tick += TimerTick;
        _timer.Start();

        ChangeIME();
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        Invalidate();
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        Console.WriteLine(e.KeyChar);

        base.OnKeyPress(e);
        _backend.OnKeyPress(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        Console.WriteLine(e.KeyValue);

        base.OnKeyDown(e);
    }


    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        _backend.ProcessCmdKey(ref msg, keyData);
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics = e.Graphics;

        AutoScrollMinSize = new Size((int)_backend.PageSize.X - SystemInformation.VerticalScrollBarWidth, (int)_backend.PageSize.Y);
        _textBox.Render(new Vector2(0, 0), new Vector2(Width, Height));
    }
    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetContext(IntPtr hWnd);
    [DllImport("imm32.dll")]
    public static extern Boolean ImmSetConversionStatus(IntPtr hIMC, Int32 fdwConversion, Int32 fdwSentence);

    public const int IME_CMODE_LANGUAGE = 0x1; // Example value
    public const int IME_CMODE_ALPHANUMERIC = 0x0; // Example value

    private void ChangeIME()
    {
        IntPtr context = ImmGetContext(Handle);
        ImmSetConversionStatus(context, IME_CMODE_LANGUAGE, 0);
    }
}