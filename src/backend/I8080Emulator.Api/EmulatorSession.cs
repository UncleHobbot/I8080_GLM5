using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using I8080Emulator.CPM;
using I8080Emulator.Core;
using I8080Emulator.Core.CPU;

namespace I8080Emulator.Api;

public class EmulatorSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Memory Memory { get; set; } = new();
    public Intel8080 Cpu { get; set; }
    public BDOS Bdos { get; set; }
    public CCP Ccp { get; set; }
    public DiskDrive[] Drives { get; set; }
    public StringBuilder Output { get; set; } = new();
    public Queue<char> InputQueue { get; set; } = new();
    public DateTime LastAccess { get; set; } = DateTime.UtcNow;
    
    public EmulatorSession()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "i8080emu", Id);
        Drives = new DiskDrive[]
        {
            new(0, Path.Combine(basePath, "A")),
            new(1, Path.Combine(basePath, "B"))
        };
        
        Cpu = new Intel8080(Memory);
        Bdos = new BDOS(Memory, Drives);
        Ccp = new CCP(Bdos, Memory);
        
        Ccp.OnOutput += text => Output.Append(text);
        Ccp.OnInputLine += () =>
        {
            var sb = new StringBuilder();
            while (InputQueue.TryDequeue(out var c))
            {
                if (c == '\r' || c == '\n') break;
                sb.Append(c);
            }
            return sb.ToString();
        };
    }
}
