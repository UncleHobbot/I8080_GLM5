# Intel 8080 Emulator - CP/M Personal Computer

A full-featured Intel 8080 microprocessor emulator with CP/M operating system, built with .NET 10 backend and React frontend.

## Features

- **Intel 8080 CPU Emulation**: Complete instruction set support
- **CP/M 2.2 Operating System**: BDOS and CCP implementation
- **Text Editor**: Built-in line editor (ED)
- **8080 Assembler**: Assemble programs from source
- **BASIC Interpreter**: Run BASIC programs
- **Web-based Terminal**: React frontend with xterm.js

## Architecture

```
src/
├── backend/
│   ├── I8080Emulator.Core/      # CPU emulator core
│   ├── I8080Emulator.CPM/       # CP/M OS, Editor, Assembler, BASIC
│   └── I8080Emulator.Api/       # ASP.NET Core WebAPI
└── frontend/
    └── src/                     # React + xterm.js terminal
```

## Quick Start

### Backend (.NET 10)
```bash
cd src/backend
dotnet restore
dotnet run --project I8080Emulator.Api
```

### Frontend (React)
```bash
cd src/frontend
npm install
npm start
```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/emulator/session` | POST | Create new session |
| `/api/emulator/session/{id}` | GET | Get session status |
| `/api/emulator/session/{id}/command` | POST | Execute command |
| `/api/emulator/session/{id}/load` | POST | Load program |
| `/api/emulator/session/{id}/run` | POST | Run program |
| `/api/emulator/session/{id}/step` | POST | Single step |
| `/api/emulator/session/{id}/memory` | GET | Dump memory |

## CP/M Commands

- `DIR` / `LS` - List files
- `TYPE` / `CAT` - Display file
- `ED` / `EDIT` - Text editor
- `ASM` - 8080 Assembler
- `BASIC` - BASIC interpreter
- `HELP` - Show help

## BASIC Commands

- `NEW` - Clear program
- `LIST` - List program
- `RUN` - Execute program
- `PRINT`, `LET`, `INPUT`, `IF...THEN`
- `GOTO`, `GOSUB`, `RETURN`
- `FOR...NEXT`, `END`

## Sample Programs

### Assembly (8080)
```asm
        MVI A, 0
        MVI B, 10
LOOP:   ADD B
        DCR B
        JNZ LOOP
        HLT
```

### BASIC
```basic
10 FOR I = 1 TO 10
20 PRINT "HELLO WORLD"
30 NEXT I
40 END
```

## License

MIT License
