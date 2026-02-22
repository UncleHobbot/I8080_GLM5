# Intel 8080 Emulator - Project Plan

## Completed
- [x] Project structure setup
- [x] .NET 10 solution with 3 projects
- [x] Intel 8080 CPU emulator core (256 opcodes)
- [x] Memory management (64KB address space)
- [x] CP/M BDOS (file operations, console I/O)
- [x] CP/M CCP (command processor)
- [x] Disk drive emulation
- [x] Text Editor (ED)
- [x] 8080 Assembler
- [x] BASIC Interpreter
- [x] ASP.NET Core WebAPI
- [x] React frontend with xterm.js terminal
- [x] CPU status panel
- [x] Memory viewer

## Architecture
- Backend: .NET 10, ASP.NET Core WebAPI
- Frontend: React 18, xterm.js
- CPU: Full Intel 8080 instruction set
- OS: CP/M 2.2 compatible

## Usage
1. Start backend: `cd src/backend && dotnet run --project I8080Emulator.Api`
2. Start frontend: `cd src/frontend && npm install && npm start`
3. Open http://localhost:3000

## Notes
- Sessions are ephemeral (stored in memory)
- Virtual disk files stored in temp directory
- Terminal uses polling for output updates
