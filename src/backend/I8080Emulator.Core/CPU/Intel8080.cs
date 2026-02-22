using System.Runtime.CompilerServices;

namespace I8080Emulator.Core.CPU;

[Flags]
public enum Flags : byte
{
    None = 0,
    Carry = 0x01,
    Parity = 0x04,
    AuxCarry = 0x10,
    Zero = 0x40,
    Sign = 0x80
}

public delegate byte PortInputHandler(byte port);
public delegate void PortOutputHandler(byte port, byte value);

public class Intel8080
{
    public byte A, B, C, D, E, H, L;
    public Flags Flags;
    public ushort SP, PC;
    public bool Halted { get; private set; }
    public bool InterruptsEnabled { get; private set; }
    
    public ushort BC { get => (ushort)((B << 8) | C); set { B = (byte)(value >> 8); C = (byte)(value & 0xFF); } }
    public ushort DE { get => (ushort)((D << 8) | E); set { D = (byte)(value >> 8); E = (byte)(value & 0xFF); } }
    public ushort HL { get => (ushort)((H << 8) | L); set { H = (byte)(value >> 8); L = (byte)(value & 0xFF); } }

    private readonly Memory _memory;
    private readonly Dictionary<byte, PortInputHandler> _inputPorts = new();
    private readonly Dictionary<byte, PortOutputHandler> _outputPorts = new();
    
    public event Action<byte>? OnOutput;
    public event Func<byte>? OnInput;
    public event Action? OnHalt;

    public Intel8080(Memory memory)
    {
        _memory = memory;
        Reset();
    }

    public void Reset()
    {
        A = B = C = D = E = H = L = 0;
        Flags = Flags.None;
        SP = PC = 0;
        Halted = false;
        InterruptsEnabled = false;
    }

    public void RegisterInputPort(byte port, PortInputHandler handler) => _inputPorts[port] = handler;
    public void RegisterOutputPort(byte port, PortOutputHandler handler) => _outputPorts[port] = handler;

    public int Step()
    {
        if (Halted) return 0;
        
        var opcode = FetchByte();
        Execute(opcode);
        return GetCycles(opcode);
    }

    private byte FetchByte() => _memory.Read(PC++);
    private ushort FetchWord() { var lo = FetchByte(); var hi = FetchByte(); return (ushort)((hi << 8) | lo); }
    private byte ReadMem(ushort addr) => _memory.Read(addr);
    private void WriteMem(ushort addr, byte val) => _memory.Write(addr, val);
    
    private void Push(ushort val) { SP -= 2; WriteMem((ushort)(SP + 1), (byte)(val >> 8)); WriteMem(SP, (byte)(val & 0xFF)); }
    private ushort Pop() { var lo = ReadMem(SP); var hi = ReadMem((ushort)(SP + 1)); SP += 2; return (ushort)((hi << 8) | lo); }
    
    private void PushPSW() { Push((ushort)((A << 8) | (byte)Flags | 0x02)); }
    private void PopPSW() { var psw = Pop(); A = (byte)(psw >> 8); Flags = (Flags)(psw & 0xFF) | Flags.AuxCarry; }

    private void SetFlags(byte val)
    {
        Flags = (Flags)(Flags & ~Flags.Zero & ~Flags.Sign);
        if (val == 0) Flags |= Flags.Zero;
        if ((val & 0x80) != 0) Flags |= Flags.Sign;
        SetParity(val);
    }

    private void SetParity(byte val)
    {
        var bits = 0;
        for (var i = 0; i < 8; i++) if ((val & (1 << i)) != 0) bits++;
        Flags = (Flags)((Flags & ~Flags.Parity) | (bits % 2 == 0 ? Flags.Parity : Flags.None));
    }

    private byte Add(byte a, byte b, bool withCarry = false)
    {
        var carry = withCarry && (Flags & Flags.Carry) != 0 ? 1 : 0;
        var result = a + b + carry;
        
        Flags = Flags.None;
        if (((a & 0x0F) + (b & 0x0F) + carry) > 0x0F) Flags |= Flags.AuxCarry;
        if (result > 0xFF) Flags |= Flags.Carry;
        
        var res = (byte)result;
        SetFlags(res);
        return res;
    }

    private byte Sub(byte a, byte b, bool withBorrow = false)
    {
        var borrow = withBorrow && (Flags & Flags.Carry) != 0 ? 1 : 0;
        var result = a - b - borrow;
        
        Flags = Flags.None;
        if ((a & 0x0F) < (b & 0x0F) + borrow) Flags |= Flags.AuxCarry;
        if (result < 0) Flags |= Flags.Carry;
        
        var res = (byte)result;
        SetFlags(res);
        return res;
    }

    private void Daa()
    {
        var correction = 0;
        if ((A & 0x0F) > 9 || (Flags & Flags.AuxCarry) != 0) correction |= 0x06;
        if ((A >> 4) > 9 || (Flags & Flags.Carry) != 0) correction |= 0x60;
        
        if ((A & 0x0F) > 9) Flags |= Flags.AuxCarry;
        else Flags &= ~Flags.AuxCarry;
        
        if ((A >> 4) > 9 || ((A >> 4) == 9 && (A & 0x0F) > 9)) Flags |= Flags.Carry;
        
        A = Add(A, (byte)correction);
    }

    private byte Inc(byte val)
    {
        var result = val + 1;
        var oldCarry = Flags & Flags.Carry;
        Flags = Flags.None;
        if ((val & 0x0F) == 0x0F) Flags |= Flags.AuxCarry;
        Flags |= oldCarry;
        var res = (byte)result;
        SetFlags(res);
        return res;
    }

    private byte Dec(byte val)
    {
        var result = val - 1;
        var oldCarry = Flags & Flags.Carry;
        Flags = Flags.None;
        Flags |= Flags.AuxCarry;
        Flags |= oldCarry;
        var res = (byte)result;
        SetFlags(res);
        return res;
    }

    private void And(byte val)
    {
        A &= val;
        Flags = Flags.None;
        Flags |= Flags.AuxCarry;
        SetFlags(A);
    }

    private void Or(byte val)
    {
        A |= val;
        Flags = Flags.None;
        SetFlags(A);
    }

    private void Xor(byte val)
    {
        A ^= val;
        Flags = Flags.None;
        SetFlags(A);
    }

    private void Cmp(byte val)
    {
        var origA = A;
        Sub(A, val);
        A = origA;
    }

    private void Rlc() { Flags = (Flags)((Flags & ~Flags.Carry) | ((A & 0x80) != 0 ? Flags.Carry : Flags.None)); A = (byte)((A << 1) | ((A & 0x80) != 0 ? 1 : 0)); }
    private void Rrc() { Flags = (Flags)((Flags & ~Flags.Carry) | ((A & 0x01) != 0 ? Flags.Carry : Flags.None)); A = (byte)((A >> 1) | ((A & 0x01) != 0 ? 0x80 : 0)); }
    private void Ral() { var cy = (Flags & Flags.Carry) != 0; Flags = (Flags)((Flags & ~Flags.Carry) | ((A & 0x80) != 0 ? Flags.Carry : Flags.None)); A = (byte)((A << 1) | (cy ? 1 : 0)); }
    private void Rar() { var cy = (Flags & Flags.Carry) != 0; Flags = (Flags)((Flags & ~Flags.Carry) | ((A & 0x01) != 0 ? Flags.Carry : Flags.None)); A = (byte)((A >> 1) | (cy ? 0x80 : 0)); }

    private bool CheckCondition(int cc) => cc switch
    {
        0 => (Flags & Flags.Zero) == 0,
        1 => (Flags & Flags.Zero) != 0,
        2 => (Flags & Flags.Carry) == 0,
        3 => (Flags & Flags.Carry) != 0,
        4 => (Flags & Flags.Parity) != 0,
        5 => (Flags & Flags.Parity) == 0,
        6 => (Flags & Flags.Sign) == 0,
        7 => (Flags & Flags.Sign) != 0,
        _ => false
    };

    private void Execute(byte op)
    {
        var rp = (op >> 2) & 0x03;
        var ddd = (op >> 3) & 0x07;
        var sss = op & 0x07;
        var addr = FetchWord();
        
        switch (op)
        {
            case 0x00: break;
            case 0x01: BC = addr; break;
            case 0x02: WriteMem(BC, A); break;
            case 0x03: BC++; break;
            case 0x04: B = Inc(B); break;
            case 0x05: B = Dec(B); break;
            case 0x06: B = FetchByte(); break;
            case 0x07: Rlc(); break;
            case 0x08: break;
            case 0x09: HL = Add16(HL, BC); break;
            case 0x0A: A = ReadMem(BC); break;
            case 0x0B: BC--; break;
            case 0x0C: C = Inc(C); break;
            case 0x0D: C = Dec(C); break;
            case 0x0E: C = FetchByte(); break;
            case 0x0F: Rrc(); break;
            case 0x10: break;
            case 0x11: DE = addr; break;
            case 0x12: WriteMem(DE, A); break;
            case 0x13: DE++; break;
            case 0x14: D = Inc(D); break;
            case 0x15: D = Dec(D); break;
            case 0x16: D = FetchByte(); break;
            case 0x17: Ral(); break;
            case 0x18: break;
            case 0x19: HL = Add16(HL, DE); break;
            case 0x1A: A = ReadMem(DE); break;
            case 0x1B: DE--; break;
            case 0x1C: E = Inc(E); break;
            case 0x1D: E = Dec(E); break;
            case 0x1E: E = FetchByte(); break;
            case 0x1F: Rar(); break;
            case 0x20: break;
            case 0x21: HL = addr; break;
            case 0x22: WriteMem(addr, L); WriteMem((ushort)(addr + 1), H); break;
            case 0x23: HL++; break;
            case 0x24: H = Inc(H); break;
            case 0x25: H = Dec(H); break;
            case 0x26: H = FetchByte(); break;
            case 0x27: Daa(); break;
            case 0x28: break;
            case 0x29: HL = Add16(HL, HL); break;
            case 0x2A: L = ReadMem(addr); H = ReadMem((ushort)(addr + 1)); break;
            case 0x2B: HL--; break;
            case 0x2C: L = Inc(L); break;
            case 0x2D: L = Dec(L); break;
            case 0x2E: L = FetchByte(); break;
            case 0x2F: A = (byte)~A; break;
            case 0x30: break;
            case 0x31: SP = addr; break;
            case 0x32: WriteMem(addr, A); break;
            case 0x33: SP++; break;
            case 0x34: WriteMem(HL, Inc(ReadMem(HL))); break;
            case 0x35: WriteMem(HL, Dec(ReadMem(HL))); break;
            case 0x36: WriteMem(HL, FetchByte()); break;
            case 0x37: Flags |= Flags.Carry; break;
            case 0x38: break;
            case 0x39: HL = Add16(HL, SP); break;
            case 0x3A: A = ReadMem(addr); break;
            case 0x3B: SP--; break;
            case 0x3C: A = Inc(A); break;
            case 0x3D: A = Dec(A); break;
            case 0x3E: A = FetchByte(); break;
            case 0x3F: Flags ^= Flags.Carry; Flags &= ~Flags.AuxCarry; break;
            
            case >= 0x40 and <= 0x7F when op != 0x76:
                SetReg(ddd, GetReg(sss));
                break;
            case 0x76: Halted = true; OnHalt?.Invoke(); break;
            
            case 0x80: A = Add(A, GetReg(sss)); break;
            case 0x81: A = Add(A, B); break;
            case 0x82: A = Add(A, C); break;
            case 0x83: A = Add(A, D); break;
            case 0x84: A = Add(A, E); break;
            case 0x85: A = Add(A, H); break;
            case 0x86: A = Add(A, ReadMem(HL)); break;
            case 0x87: A = Add(A, A); break;
            case 0x88: A = Add(A, GetReg(sss), true); break;
            case 0x89: A = Add(A, B, true); break;
            case 0x8A: A = Add(A, C, true); break;
            case 0x8B: A = Add(A, D, true); break;
            case 0x8C: A = Add(A, E, true); break;
            case 0x8D: A = Add(A, H, true); break;
            case 0x8E: A = Add(A, ReadMem(HL), true); break;
            case 0x8F: A = Add(A, A, true); break;
            case 0x90: A = Sub(A, B); break;
            case 0x91: A = Sub(A, B); break;
            case 0x92: A = Sub(A, C); break;
            case 0x93: A = Sub(A, D); break;
            case 0x94: A = Sub(A, E); break;
            case 0x95: A = Sub(A, H); break;
            case 0x96: A = Sub(A, ReadMem(HL)); break;
            case 0x97: A = Sub(A, A); break;
            case 0x98: A = Sub(A, GetReg(sss), true); break;
            case 0x99: A = Sub(A, B, true); break;
            case 0x9A: A = Sub(A, C, true); break;
            case 0x9B: A = Sub(A, D, true); break;
            case 0x9C: A = Sub(A, E, true); break;
            case 0x9D: A = Sub(A, H, true); break;
            case 0x9E: A = Sub(A, ReadMem(HL), true); break;
            case 0x9F: A = Sub(A, A, true); break;
            case 0xA0: And(B); break;
            case 0xA1: And(C); break;
            case 0xA2: And(D); break;
            case 0xA3: And(E); break;
            case 0xA4: And(H); break;
            case 0xA5: And(L); break;
            case 0xA6: And(ReadMem(HL)); break;
            case 0xA7: And(A); break;
            case 0xA8: Xor(B); break;
            case 0xA9: Xor(C); break;
            case 0xAA: Xor(D); break;
            case 0xAB: Xor(E); break;
            case 0xAC: Xor(H); break;
            case 0xAD: Xor(L); break;
            case 0xAE: Xor(ReadMem(HL)); break;
            case 0xAF: Xor(A); break;
            case 0xB0: Or(B); break;
            case 0xB1: Or(C); break;
            case 0xB2: Or(D); break;
            case 0xB3: Or(E); break;
            case 0xB4: Or(H); break;
            case 0xB5: Or(L); break;
            case 0xB6: Or(ReadMem(HL)); break;
            case 0xB7: Or(A); break;
            case 0xB8: Cmp(B); break;
            case 0xB9: Cmp(C); break;
            case 0xBA: Cmp(D); break;
            case 0xBB: Cmp(E); break;
            case 0xBC: Cmp(H); break;
            case 0xBD: Cmp(L); break;
            case 0xBE: Cmp(ReadMem(HL)); break;
            case 0xBF: Cmp(A); break;
            
            case 0xC0: if (!CheckCondition(0)) return; PC = Pop(); break;
            case 0xC1: BC = Pop(); break;
            case 0xC2: if (!CheckCondition(0)) return; PC = addr; break;
            case 0xC3: PC = addr; break;
            case 0xC4: if (!CheckCondition(0)) return; Push(PC); PC = addr; break;
            case 0xC5: Push(HL); break;
            case 0xC6: A = Add(A, FetchByte()); break;
            case 0xC7: Push(PC); PC = 0x00; break;
            case 0xC8: if (!CheckCondition(1)) return; PC = Pop(); break;
            case 0xC9: PC = Pop(); break;
            case 0xCA: if (!CheckCondition(1)) return; PC = addr; break;
            case 0xCC: if (!CheckCondition(1)) return; Push(PC); PC = addr; break;
            case 0xCD: Push(PC); PC = addr; break;
            case 0xCE: A = Add(A, FetchByte(), true); break;
            case 0xCF: Push(PC); PC = 0x08; break;
            case 0xD0: if (!CheckCondition(2)) return; PC = Pop(); break;
            case 0xD1: DE = Pop(); break;
            case 0xD2: if (!CheckCondition(2)) return; PC = addr; break;
            case 0xD3: var portOut = FetchByte(); HandleOutput(portOut, A); break;
            case 0xD4: if (!CheckCondition(2)) return; Push(PC); PC = addr; break;
            case 0xD5: Push(DE); break;
            case 0xD6: A = Sub(A, FetchByte()); break;
            case 0xD7: Push(PC); PC = 0x10; break;
            case 0xD8: if (!CheckCondition(3)) return; PC = Pop(); break;
            case 0xD9: break;
            case 0xDA: if (!CheckCondition(3)) return; PC = addr; break;
            case 0xDB: var portIn = FetchByte(); A = HandleInput(portIn); break;
            case 0xDC: if (!CheckCondition(3)) return; Push(PC); PC = addr; break;
            case 0xDD: break;
            case 0xDE: A = Sub(A, FetchByte(), true); break;
            case 0xDF: Push(PC); PC = 0x18; break;
            case 0xE0: if (!CheckCondition(4)) return; PC = Pop(); break;
            case 0xE1: HL = Pop(); break;
            case 0xE2: if (!CheckCondition(4)) return; PC = addr; break;
            case 0xE3: var tmp = ReadMem(SP); WriteMem(SP, L); L = tmp; tmp = ReadMem((ushort)(SP + 1)); WriteMem((ushort)(SP + 1), H); H = tmp; break;
            case 0xE4: if (!CheckCondition(4)) return; Push(PC); PC = addr; break;
            case 0xE5: Push(HL); break;
            case 0xE6: And(FetchByte()); break;
            case 0xE7: Push(PC); PC = 0x20; break;
            case 0xE8: if (!CheckCondition(5)) return; PC = Pop(); break;
            case 0xE9: PC = HL; break;
            case 0xEA: if (!CheckCondition(5)) return; PC = addr; break;
            case 0xEB: var t = H; H = D; D = t; t = L; L = E; E = t; break;
            case 0xEC: if (!CheckCondition(5)) return; Push(PC); PC = addr; break;
            case 0xED: break;
            case 0xEE: Xor(FetchByte()); break;
            case 0xEF: Push(PC); PC = 0x28; break;
            case 0xF0: if (!CheckCondition(6)) return; PC = Pop(); break;
            case 0xF1: PopPSW(); break;
            case 0xF2: if (!CheckCondition(6)) return; PC = addr; break;
            case 0xF3: InterruptsEnabled = false; break;
            case 0xF4: if (!CheckCondition(6)) return; Push(PC); PC = addr; break;
            case 0xF5: PushPSW(); break;
            case 0xF6: Or(FetchByte()); break;
            case 0xF7: Push(PC); PC = 0x30; break;
            case 0xF8: if (!CheckCondition(7)) return; PC = Pop(); break;
            case 0xF9: SP = HL; break;
            case 0xFA: if (!CheckCondition(7)) return; PC = addr; break;
            case 0xFB: InterruptsEnabled = true; break;
            case 0xFC: if (!CheckCondition(7)) return; Push(PC); PC = addr; break;
            case 0xFD: break;
            case 0xFE: Cmp(FetchByte()); break;
            case 0xFF: Push(PC); PC = 0x38; break;
        }
    }

    private ushort Add16(ushort a, ushort b)
    {
        var result = a + b;
        Flags = (Flags)((Flags & ~Flags.Carry) | (result > 0xFFFF ? Flags.Carry : Flags.None));
        return (ushort)result;
    }

    private byte GetReg(int r) => r switch
    {
        0 => B, 1 => C, 2 => D, 3 => E, 4 => H, 5 => L, 6 => ReadMem(HL), 7 => A, _ => 0
    };

    private void SetReg(int r, byte val)
    {
        switch (r)
        {
            case 0: B = val; break;
            case 1: C = val; break;
            case 2: D = val; break;
            case 3: E = val; break;
            case 4: H = val; break;
            case 5: L = val; break;
            case 6: WriteMem(HL, val); break;
            case 7: A = val; break;
        }
    }

    private byte HandleInput(byte port)
    {
        if (_inputPorts.TryGetValue(port, out var handler))
            return handler(port);
        return OnInput?.Invoke() ?? 0;
    }

    private void HandleOutput(byte port, byte value)
    {
        if (_outputPorts.TryGetValue(port, out var handler))
            handler(port, value);
        OnOutput?.Invoke(value);
    }

    private int GetCycles(byte op)
    {
        return op switch
        {
            0x00 or 0x08 or 0x10 or 0x18 or 0x20 or 0x27 or 0x28 or 0x30 or 0x37 or 0x38 or 0x3F or 0xD9 or 0xDD or 0xED or 0xFD => 4,
            0x76 => 7,
            _ => 4
        };
    }

    public void Interrupt(byte opcode)
    {
        if (!InterruptsEnabled) return;
        InterruptsEnabled = false;
        Execute(opcode);
    }
}
