using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Bell;
using Bell.Languages;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

namespace BellColorTextBox.Demo.Net;

class ImGuiDemo
{
    private static bool _readOnly = false;
    
    private static bool _autoIndent = true;
    
    private static Option<WrapMode> _wrapMode = new()
    {
        Options =  new() { WrapMode.None, WrapMode.Word, WrapMode.BreakWord },
        Names = new[] { "None", "Word", "BreakWord" },
        Index = 0
    };
    
    private static bool _wordWrapIndent = true;
    
    private static Option<EolMode> _eolMode = new()
    {
        Options =  new() {  EolMode.CRLF, EolMode.LF, EolMode.CR },
        Names = new[] { "CRLF", "LF", "CR" },
        Index = 1
    };
    
    private static bool _syntaxHighlight = true;
    private static bool _syntaxFolding = true;
    
    private static bool _showingSpace = true;
    private static bool _showingTab = true;

    private static float _leadingHeight = 1.2f;
    
    private static Option<TabMode> _tabMode = new()
    {
        Options =  new() { TabMode.Space, TabMode.Tab },
        Names = new[] { "Space", "Tab" },
        Index = 1
    };
    
    private static int _tabSize = 4;
    
    private static Option<Language> _language = new()
    {
        Options =  new() { Language.CSharp(), Language.Json(), Language.Proto(), Language.Sql(), Language.PlainText() },
        Names = new[] { "C#", "Json", "Proto", "Sql", "PlainText" },
        Index = 0
    };
    
    private static Option<byte[]> _font = new()
    {
        Options =  new() { Fonts.D2Coding, Fonts.MaruBuri, Fonts.NanumGothic },
        Names = new []{ "D2Coding", "MaruBuri", "NanumGothic" },
        Index = 0
    };

    private static bool _isDebugMode = false;


    public static Thread ThreadStart()
    {
        var thread = new Thread(ThreadMain)
        {
            Name = "ImGui"
        };
        thread.Start();
        return thread;
    }

    private static void ThreadMain()
    {
        Vector3 clearColor = new(0.45f, 0.55f, 0.6f);

        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImGui.NET Sample Program"),
            new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
            out var sdl2Window,
            out var graphicsDevice);

        var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
        var imGuiRenderer = new ImGuiRenderer(graphicsDevice,
            graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, sdl2Window.Width,
            sdl2Window.Height);

        sdl2Window.Resized += () =>
        {
            graphicsDevice.MainSwapchain.Resize((uint)sdl2Window.Width, (uint)sdl2Window.Height);
            imGuiRenderer.WindowResized(sdl2Window.Width, sdl2Window.Height);
        };

        ImFontPtr fontPtr;
        GCHandle fontHandle = GCHandle.Alloc(Fonts.D2Coding, GCHandleType.Pinned);
        try
        {
            fontPtr = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(fontHandle.AddrOfPinnedObject(),
                Fonts.D2Coding.Length, 16.0f, null, ImGui.GetIO().Fonts.GetGlyphRangesKorean());
        }
        finally
        {
            fontHandle.Free();
        }
        ImGuiTextBox.LoadFontAwesome();
        imGuiRenderer.RecreateFontDeviceTexture(graphicsDevice);

        var stopwatch = Stopwatch.StartNew();

        ImGuiTextBox imGuiBellTextBox = new();
        imGuiBellTextBox.Text = SourceCodeExample.CSharp;

        while (sdl2Window.Exists)
        {
            var deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            stopwatch.Restart();
            var snapshot = sdl2Window.PumpEvents();
            if (!sdl2Window.Exists)
                break;
            imGuiRenderer.Update(deltaTime, snapshot);

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(sdl2Window.Width, sdl2Window.Height));
            ImGui.Begin("Demo", ImGuiWindowFlags.NoResize);
            
            if (ImGui.BeginTable("##DemoTable", 2, ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("##DemoTable_Options", ImGuiTableColumnFlags.None, 30);
                ImGui.TableSetupColumn("##DemoTable_TextBox", ImGuiTableColumnFlags.None, 100);

                ImGui.TableNextRow();
                
                // Options
                ImGui.TableNextColumn();
                ImGui.Separator();
                if (ImGui.Checkbox("ReadOnly" , ref _readOnly))
                    imGuiBellTextBox.ReadOnly = _readOnly;
                
                ImGui.Separator();
                if (ImGui.Checkbox("AutoIndent" , ref _autoIndent))
                    imGuiBellTextBox.AutoIndent = _autoIndent;
                
                ImGui.Separator();
                if (ImGui.Combo("WrapMode", ref _wrapMode.Index, _wrapMode.Names, _wrapMode.Names.Length))
                    imGuiBellTextBox.WrapMode = _wrapMode.Options[_wrapMode.Index];
                if (ImGui.Checkbox("WordWrapIndent", ref _wordWrapIndent))
                    imGuiBellTextBox.WordWrapIndent = _wordWrapIndent;
                
                ImGui.Separator();
                if (ImGui.Combo("EolMode", ref _eolMode.Index, _eolMode.Names, _eolMode.Names.Length))
                    imGuiBellTextBox.EolMode = _eolMode.Options[_eolMode.Index];
                
                ImGui.Separator();
                if (ImGui.Checkbox("SyntaxHighlight", ref _syntaxHighlight))
                    imGuiBellTextBox.SyntaxHighlight = _syntaxHighlight;
                if (ImGui.Checkbox("SyntaxFolding", ref _syntaxFolding))
                    imGuiBellTextBox.SyntaxFolding = _syntaxFolding;
                if (ImGui.Checkbox("ShowingSpace", ref _showingSpace))
                    imGuiBellTextBox.ShowingSpace = _showingSpace;
                if (ImGui.Checkbox("ShowingTab", ref _showingTab))
                    imGuiBellTextBox.ShowingTab = _showingTab;
                
                if (ImGui.InputFloat("LeadingHeight", ref _leadingHeight, 0.01f, 0.05f, "%.2f"))
                    imGuiBellTextBox.LeadingHeight = _leadingHeight;
                
                if (ImGui.Combo("TabMode", ref _tabMode.Index, _tabMode.Names, _tabMode.Names.Length))
                    imGuiBellTextBox.TabMode = _tabMode.Options[_tabMode.Index];
                if (ImGui.InputInt("TabSize", ref _tabSize))
                    imGuiBellTextBox.TabSize = _tabSize;
                
                ImGui.Separator();
                if (ImGui.Combo("Language", ref _language.Index, _language.Names, _language.Names.Length))
                {
                    imGuiBellTextBox.Language =  _language.Options[_language.Index];
                    
                    string name = _language.Names[_language.Index];
                    if (name == "C#")
                        imGuiBellTextBox.Text = SourceCodeExample.CSharp;
                    else if (name == "Json")
                        imGuiBellTextBox.Text = SourceCodeExample.Json;
                    else if (name == "Proto")
                        imGuiBellTextBox.Text = SourceCodeExample.Proto;
                    else if (name == "Sql")
                        imGuiBellTextBox.Text = SourceCodeExample.Sql;
                    else
                        imGuiBellTextBox.Text = string.Empty;
                }

                ImGui.Separator();
                if (ImGui.Checkbox("IsDebugMode", ref _isDebugMode))
                    imGuiBellTextBox.IsDebugMode = _isDebugMode;

                // TextBox
                ImGui.TableNextColumn();

                ImGui.PushFont(fontPtr);
                imGuiBellTextBox.Render();
                ImGui.PopFont();
                
                ImGui.EndTable();
            }

            ImGui.End();

            commandList.Begin();
            commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);
            commandList.ClearColorTarget(0, new RgbaFloat(clearColor.X, clearColor.Y, clearColor.Z, 1f));
            imGuiRenderer.Render(graphicsDevice, commandList);
            commandList.End();
            graphicsDevice.SubmitCommands(commandList);
            graphicsDevice.SwapBuffers(graphicsDevice.MainSwapchain);
        }

        // Clean up Veldrid resources
        graphicsDevice.WaitForIdle();
        imGuiRenderer.Dispose();
        commandList.Dispose();
        graphicsDevice.Dispose();
    }
    
    private struct Option<T>
    {
        public int Index;
        public List<T> Options;
        public string[] Names;
    }
}