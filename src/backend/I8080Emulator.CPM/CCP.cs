using System;
using System.Linq;
using System.Text;

namespace I8080Emulator.CPM;

public class CCP
{
    private readonly BDOS _bdos;
    private readonly I8080Emulator.Core.Memory _memory;
    private readonly TextEditor _editor;
    private readonly Assembler _assembler;
    private readonly BasicInterpreter _basic;
    
    private string _commandBuffer = "";
    private int _currentDrive = 0;
    
    public event Action<string>? OnOutput;
    public event Func<string>? OnInputLine;
    
    public CCP(BDOS bdos, I8080Emulator.Core.Memory memory)
    {
        _bdos = bdos;
        _memory = memory;
        _editor = new TextEditor(memory);
        _assembler = new Assembler(memory);
        _basic = new BasicInterpreter(memory);
    }
    
    public void Start()
    {
        PrintBanner();
        RunCommandLoop();
    }
    
    private void PrintBanner()
    {
        PrintLine("");
        PrintLine("CP/M 2.2 Emulator - Intel 8080");
        PrintLine("Copyright (C) 2024");
        PrintLine("");
    }
    
    private void RunCommandLoop()
    {
        while (true)
        {
            Print($"{(char)('A' + _currentDrive)}>");
            var command = ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(command)) continue;
            
            var parts = ParseCommand(command);
            if (parts.Length == 0) continue;
            
            ExecuteCommand(parts[0].ToUpperInvariant(), parts.Skip(1).ToArray());
        }
    }
    
    private string[] ParseCommand(string command)
    {
        return command.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }
    
    private void ExecuteCommand(string cmd, string[] args)
    {
        switch (cmd)
        {
            case "DIR":
            case "LS":
                Dir(args);
                break;
            case "TYPE":
            case "CAT":
                Type(args);
                break;
            case "ERA":
            case "DEL":
            case "RM":
                Erase(args);
                break;
            case "REN":
            case "RENAME":
                Rename(args);
                break;
            case "SAVE":
                Save(args);
                break;
            case "ED":
            case "EDIT":
                Edit(args);
                break;
            case "ASM":
                Assemble(args);
                break;
            case "BASIC":
                RunBasic(args);
                break;
            case "HELP":
            case "?":
                Help();
                break;
            case "EXIT":
            case "QUIT":
                Environment.Exit(0);
                break;
            case "A:":
            case "B:":
            case "C:":
            case "D:":
                _currentDrive = cmd[0] - 'A';
                PrintLine("");
                break;
            default:
                if (!TryRunProgram(cmd, args))
                    PrintLine($"Unknown command: {cmd}");
                break;
        }
    }
    
    private void Dir(string[] args)
    {
        PrintLine("Directory of " + (char)('A' + _currentDrive) + ":");
        PrintLine("");
    }
    
    private void Type(string[] args)
    {
        if (args.Length == 0)
        {
            PrintLine("Usage: TYPE <filename>");
            return;
        }
        PrintLine($"File: {args[0]}");
    }
    
    private void Erase(string[] args)
    {
        if (args.Length == 0)
        {
            PrintLine("Usage: ERA <filename>");
            return;
        }
        PrintLine($"Deleted: {args[0]}");
    }
    
    private void Rename(string[] args)
    {
        if (args.Length < 2)
        {
            PrintLine("Usage: REN <newname>=<oldname>");
            return;
        }
        PrintLine("File renamed");
    }
    
    private void Save(string[] args)
    {
        PrintLine("File saved");
    }
    
    private void Edit(string[] args)
    {
        var filename = args.Length > 0 ? args[0] : "UNTITLED";
        PrintLine($"Editing: {filename}");
        PrintLine("Type SAVE to save, QUIT to exit");
        _editor.Run(filename);
    }
    
    private void Assemble(string[] args)
    {
        if (args.Length == 0)
        {
            PrintLine("Usage: ASM <sourcefile>");
            return;
        }
        PrintLine($"Assembling: {args[0]}");
        _assembler.Assemble(args[0]);
    }
    
    private void RunBasic(string[] args)
    {
        PrintLine("BASIC Interpreter");
        PrintLine("Type RUN to execute program");
        _basic.Run();
    }
    
    private void Help()
    {
        PrintLine("Available commands:");
        PrintLine("  DIR / LS     - List files");
        PrintLine("  TYPE / CAT   - Display file");
        PrintLine("  ERA / DEL    - Delete file");
        PrintLine("  REN          - Rename file");
        PrintLine("  SAVE         - Save file");
        PrintLine("  ED / EDIT    - Text editor");
        PrintLine("  ASM          - 8080 Assembler");
        PrintLine("  BASIC        - BASIC interpreter");
        PrintLine("  HELP / ?     - This help");
        PrintLine("  EXIT         - Exit emulator");
    }
    
    private bool TryRunProgram(string name, string[] args)
    {
        return false;
    }
    
    private void Print(string text) => OnOutput?.Invoke(text);
    private void PrintLine(string text) => OnOutput?.Invoke(text + "\r\n");
    private string? ReadLine() => OnInputLine?.Invoke();
}
