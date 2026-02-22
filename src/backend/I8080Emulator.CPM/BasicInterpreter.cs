using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using I8080Emulator.Core;

namespace I8080Emulator.CPM;

public class BasicInterpreter
{
    private readonly Memory _memory;
    private readonly Dictionary<int, string> _program = new();
    private readonly Dictionary<string, object> _variables = new();
    private readonly Stack<int> _gosubStack = new();
    private readonly Stack<int> _forStack = new();
    private readonly Dictionary<string, string> _forVars = new();
    
    private int _currentLine = 0;
    private bool _running = false;
    
    public event Action<string>? OnOutput;
    public event Func<string>? OnInputLine;

    public BasicInterpreter(Memory memory)
    {
        _memory = memory;
    }
    
    public void Run()
    {
        PrintLine("BASIC Interpreter v1.0");
        PrintLine("Type LIST to list program, RUN to execute");
        PrintLine("");
        
        while (true)
        {
            Print("] ");
            var input = ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(input)) continue;
            
            if (int.TryParse(input.Split(' ')[0], out _))
            {
                StoreLine(input);
            }
            else
            {
                ExecuteDirect(input);
            }
        }
    }
    
    private void StoreLine(string input)
    {
        var spaceIdx = input.IndexOf(' ');
        var lineNum = int.Parse(input.Substring(0, spaceIdx > 0 ? spaceIdx : input.Length));
        var code = spaceIdx > 0 ? input.Substring(spaceIdx + 1) : "";
        
        if (string.IsNullOrWhiteSpace(code))
            _program.Remove(lineNum);
        else
            _program[lineNum] = code;
    }
    
    private void ExecuteDirect(string input)
    {
        var parts = input.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToUpperInvariant();
        var args = parts.Length > 1 ? parts[1] : "";
        
        switch (cmd)
        {
            case "NEW":
                _program.Clear();
                PrintLine("Program cleared");
                break;
            case "LIST":
                ListProgram(args);
                break;
            case "RUN":
                RunProgram();
                break;
            case "CLR":
                _variables.Clear();
                PrintLine("Variables cleared");
                break;
            case "HELP":
                PrintHelp();
                break;
            case "QUIT" or "EXIT":
                return;
            default:
                PrintLine($"?Syntax error");
                break;
        }
    }
    
    private void ListProgram(string args)
    {
        foreach (var line in _program.OrderBy(x => x.Key))
            PrintLine($"{line.Key} {line.Value}");
    }
    
    private void RunProgram()
    {
        _running = true;
        _currentLine = 0;
        _variables.Clear();
        _gosubStack.Clear();
        _forStack.Clear();
        
        var lines = _program.OrderBy(x => x.Key).ToList();
        if (lines.Count == 0)
        {
            PrintLine("No program to run");
            return;
        }
        
        var lineIndex = 0;
        
        while (_running && lineIndex < lines.Count)
        {
            var entry = lines[lineIndex];
            _currentLine = entry.Key;
            
            var result = ExecuteStatement(entry.Value);
            
            switch (result)
            {
                case ExecutionResult.Next:
                    lineIndex++;
                    break;
                case ExecutionResult.Goto when int.TryParse(_variables.TryGetValue("__GOTO__", out var target) ? target.ToString() : "", out var gotoLine):
                    lineIndex = lines.FindIndex(x => x.Key >= gotoLine);
                    break;
                case ExecutionResult.Stop:
                    _running = false;
                    break;
            }
        }
        
        PrintLine("");
    }
    
    private enum ExecutionResult { Next, Goto, Stop }
    
    private ExecutionResult ExecuteStatement(string code)
    {
        var parts = code.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return ExecutionResult.Next;
        
        var cmd = parts[0].ToUpperInvariant();
        var args = parts.Length > 1 ? parts[1] : "";
        
        switch (cmd)
        {
            case "PRINT" or "?":
                PrintStatement(args);
                break;
            case "LET":
                LetStatement(args);
                break;
            case "INPUT":
                InputStatement(args);
                break;
            case "IF":
                return IfStatement(args);
            case "GOTO":
                return GotoStatement(args);
            case "GOSUB":
                return GosubStatement(args);
            case "RETURN":
                return ReturnStatement();
            case "FOR":
                ForStatement(args);
                break;
            case "NEXT":
                return NextStatement(args);
            case "END" or "STOP":
                return ExecutionResult.Stop;
            case "REM":
                break;
            default:
                if (args.StartsWith("="))
                    LetStatement(cmd + args);
                else
                    PrintLine($"?Syntax error at line {_currentLine}");
                break;
        }
        
        return ExecutionResult.Next;
    }
    
    private void PrintStatement(string args)
    {
        var output = new StringBuilder();
        var i = 0;
        
        while (i < args.Length)
        {
            if (args[i] == '"')
            {
                var end = args.IndexOf('"', i + 1);
                if (end > i)
                {
                    output.Append(args.Substring(i + 1, end - i - 1));
                    i = end + 1;
                }
            }
            else if (char.IsLetter(args[i]) || args[i] == '_' || args[i] == '$')
            {
                var start = i;
                while (i < args.Length && (char.IsLetterOrDigit(args[i]) || args[i] == '_' || args[i] == '$' || args[i] == '%'))
                    i++;
                var varName = args.Substring(start, i - start);
                var val = Evaluate(varName);
                output.Append(val.ToString(CultureInfo.InvariantCulture));
            }
            else if (char.IsDigit(args[i]) || args[i] == '-' || args[i] == '.')
            {
                var start = i;
                while (i < args.Length && (char.IsDigit(args[i]) || args[i] == '.' || args[i] == 'E' || args[i] == 'e' || args[i] == '+' || args[i] == '-'))
                    i++;
                output.Append(args.Substring(start, i - start));
            }
            else
            {
                i++;
            }
        }
        
        Print(output.ToString());
        if (!args.EndsWith(";"))
            PrintLine("");
    }
    
    private void LetStatement(string args)
    {
        var eqIdx = args.IndexOf('=');
        if (eqIdx < 0) return;
        
        var varName = args.Substring(0, eqIdx).Trim();
        var expr = args.Substring(eqIdx + 1).Trim();
        
        _variables[varName.ToUpperInvariant()] = Evaluate(expr);
    }
    
    private void InputStatement(string args)
    {
        var parts = args.Split(new[] { ';' }, 2);
        
        if (parts.Length > 1 && parts[0].StartsWith("\"") && parts[0].EndsWith("\""))
        {
            Print(parts[0].Substring(1, parts[0].Length - 2));
            parts = new[] { parts[1] };
        }
        
        var varName = parts[0].Trim();
        var input = ReadLine();
        
        if (double.TryParse(input, out var val))
            _variables[varName.ToUpperInvariant()] = val;
        else
            _variables[varName.ToUpperInvariant() + "$"] = input ?? "";
    }
    
    private ExecutionResult IfStatement(string args)
    {
        var thenIdx = args.ToUpperInvariant().IndexOf("THEN");
        if (thenIdx < 0) return ExecutionResult.Next;
        
        var condition = args.Substring(0, thenIdx).Trim();
        var thenPart = args.Substring(thenIdx + 4).Trim();
        
        if (EvaluateCondition(condition))
        {
            if (int.TryParse(thenPart, out var lineNum))
            {
                _variables["__GOTO__"] = lineNum;
                return ExecutionResult.Goto;
            }
            return ExecuteStatement(thenPart);
        }
        
        return ExecutionResult.Next;
    }
    
    private ExecutionResult GotoStatement(string args)
    {
        if (int.TryParse(args, out var lineNum))
        {
            _variables["__GOTO__"] = lineNum;
            return ExecutionResult.Goto;
        }
        return ExecutionResult.Next;
    }
    
    private ExecutionResult GosubStatement(string args)
    {
        if (int.TryParse(args, out var lineNum))
        {
            _gosubStack.Push(_currentLine);
            _variables["__GOTO__"] = lineNum;
            return ExecutionResult.Goto;
        }
        return ExecutionResult.Next;
    }
    
    private ExecutionResult ReturnStatement()
    {
        if (_gosubStack.Count > 0)
        {
            var returnLine = _gosubStack.Pop();
            _variables["__GOTO__"] = returnLine;
            return ExecutionResult.Goto;
        }
        return ExecutionResult.Next;
    }
    
    private void ForStatement(string args)
    {
        var match = System.Text.RegularExpressions.Regex.Match(args, @"(\w+)\s*=\s*(.+)\s+TO\s+(.+?)(?:\s+STEP\s+(.+))?$");
        if (!match.Success) return;
        
        var varName = match.Groups[1].Value.ToUpperInvariant();
        var start = Evaluate(match.Groups[2].Value);
        var end = Evaluate(match.Groups[3].Value);
        var step = match.Groups[4].Success ? Evaluate(match.Groups[4].Value) : 1;
        
        _variables[varName] = start;
        _forVars[varName] = $"{end}|{step}|{_currentLine}";
    }
    
    private ExecutionResult NextStatement(string args)
    {
        var varName = args.Trim().ToUpperInvariant();
        if (!_forVars.TryGetValue(varName, out var info)) return ExecutionResult.Next;
        
        var parts = info.Split('|');
        var end = double.Parse(parts[0]);
        var step = double.Parse(parts[1]);
        var forLine = int.Parse(parts[2]);
        
        _variables[varName] = Convert.ToDouble(_variables[varName]) + step;
        
        if ((step > 0 && Convert.ToDouble(_variables[varName]) <= end) || (step < 0 && Convert.ToDouble(_variables[varName]) >= end))
        {
            _variables["__GOTO__"] = forLine + 1;
            return ExecutionResult.Goto;
        }
        
        return ExecutionResult.Next;
    }
    
    private bool EvaluateCondition(string condition)
    {
        var ops = new[] { "<=", ">=", "<>", "<", ">", "=" };
        foreach (var op in ops)
        {
            var idx = condition.IndexOf(op);
            if (idx > 0)
            {
                var left = Evaluate(condition.Substring(0, idx));
                var right = Evaluate(condition.Substring(idx + op.Length));
                return op switch
                {
                    "<=" => left <= right,
                    ">=" => left >= right,
                    "<>" => Math.Abs(left - right) > 0.0001,
                    "<" => left < right,
                    ">" => left > right,
                    "=" => Math.Abs(left - right) < 0.0001,
                    _ => false
                };
            }
        }
        return Evaluate(condition) != 0;
    }
    
    private double Evaluate(string expr)
    {
        expr = expr.Trim();
        
        if (double.TryParse(expr, out var num))
            return num;
        
        var varName = expr.ToUpperInvariant();
        if (_variables.TryGetValue(varName, out var val))
            return Convert.ToDouble(val);
        
        var plusIdx = FindOperator(expr, '+');
        var minusIdx = FindOperator(expr, '-');
        
        if (plusIdx > 0)
            return Evaluate(expr.Substring(0, plusIdx)) + Evaluate(expr.Substring(plusIdx + 1));
        if (minusIdx > 0)
            return Evaluate(expr.Substring(0, minusIdx)) - Evaluate(expr.Substring(minusIdx + 1));
        
        var mulIdx = FindOperator(expr, '*');
        var divIdx = FindOperator(expr, '/');
        
        if (mulIdx > 0)
            return Evaluate(expr.Substring(0, mulIdx)) * Evaluate(expr.Substring(mulIdx + 1));
        if (divIdx > 0)
            return Evaluate(expr.Substring(0, divIdx)) / Evaluate(expr.Substring(divIdx + 1));
        
        return 0;
    }
    
    private int FindOperator(string expr, char op)
    {
        var parenCount = 0;
        for (int i = expr.Length - 1; i >= 0; i--)
        {
            if (expr[i] == ')') parenCount++;
            else if (expr[i] == '(') parenCount--;
            else if (parenCount == 0 && expr[i] == op)
                return i;
        }
        return -1;
    }
    
    private void PrintHelp()
    {
        PrintLine("Commands:");
        PrintLine("  NEW         - Clear program");
        PrintLine("  LIST        - List program");
        PrintLine("  RUN         - Run program");
        PrintLine("  CLR         - Clear variables");
        PrintLine("  QUIT        - Exit BASIC");
        PrintLine("");
        PrintLine("Statements:");
        PrintLine("  PRINT       - Print value");
        PrintLine("  LET         - Assign variable");
        PrintLine("  INPUT       - Get input");
        PrintLine("  IF...THEN   - Conditional");
        PrintLine("  GOTO        - Jump to line");
        PrintLine("  GOSUB/RETURN- Subroutine");
        PrintLine("  FOR...NEXT  - Loop");
        PrintLine("  END/STOP    - End program");
    }
    
    private void Print(string text) => OnOutput?.Invoke(text);
    private void PrintLine(string text) => OnOutput?.Invoke(text + "\r\n");
    private string? ReadLine() => OnInputLine?.Invoke();
}
