using System.Numerics;
using System.Runtime.InteropServices;
using Bell.Languages;
using Bell.Utils;
using ImGuiNET;

namespace Bell;

public class ImGuiTextBox
{
    private readonly TextBox _textBox = new(new ImGuiBackend());
    
    // Options
    public bool ReadOnly { get => _textBox.ReadOnly; set => _textBox.ReadOnly = value; }
    public bool AutoIndent { get => _textBox.AutoIndent; set => _textBox.AutoIndent = value; }
    public WrapMode WrapMode { get => _textBox.WrapMode; set => _textBox.WrapMode = value; }
    public bool WordWrapIndent { get => _textBox.WordWrapIndent; set => _textBox.WordWrapIndent = value; }
    public EolMode EolMode { get => _textBox.EolMode; set => _textBox.EolMode = value; }
    public bool SyntaxHighlight { get => _textBox.SyntaxHighlight; set => _textBox.SyntaxHighlight = value; }
    public bool SyntaxFolding { get => _textBox.SyntaxFolding; set => _textBox.SyntaxFolding = value; }
    public bool ShowingWhitespace { get => _textBox.ShowingWhitespace; set => _textBox.ShowingWhitespace = value; }
    public float LeadingHeight { get => _textBox.LeadingHeight; set => _textBox.LeadingHeight = value; }
    public TabMode TabMode { get => _textBox.TabMode; set => _textBox.TabMode = value; }
    public int TabSize { get => _textBox.TabSize; set => _textBox.TabSize = value; }
    public Language Language { get => _textBox.Language; set => _textBox.Language = value; }
    public string Text { get => _textBox.Text; set => _textBox.Text = value; }
    public bool IsDebugMode { get => _textBox.IsDebugMode; set => _textBox.IsDebugMode = value; }

    public static bool IsFontAwesomeLoaded;
    public static ImFontPtr FontAwesome;
    public static void LoadFontAwesome()
    {
        float fontSize = 12.0f;
        GCHandle glyphHandle = GCHandle.Alloc(new ushort[] { 0xe005, 0xf8ff, 0x0000 }, GCHandleType.Pinned);
        GCHandle fontHandle = GCHandle.Alloc(Fonts.fa_solid_900, GCHandleType.Pinned);

        try
        {
            FontAwesome = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(fontHandle.AddrOfPinnedObject(),
                Fonts.fa_solid_900.Length,
                fontSize, null, glyphHandle.AddrOfPinnedObject());
            ImGui.GetIO().Fonts.Build();
            IsFontAwesomeLoaded = true;
        }
        finally
        {
            glyphHandle.Free();
            fontHandle.Free();
        }
    }

    public void Render()
    {
        Render(new Vector2(-1, -1));
    }

    public void Render(Vector2 size)
    {
        if (_textBox.IsDebugMode)
        {
            if (ImGui.BeginTable($"##ImGuiTextBoxDebugTable_{_textBox.Id}", 3, ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn($"##ImGuiTextBoxDebugTable_Column1_{_textBox.Id}", ImGuiTableColumnFlags.None, 100);
                ImGui.TableSetupColumn($"##ImGuiTextBoxDebugTable_Column2_{_textBox.Id}", ImGuiTableColumnFlags.None, 100);
                ImGui.TableSetupColumn($"##ImGuiTextBoxDebugTable_Column3_{_textBox.Id}", ImGuiTableColumnFlags.None, 200);

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                string debugString = _textBox.GetDebugString();
                ImGui.InputTextMultiline($"##ImGuiTextBoxDebug_{_textBox.Id}", ref debugString, (uint)debugString.Length, new Vector2(-1, -1));
                
                ImGui.TableNextColumn();
                string logString = string.Join("\n", _textBox.GetLogs());
                ImGui.InputTextMultiline($"##ImGuiTextBoxLogs_{_textBox.Id}", ref logString, (uint)logString.Length, new Vector2(-1, -1));
                
                ImGui.TableNextColumn();
                RenderTextBox(size);
                ImGui.EndTable();
            }
        }
        else
        {
            RenderTextBox(size);
        }
    }

    private string _childId = string.Empty;
    private string _windowId = string.Empty;

    private void RenderTextBox(Vector2 size)
    {
        if (_childId == string.Empty)
            _childId = $"##TextBox_{_textBox.Id}";
        if (_windowId == string.Empty)
            _windowId = $"##TextBoxWindow_{_textBox.Id}";

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(0, 0));
        
        ImGui.PushStyleColor(ImGuiCol.ChildBg, _textBox.Theme.Background.ToVector());
        ImGui.BeginChild(_childId, size, true, ImGuiWindowFlags.HorizontalScrollbar);
        Vector2 contentSize = ImGui.GetWindowContentRegionMax();

        ImGui.SetNextWindowSize(new Vector2(contentSize.X, contentSize.Y));
        ImGui.Begin(_windowId,
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs |
            ImGuiWindowFlags.ChildWindow);

        Vector2 viewPos = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
        Vector2 viewSize = new Vector2(contentSize.X - ImGui.GetStyle().ScrollbarSize, contentSize.Y);

        _textBox.Render(viewPos, viewSize);

        ImGui.End();
        ImGui.EndChild();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(5);
    }
}