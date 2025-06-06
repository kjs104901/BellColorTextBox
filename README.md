# BellColorTextBox

*A lightweight, extensible text-editor control with syntax highlighting and multi-caret support.*

BellColorTextBox is a pure-C# library that brings fast, code-friendly editing to any .NET 6+ application.  
It ships with an **ImGui** back-end out of the box, and you can plug in other renderers (e.g., WinForms, WPF, SDL) by implementing a small `IBackend` interface.

<p align="center">
  <img src="https://raw.githubusercontent.com/kjs104901/BellColorTextBox/main/Documents/screenshot.PNG" alt="BellColorTextBox screenshot" width="700">
</p>

---

## NuGet Packages

| Package | Latest Version |
|---------|----------------|
| **BellColorTextBox.Net** | [![NuGet](https://img.shields.io/nuget/v/BellColorTextBox.Net?logo=nuget)](https://www.nuget.org/packages/BellColorTextBox.Net) |
| **BellColorTextBox.ImGuiNet** | [![NuGet](https://img.shields.io/nuget/v/BellColorTextBox.ImGuiNet?logo=nuget)](https://www.nuget.org/packages/BellColorTextBox.ImGuiNet) |

---

## Requirements

| Project | Framework / Library |
|---------|---------------------|
| **BellColorTextBox.Net** | .NET 6.0 or later |
| **BellColorTextBox.ImGuiNet** | BellColorTextBox.Net, ImGui.NET (≥ 1.89.7.1) |

---

## Key Features

- **Syntax Highlighting** — token-based or regex-based, fully themeable  
- **Multiple Carets & Selections** — `Ctrl+Click` to add cursors, each with independent undo/redo  
- **Code Folding** — hide regions without breaking selections or clipboard ops  
- **Auto-Indent & Tab Modes** — spaces/tabs, overwrite mode, smart EOL handling  
- **Word Wrap & Read-Only Mode**  
- **Custom Languages** — implement `ILanguage` to add your own highlighter

---

## Roadmap / To-Do

- [ ] Complete documentation site  
- [ ] Find / Replace / Go to  
- [ ] Auto-completion & intellisense hooks  
- [ ] Smart auto-scroll on typing  
- [ ] Paste handling for multiple carets  
- [ ] More built-in language grammars

---

## Core vs. Native Port

| Module | Status |
|--------|--------|
| **BellColorTextBox.Net** (C#) | ✅ Feature-complete |
| **BellColorTextBox** (C++) | ⏳ Planned |

### Built-in Language Definitions

- C#
- JSON
- Protocol Buffers
- SQL

---

## Rendering Back-Ends

| Platform | ImGuiNet (C#) | WinForms (C#) | ImGui (C++) |
|----------|---------------|---------------|-------------|
| **Windows** | ✅ | ⏳ | ⏳ |
| **Linux**   | ⏳ | ❌ | ⏳ |
| **macOS**   | ⏳ | ❌ | ⏳ |

Legend: ✅ available ⏳ planned ❌ not planned

---

## Getting Started

```bash
dotnet add package BellColorTextBox.ImGuiNet
