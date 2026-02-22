using System;
using System.Collections.Generic;
using System.Text;
using I8080Emulator.Core;

namespace I8080Emulator.CPM;

public class Assembler
{
    private readonly Memory _memory;
    private readonly Dictionary<string, byte> _opcodes = new();
    private readonly Dictionary<string, ushort> _labels = new();
    private readonly List<byte> _output = new();
    private ushort _address = 0x0100;
    
    public event Action<string>? OnOutput;
    public event Action<byte[]>? OnCodeGenerated;

    public Assembler(Memory memory)
    {
        _memory = memory;
        InitializeOpcodes();
    }
    
    private void InitializeOpcodes()
    {
        _opcodes["NOP"] = 0x00;
        _opcodes["LXI"] = 0x01;
        _opcodes["STAX"] = 0x02;
        _opcodes["INX"] = 0x03;
        _opcodes["INR"] = 0x04;
        _opcodes["DCR"] = 0x05;
        _opcodes["MVI"] = 0x06;
        _opcodes["RLC"] = 0x07;
        _opcodes["DAD"] = 0x09;
        _opcodes["LDAX"] = 0x0A;
        _opcodes["DCX"] = 0x0B;
        _opcodes["RRC"] = 0x0F;
        _opcodes["RAL"] = 0x17;
        _opcodes["RAR"] = 0x1F;
        _opcodes["SHLD"] = 0x22;
        _opcodes["DAA"] = 0x27;
        _opcodes["LHLD"] = 0x2A;
        _opcodes["CMA"] = 0x2F;
        _opcodes["STA"] = 0x32;
        _opcodes["STC"] = 0x37;
        _opcodes["LDA"] = 0x3A;
        _opcodes["CMC"] = 0x3F;
        _opcodes["MOV"] = 0x40;
        _opcodes["HLT"] = 0x76;
        _opcodes["ADD"] = 0x80;
        _opcodes["ADC"] = 0x88;
        _opcodes["SUB"] = 0x90;
        _opcodes["SBB"] = 0x98;
        _opcodes["ANA"] = 0xA0;
        _opcodes["XRA"] = 0xA8;
        _opcodes["ORA"] = 0xB0;
        _opcodes["CMP"] = 0xB8;
        _opcodes["RNZ"] = 0xC0;
        _opcodes["POP"] = 0xC1;
        _opcodes["JNZ"] = 0xC2;
        _opcodes["JMP"] = 0xC3;
        _opcodes["CNZ"] = 0xC4;
        _opcodes["PUSH"] = 0xC5;
        _opcodes["ADI"] = 0xC6;
        _opcodes["RST0"] = 0xC7;
        _opcodes["RZ"] = 0xC8;
        _opcodes["RET"] = 0xC9;
        _opcodes["JZ"] = 0xCA;
        _opcodes["CZ"] = 0xCC;
        _opcodes["CALL"] = 0xCD;
        _opcodes["ACI"] = 0xCE;
        _opcodes["RST1"] = 0xCF;
        _opcodes["RNC"] = 0xD0;
        _opcodes["JNC"] = 0xD2;
        _opcodes["OUT"] = 0xD3;
        _opcodes["CNC"] = 0xD4;
        _opcodes["SUI"] = 0xD6;
        _opcodes["RST2"] = 0xD7;
        _opcodes["RC"] = 0xD8;
        _opcodes["JC"] = 0xDA;
        _opcodes["IN"] = 0xDB;
        _opcodes["CC"] = 0xDC;
        _opcodes["SBI"] = 0xDE;
        _opcodes["RST3"] = 0xDF;
        _opcodes["RPO"] = 0xE0;
        _opcodes["JPO"] = 0xE2;
        _opcodes["XTHL"] = 0xE3;
        _opcodes["CPO"] = 0xE4;
        _opcodes["ANI"] = 0xE6;
        _opcodes["RST4"] = 0xE7;
        _opcodes["RPE"] = 0xE8;
        _opcodes["PCHL"] = 0xE9;
        _opcodes["JPE"] = 0xEA;
        _opcodes["XCHG"] = 0xEB;
        _opcodes["CPE"] = 0xEC;
        _opcodes["XRI"] = 0xEE;
        _opcodes["RST5"] = 0xEF;
        _opcodes["RP"] = 0xF0;
        _opcodes["JP"] = 0xF2;
        _opcodes["DI"] = 0xF3;
        _opcodes["CP"] = 0xF4;
        _opcodes["ORI"] = 0xF6;
        _opcodes["RST6"] = 0xF7;
        _opcodes["RM"] = 0xF8;
        _opcodes["SPHL"] = 0xF9;
        _opcodes["JM"] = 0xFA;
        _opcodes["EI"] = 0xFB;
        _opcodes["CM"] = 0xFC;
        _opcodes["CPI"] = 0xFE;
        _opcodes["RST7"] = 0xFF;
    }
    
    public byte[] Assemble(string source)
    {
        _output.Clear();
        _labels.Clear();
        _address = 0x0100;
        
        var lines = source.Split('\n');
        var errors = new List<string>();
        
        FirstPass(lines, errors);
        _address = 0x0100;
        SecondPass(lines, errors);
        
        if (errors.Count > 0)
        {
            PrintLine("Assembly errors:");
            foreach (var error in errors)
                PrintLine($"  {error}");
            return Array.Empty<byte>();
        }
        
        PrintLine($"Assembly successful: {_output.Count} bytes");
        OnCodeGenerated?.Invoke(_output.ToArray());
        return _output.ToArray();
    }
    
    private void FirstPass(string[] lines, List<string> errors)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";")) continue;
            
            var colonIdx = line.IndexOf(':');
            if (colonIdx > 0)
            {
                var label = line.Substring(0, colonIdx).Trim();
                _labels[label.ToUpperInvariant()] = _address;
                line = line.Substring(colonIdx + 1).Trim();
            }
            
            if (string.IsNullOrEmpty(line)) continue;
            
            var parts = ParseLine(line);
            if (parts.Length == 0) continue;
            
            var opcode = parts[0].ToUpperInvariant();
            if (opcode == "ORG" && parts.Length > 1 && ushort.TryParse(parts[1], out var org))
            {
                _address = org;
                continue;
            }
            
            _address += (ushort)GetInstructionSize(opcode, parts);
        }
    }
    
    private void SecondPass(string[] lines, List<string> errors)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";")) continue;
            
            var colonIdx = line.IndexOf(':');
            if (colonIdx > 0)
                line = line.Substring(colonIdx + 1).Trim();
            
            if (string.IsNullOrEmpty(line)) continue;
            
            var parts = ParseLine(line);
            if (parts.Length == 0) continue;
            
            var opcode = parts[0].ToUpperInvariant();
            if (opcode == "ORG") continue;
            
            AssembleInstruction(opcode, parts, errors, i + 1);
        }
    }
    
    private string[] ParseLine(string line)
    {
        var commentIdx = line.IndexOf(';');
        if (commentIdx >= 0) line = line.Substring(0, commentIdx);
        
        line = line.Replace(",", " ");
        return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }
    
    private int GetInstructionSize(string opcode, string[] parts)
    {
        if (opcode == "ORG") return 0;
        if (!_opcodes.ContainsKey(opcode)) return 0;
        
        return opcode switch
        {
            "LXI" or "LHLD" or "SHLD" or "LDA" or "STA" or "JMP" or "CALL" or "JNZ" or "JZ" or "JNC" or "JC" or "JPO" or "JPE" or "JP" or "JM" or "CNZ" or "CZ" or "CNC" or "CC" or "CPO" or "CPE" or "CP" or "CM" => 3,
            "MVI" or "ADI" or "ACI" or "SUI" or "SBI" or "ANI" or "XRI" or "ORI" or "CPI" or "IN" or "OUT" => 2,
            "MOV" => 1,
            _ => 1
        };
    }
    
    private void AssembleInstruction(string opcode, string[] parts, List<string> errors, int lineNum)
    {
        if (!_opcodes.TryGetValue(opcode, out var baseOpcode))
        {
            errors.Add($"Line {lineNum}: Unknown opcode '{opcode}'");
            return;
        }
        
        switch (opcode)
        {
            case "NOP" or "HLT" or "RET" or "RLC" or "RRC" or "RAL" or "RAR" or "DAA" or "CMA" or "STC" or "CMC" or "XCHG" or "XTHL" or "SPHL" or "PCHL" or "DI" or "EI":
                _output.Add(baseOpcode);
                break;
            
            case "MVI":
                if (parts.Length >= 3)
                {
                    var reg = GetRegisterCode(parts[1]);
                    var val = ParseValue(parts[2]);
                    _output.Add((byte)(0x06 | (reg << 3)));
                    _output.Add((byte)val);
                }
                break;
            
            case "LXI":
                if (parts.Length >= 3)
                {
                    var rp = GetRegisterPair(parts[1]);
                    var val = ParseValue16(parts[2]);
                    _output.Add((byte)(0x01 | (rp << 4)));
                    _output.Add((byte)(val & 0xFF));
                    _output.Add((byte)(val >> 8));
                }
                break;
            
            case "MOV":
                if (parts.Length >= 3)
                {
                    var dst = GetRegisterCode(parts[1]);
                    var src = GetRegisterCode(parts[2]);
                    _output.Add((byte)(0x40 | (dst << 3) | src));
                }
                break;
            
            case "ADD" or "ADC" or "SUB" or "SBB" or "ANA" or "XRA" or "ORA" or "CMP":
                if (parts.Length >= 2)
                {
                    var reg = GetRegisterCode(parts[1]);
                    _output.Add((byte)(baseOpcode | reg));
                }
                break;
            
            case "INR" or "DCR":
                if (parts.Length >= 2)
                {
                    var reg = GetRegisterCode(parts[1]);
                    _output.Add((byte)(baseOpcode | (reg << 3)));
                }
                break;
            
            case "INX" or "DCX" or "DAD":
                if (parts.Length >= 2)
                {
                    var rp = GetRegisterPair(parts[1]);
                    _output.Add((byte)(baseOpcode | (rp << 4)));
                }
                break;
            
            case "JMP" or "CALL" or "CNZ" or "CZ" or "CNC" or "CC" or "CPO" or "CPE" or "CP" or "CM" or "JNZ" or "JZ" or "JNC" or "JC" or "JPO" or "JPE" or "JP" or "JM":
                if (parts.Length >= 2)
                {
                    var addr = ResolveLabel(parts[1]);
                    _output.Add(baseOpcode);
                    _output.Add((byte)(addr & 0xFF));
                    _output.Add((byte)(addr >> 8));
                }
                break;
            
            case "ADI" or "ACI" or "SUI" or "SBI" or "ANI" or "XRI" or "ORI" or "CPI":
                if (parts.Length >= 2)
                {
                    var val = ParseValue(parts[1]);
                    _output.Add(baseOpcode);
                    _output.Add((byte)val);
                }
                break;
            
            case "IN" or "OUT":
                if (parts.Length >= 2)
                {
                    var port = ParseValue(parts[1]);
                    _output.Add(baseOpcode);
                    _output.Add((byte)port);
                }
                break;
            
            case "PUSH" or "POP":
                if (parts.Length >= 2)
                {
                    var rp = GetRegisterPair(parts[1]);
                    if (parts[1].ToUpperInvariant() == "PSW") rp = 3;
                    _output.Add((byte)(baseOpcode | (rp << 4)));
                }
                break;
            
            default:
                _output.Add(baseOpcode);
                break;
        }
    }
    
    private int GetRegisterCode(string reg)
    {
        return reg.ToUpperInvariant() switch
        {
            "B" => 0, "C" => 1, "D" => 2, "E" => 3, "H" => 4, "L" => 5, "M" => 6, "A" => 7,
            _ => 0
        };
    }
    
    private int GetRegisterPair(string rp)
    {
        return rp.ToUpperInvariant() switch
        {
            "BC" or "B" => 0, "DE" or "D" => 1, "HL" or "H" => 2, "SP" or "PSW" => 3,
            _ => 0
        };
    }
    
    private ushort ParseValue(string val)
    {
        val = val.Trim();
        if (val.StartsWith("0x") || val.StartsWith("0X"))
            return ushort.Parse(val[2..], System.Globalization.NumberStyles.HexNumber);
        if (val.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            return ushort.Parse(val[..^1], System.Globalization.NumberStyles.HexNumber);
        return ushort.Parse(val);
    }
    
    private ushort ParseValue16(string val)
    {
        val = val.Trim();
        if (val.StartsWith("0x") || val.StartsWith("0X"))
            return ushort.Parse(val[2..], System.Globalization.NumberStyles.HexNumber);
        if (val.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            return ushort.Parse(val[..^1], System.Globalization.NumberStyles.HexNumber);
        return ushort.Parse(val);
    }
    
    private ushort ResolveLabel(string label)
    {
        if (_labels.TryGetValue(label.ToUpperInvariant(), out var addr))
            return addr;
        return ParseValue16(label);
    }
    
    private void PrintLine(string text) => OnOutput?.Invoke(text + "\r\n");
}
