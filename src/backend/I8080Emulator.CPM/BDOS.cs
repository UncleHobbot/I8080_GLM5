using System;
using System.Linq;
using System.Text;
using I8080Emulator.Core;
using I8080Emulator.Core.CPU;

namespace I8080Emulator.CPM;

public class BDOS
{
    private readonly Memory _memory;
    private readonly DiskDrive[] _drives;
    private readonly StringBuilder _output = new();
    
    private int _currentDrive = 0;
    private ushort _dmaAddress = 0x0080;
    private byte _userNumber = 0;
    
    public event Action<char>? OnCharOutput;
    public event Func<char>? OnCharInput;
    public event Action<string>? OnStringOutput;
    
    public string OutputBuffer => _output.ToString();
    
    public BDOS(Memory memory, DiskDrive[] drives)
    {
        _memory = memory;
        _drives = drives;
    }
    
    public byte Call(ushort address)
    {
        var function = _memory.Read(address);
        return Execute(function);
    }
    
    public byte Execute(byte function)
    {
        var cpu = new Intel8080(_memory);
        return (byte)ExecuteFunction(function, cpu.BC, cpu.DE, cpu.HL);
    }
    
    public ushort ExecuteFunction(int function, ushort bc, ushort de, ushort hl)
    {
        return function switch
        {
            0 => SystemReset(),
            1 => ConsoleInput(),
            2 => ConsoleOutput((byte)bc),
            9 => PrintString(de),
            10 => ReadConsoleBuffer(de),
            11 => GetConsoleStatus(),
            12 => ReturnVersion(),
            13 => ResetDiskSystem(),
            14 => SelectDisk((byte)bc),
            15 => OpenFile(de),
            16 => CloseFile(de),
            17 => SearchFirst(de),
            18 => SearchNext(de),
            19 => DeleteFile(de),
            20 => ReadSequential(de),
            21 => WriteSequential(de),
            22 => MakeFile(de),
            23 => RenameFile(de),
            25 => GetCurrentDisk(),
            26 => SetDMA(bc),
            32 => GetUserNumber(),
            33 => SetUserNumber((byte)bc),
            _ => 0xFFFF
        };
    }
    
    private ushort SystemReset()
    {
        _currentDrive = 0;
        _dmaAddress = 0x0080;
        return 0;
    }
    
    private ushort ConsoleInput()
    {
        var c = OnCharInput?.Invoke() ?? (char)0;
        return c;
    }
    
    private ushort ConsoleOutput(byte ch)
    {
        _output.Append((char)ch);
        OnCharOutput?.Invoke((char)ch);
        return 0;
    }
    
    private ushort PrintString(ushort address)
    {
        var sb = new StringBuilder();
        while (true)
        {
            var ch = (char)_memory.Read(address++);
            if (ch == '$') break;
            sb.Append(ch);
        }
        var str = sb.ToString();
        _output.Append(str);
        OnStringOutput?.Invoke(str);
        return 0;
    }
    
    private ushort ReadConsoleBuffer(ushort address)
    {
        var maxLen = _memory.Read(address);
        var input = "";
        
        if (OnCharInput != null)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var c = OnCharInput();
                if (c == '\r' || c == '\n') break;
                if (sb.Length < maxLen)
                {
                    sb.Append(c);
                    OnCharOutput?.Invoke(c);
                }
            }
            OnCharOutput?.Invoke('\r');
            OnCharOutput?.Invoke('\n');
            input = sb.ToString();
        }
        
        _memory.Write((ushort)(address + 1), (byte)input.Length);
        for (int i = 0; i < input.Length; i++)
            _memory.Write((ushort)(address + 2 + i), (byte)input[i]);
        
        return 0;
    }
    
    private ushort GetConsoleStatus() => 0xFF;
    private ushort ReturnVersion() => 0x0022;
    private ushort ResetDiskSystem() { _currentDrive = 0; return 0; }
    private ushort SelectDisk(byte disk) { _currentDrive = disk; return 0; }
    private ushort GetCurrentDisk() => (ushort)_currentDrive;
    private ushort SetDMA(ushort address) { _dmaAddress = address; return 0; }
    private ushort GetUserNumber() => _userNumber;
    private ushort SetUserNumber(byte user) { _userNumber = (byte)(user & 0x1F); return 0; }
    
    private ushort OpenFile(ushort fcbAddress)
    {
        var drive = _memory.Read(fcbAddress);
        if (drive == 0) drive = (byte)(_currentDrive + 1);
        
        var fileName = ReadFCBName(fcbAddress);
        var file = _drives[drive - 1].GetFile(fileName);
        
        if (file == null) return 0xFFFF;
        
        _memory.Write((ushort)(fcbAddress + 0x0F), (byte)(file.Length / 128));
        return 0;
    }
    
    private ushort CloseFile(ushort fcbAddress) => 0;
    
    private ushort SearchFirst(ushort fcbAddress)
    {
        var pattern = ReadFCBName(fcbAddress);
        var files = _drives[_currentDrive].ListFiles();
        
        var match = files.FirstOrDefault(f => MatchesPattern(f, pattern));
        if (match == null) return 0xFFFF;
        
        WriteDirectoryEntry(match);
        return 0;
    }
    
    private ushort SearchNext(ushort fcbAddress) => 0xFFFF;
    
    private ushort DeleteFile(ushort fcbAddress)
    {
        var fileName = ReadFCBName(fcbAddress);
        _drives[_currentDrive].DeleteFile(fileName);
        return 0;
    }
    
    private ushort ReadSequential(ushort fcbAddress)
    {
        var fileName = ReadFCBName(fcbAddress);
        var file = _drives[_currentDrive].GetFile(fileName);
        if (file == null) return 0xFFFF;
        
        var record = _memory.Read((ushort)(fcbAddress + 0x20));
        var offset = record * 128;
        
        if (offset >= file.Length) return 1;
        
        for (int i = 0; i < 128 && offset + i < file.Length; i++)
            _memory.Write((ushort)(_dmaAddress + i), file[offset + i]);
        
        _memory.Write((ushort)(fcbAddress + 0x20), (byte)(record + 1));
        return 0;
    }
    
    private ushort WriteSequential(ushort fcbAddress) => 0;
    private ushort MakeFile(ushort fcbAddress) => 0;
    private ushort RenameFile(ushort fcbAddress) => 0xFFFF;
    
    private string ReadFCBName(ushort address)
    {
        var name = new char[8];
        var ext = new char[3];
        
        for (int i = 0; i < 8; i++)
            name[i] = _memory.Read((ushort)(address + 1 + i)) == ' ' ? '\0' : (char)_memory.Read((ushort)(address + 1 + i));
        for (int i = 0; i < 3; i++)
            ext[i] = _memory.Read((ushort)(address + 9 + i)) == ' ' ? '\0' : (char)_memory.Read((ushort)(address + 9 + i));
        
        var n = new string(name).TrimEnd('\0');
        var e = new string(ext).TrimEnd('\0');
        
        return string.IsNullOrEmpty(e) ? n : $"{n}.{e}";
    }
    
    private void WriteDirectoryEntry(string fileName)
    {
        var parts = fileName.Split('.');
        var name = parts[0].PadRight(8).Substring(0, 8);
        var ext = parts.Length > 1 ? parts[1].PadRight(3).Substring(0, 3) : "   ";
        
        for (int i = 0; i < 8; i++)
            _memory.Write((ushort)(_dmaAddress + i), (byte)name[i]);
        for (int i = 0; i < 3; i++)
            _memory.Write((ushort)(_dmaAddress + 8 + i), (byte)ext[i]);
    }
    
    private bool MatchesPattern(string fileName, string pattern)
    {
        if (pattern.Contains('?') || pattern.Contains('*'))
        {
            var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\?", ".")
                .Replace("\\*", ".*") + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(fileName, regex, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
