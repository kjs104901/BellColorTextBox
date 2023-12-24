using System.Numerics;
using System.Windows.Forms;
using System.Drawing;
using Timer = System.Windows.Forms.Timer;
using Bell.Languages;
using System.Runtime.InteropServices;
using System.Text;
using Bell.Inputs;

namespace Bell;

public class TextBoxControl : UserControl
{
    private TextBox _textBox;
    private WinFormBackend _backend;
    private Timer _timer;

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
    public override string Text { get => _textBox.Text; set => _textBox.Text = value; }
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
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        Invalidate();
    }

    protected override bool CanEnableIme => true;
    protected override ImeMode ImeModeBase => ImeMode.On;

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        _backend.OnKeyPress(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        bool result = base.ProcessCmdKey(ref msg, keyData);
        _backend.ProcessCmdKey(ref msg, keyData);
        return result;
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _backend.MouseInput.LeftAction = MouseAction.Click;
        else if (e.Button == MouseButtons.Middle)
            _backend.MouseInput.MiddleAction = MouseAction.Click;
        base.OnMouseClick(e);
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _backend.MouseInput.LeftAction = MouseAction.DoubleClick;
        else if (e.Button == MouseButtons.Middle)
            _backend.MouseInput.MiddleAction = MouseAction.DoubleClick;
        base.OnMouseDoubleClick(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        //if (e.Button == MouseButtons.Left)
        //    _backend.MouseInput.LeftAction = MouseAction.Dragging;
        //else if (e.Button == MouseButtons.Middle)
        //    _backend.MouseInput.MiddleAction = MouseAction.Dragging;
        //base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _backend.MouseInput.Position.X = e.X + HorizontalScroll.Value;
        _backend.MouseInput.Position.Y = e.Y + VerticalScroll.Value;
        base.OnMouseMove(e);
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        _backend.Graphics = e.Graphics;

        AutoScrollMinSize = new Size((int)_backend.PageSize.X - SystemInformation.VerticalScrollBarWidth, (int)_backend.PageSize.Y);

        _textBox.Tick();
        _textBox.Render(new Vector2(HorizontalScroll.Value, VerticalScroll.Value), new Vector2(Width, Height));
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_IME_COMPOSITION)
        {
            _backend.OnImeComposition(NativeMethods.GetCompositionString(Handle));
        }
        else if (m.Msg == NativeMethods.WM_IME_STARTCOMPOSITION)
        {
            NativeMethods.SetCompositionWindowOffScreen(Handle);
            return;
        }
        base.WndProc(ref m);
    }
}