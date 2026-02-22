using System;
using System.Collections.Generic;
using System.Text;

namespace I8080Emulator.CPM;

public class TextEditor
{
    private readonly I8080Emulator.Core.Memory _memory;
    private readonly List<string> _lines = new();
    private int _currentLine = 0;
    private string _filename = "";
    
    public event Action<string>? OnOutput;
    public event Func<string>? OnInputLine;
    
    public TextEditor(I8080Emulator.Core.Memory memory)
    {
        _memory = memory;
    }
    
    public void Run(string filename)
    {
        _filename = filename;
        _lines.Clear();
        _currentLine = 0;
        
        PrintLine($"ED - CP/M Text Editor");
        PrintLine($"Editing: {filename}");
        PrintLine("Commands: I=Insert, D=Delete, L=List, S=Save, Q=Quit");
        PrintLine("");
        
        while (true)
        {
            Print("*");
            var input = ReadLine()?.Trim().ToUpperInvariant();
            
            if (string.IsNullOrEmpty(input)) continue;
            
            var cmd = input[0];
            var arg = input.Length > 1 ? input[1..].Trim() : "";
            
            switch (cmd)
            {
                case 'I':
                    InsertMode();
                    break;
                case 'D':
                    DeleteLine(arg);
                    break;
                case 'L':
                    ListLines(arg);
                    break;
                case 'S':
                    Save();
                    break;
                case 'Q':
                    return;
                case 'H':
                    PrintHelp();
                    break;
                default:
                    PrintLine($"Unknown command: {cmd}");
                    break;
            }
        }
    }
    
    private void InsertMode()
    {
        PrintLine("Enter text (empty line to exit insert mode):");
        while (true)
        {
            Print($"{_currentLine + 1,4}: ");
            var line = ReadLine();
            
            if (string.IsNullOrEmpty(line))
                break;
            
            _lines.Insert(_currentLine, line);
            _currentLine++;
        }
    }
    
    private void DeleteLine(string arg)
    {
        if (int.TryParse(arg, out var lineNum) && lineNum > 0 && lineNum <= _lines.Count)
        {
            _lines.RemoveAt(lineNum - 1);
            PrintLine($"Deleted line {lineNum}");
        }
        else
        {
            PrintLine("Invalid line number");
        }
    }
    
    private void ListLines(string arg)
    {
        int start = 0, end = _lines.Count;
        
        if (!string.IsNullOrEmpty(arg))
        {
            var parts = arg.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out var s) && int.TryParse(parts[1], out var e))
            {
                start = Math.Max(0, s - 1);
                end = Math.Min(_lines.Count, e);
            }
        }
        
        for (int i = start; i < end; i++)
            PrintLine($"{i + 1,4}: {_lines[i]}");
    }
    
    private void Save()
    {
        var content = string.Join("\r\n", _lines);
        var data = Encoding.ASCII.GetBytes(content);
        
        var addr = 0x0100;
        _memory.Load((ushort)addr, data);
        
        PrintLine($"Saved {_lines.Count} lines to {_filename}");
    }
    
    private void PrintHelp()
    {
        PrintLine("ED Commands:");
        PrintLine("  I        - Insert mode");
        PrintLine("  D n      - Delete line n");
        PrintLine("  L        - List all lines");
        PrintLine("  L n-m    - List lines n to m");
        PrintLine("  S        - Save file");
        PrintLine("  Q        - Quit editor");
    }
    
    private void Print(string text) => OnOutput?.Invoke(text);
    private void PrintLine(string text) => OnOutput?.Invoke(text + "\r\n");
    private string? ReadLine() => OnInputLine?.Invoke();
}
