using System;

namespace I8080Emulator.Api.Models;

public class SessionResponse
{
    public string SessionId { get; set; } = "";
}

public class SessionStatus
{
    public string SessionId { get; set; } = "";
    public string Output { get; set; } = "";
    public CpuState CpuState { get; set; } = new();
}

public class CpuState
{
    public byte A { get; set; }
    public byte B { get; set; }
    public byte C { get; set; }
    public byte D { get; set; }
    public byte E { get; set; }
    public byte H { get; set; }
    public byte L { get; set; }
    public byte Flags { get; set; }
    public ushort SP { get; set; }
    public ushort PC { get; set; }
}

public class InputRequest
{
    public string Input { get; set; } = "";
}

public class CommandRequest
{
    public string Command { get; set; } = "";
}

public class CommandResponse
{
    public string Output { get; set; } = "";
}

public class LoadRequest
{
    public byte[] Program { get; set; } = Array.Empty<byte>();
    public int Address { get; set; } = 0x0100;
}

public class RunRequest
{
    public int StartAddress { get; set; } = 0x0100;
    public int MaxCycles { get; set; } = 100000;
}

public class MemoryResponse
{
    public ushort Address { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}
