# BellColorTextBox

The bell text box is an abstracted text box library that supports simple syntax highlighting.

It allows the use of a desired rendering backend, and the directly supported backends will continue to be added

![screenshot](https://github.com/kjs104901/BellColorTextBox/blob/main/Documents/screenshot.PNG)


## Dependency
- BellColorTextBox.Net
  - .Net 6.0

- BellColorTextBox.ImGuiNet
  - BellColorTextBox.Net
  - ImGui.Net (1.89.7.1)


## Features 
- Syntax highlighting 
- Multiple carets
- AutoIndent
- ReadOnly
- WordWrap
- Eol
- Folding
- TabMode
- Custom Language
- Theme

## To Do
- [ ] Documentation
- [ ] Fin, Replace, Goto
- [ ] Overwrite mode
- [ ] Auto completion
- [ ] Singleline mode
- [ ] Auto scroll
- [ ] Multiple caret paste
- [ ] Add languages

## Core
|BellColorTextBox.Net (C#)|BellColorTextBox (C++)|
|---|---|
|O|To Do|

### Languages
- C#
- Json
- Protobuf
- SQL

## Backends
||BellColorTextBox.ImGuiNet (C#)|BellColorTextBox.WinForm (C#)|BellColorTextBox.WPF (C#)|BellColorTextBox.ImGui (C++)|
|---|---|---|---|---|
|Windows|O|To Do|To Do|To Do|
|Linux|To Do|X|X|To Do|
|MacOS|To Do|X|X|To Do|