using System;
using I8080Emulator.Api.Models;
using I8080Emulator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace I8080Emulator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmulatorController : ControllerBase
{
    private readonly SessionManager _sessionManager;
    
    public EmulatorController(SessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }
    
    [HttpPost("session")]
    public ActionResult<SessionResponse> CreateSession()
    {
        var session = _sessionManager.CreateSession();
        return Ok(new SessionResponse { SessionId = session.Id });
    }
    
    [HttpGet("session/{id}")]
    public ActionResult<SessionStatus> GetSessionStatus(string id)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        return Ok(new SessionStatus
        {
            SessionId = session.Id,
            Output = session.Output.ToString(),
            CpuState = new CpuState
            {
                A = session.Cpu.A,
                B = session.Cpu.B,
                C = session.Cpu.C,
                D = session.Cpu.D,
                E = session.Cpu.E,
                H = session.Cpu.H,
                L = session.Cpu.L,
                Flags = (byte)session.Cpu.Flags,
                SP = session.Cpu.SP,
                PC = session.Cpu.PC
            }
        });
    }
    
    [HttpPost("session/{id}/input")]
    public ActionResult SendInput(string id, [FromBody] InputRequest request)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        foreach (var c in request.Input)
            session.InputQueue.Enqueue(c);
        session.InputQueue.Enqueue('\r');
        
        return Ok();
    }
    
    [HttpPost("session/{id}/command")]
    public ActionResult<CommandResponse> ExecuteCommand(string id, [FromBody] CommandRequest request)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        session.Output.Clear();
        
        foreach (var c in request.Command)
            session.InputQueue.Enqueue(c);
        session.InputQueue.Enqueue('\r');
        
        return Ok(new CommandResponse { Output = session.Output.ToString() });
    }
    
    [HttpPost("session/{id}/load")]
    public ActionResult LoadProgram(string id, [FromBody] LoadRequest request)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        session.Memory.Load((ushort)request.Address, request.Program);
        return Ok();
    }
    
    [HttpPost("session/{id}/run")]
    public ActionResult<CpuState> RunProgram(string id, [FromBody] RunRequest? request = null)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        var startAddress = request?.StartAddress ?? 0x0100;
        session.Cpu.PC = (ushort)startAddress;
        
        var maxCycles = request?.MaxCycles ?? 100000;
        var cycles = 0;
        
        while (!session.Cpu.Halted && cycles < maxCycles)
        {
            cycles += session.Cpu.Step();
        }
        
        return Ok(new CpuState
        {
            A = session.Cpu.A,
            B = session.Cpu.B,
            C = session.Cpu.C,
            D = session.Cpu.D,
            E = session.Cpu.E,
            H = session.Cpu.H,
            L = session.Cpu.L,
            Flags = (byte)session.Cpu.Flags,
            SP = session.Cpu.SP,
            PC = session.Cpu.PC
        });
    }
    
    [HttpPost("session/{id}/step")]
    public ActionResult<CpuState> StepProgram(string id)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        session.Cpu.Step();
        
        return Ok(new CpuState
        {
            A = session.Cpu.A,
            B = session.Cpu.B,
            C = session.Cpu.C,
            D = session.Cpu.D,
            E = session.Cpu.E,
            H = session.Cpu.H,
            L = session.Cpu.L,
            Flags = (byte)session.Cpu.Flags,
            SP = session.Cpu.SP,
            PC = session.Cpu.PC
        });
    }
    
    [HttpPost("session/{id}/reset")]
    public ActionResult ResetCpu(string id)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        session.Cpu.Reset();
        return Ok();
    }
    
    [HttpGet("session/{id}/memory")]
    public ActionResult<MemoryResponse> DumpMemory(string id, [FromQuery] ushort address = 0, [FromQuery] int length = 256)
    {
        var session = _sessionManager.GetSession(id);
        if (session == null) return NotFound();
        
        var data = session.Memory.Dump(address, Math.Min(length, 4096));
        return Ok(new MemoryResponse { Address = address, Data = data });
    }
    
    [HttpDelete("session/{id}")]
    public ActionResult DeleteSession(string id)
    {
        return _sessionManager.RemoveSession(id) ? Ok() : NotFound();
    }
}
