# AGENTS.md - Intel 8080 Emulator Project

Intel 8080 microprocessor emulator with CP/M, .NET 10 backend, React frontend.

```
src/
├── backend/
│   ├── I8080Emulator.Core/      # CPU emulator, Memory
│   ├── I8080Emulator.CPM/       # CP/M OS, Editor, Assembler, BASIC
│   └── I8080Emulator.Api/       # ASP.NET Core WebAPI
└── frontend/
    └── src/                     # React + xterm.js terminal
```

---

## Build Commands

### Backend (.NET 10)
```bash
cd src/backend
dotnet restore
dotnet build I8080Emulator.sln
dotnet run --project I8080Emulator.Api --urls "http://localhost:5000"
dotnet watch --project I8080Emulator.Api  # Hot reload
dotnet clean
```

### Frontend (React)
```bash
cd src/frontend
npm install
npm start
npm run build
```

### Testing (when added)
```bash
dotnet test                                    # All tests
dotnet test --filter "FullyQualifiedName~Intel8080Tests"  # Specific class
dotnet test --filter "FullyQualifiedName~AddInstruction"  # Single test
dotnet test --verbosity normal                 # Verbose output
```

---

## Code Style Guidelines

### C# (.NET 10)

**Naming:**
- PascalCase: classes, methods, public properties, enums
- camelCase with `_` prefix: private fields
- Interfaces: `I` prefix (e.g., `IMemoryDevice`)

**Imports:** ImplicitUsings enabled. Order: System → Third-party → Project-specific

```csharp
using System;
using System.Collections.Generic;
using I8080Emulator.Core;
```

**Formatting:**
- 4 spaces indentation
- Single-line methods encouraged
- Prefer switch expressions over statements

```csharp
public ushort BC 
{ 
    get => (ushort)((B << 8) | C); 
    set { B = (byte)(value >> 8); C = (byte)(value & 0xFF); } 
}

private byte FetchByte() => _memory.Read(PC++);
```

**Null Handling:** Nullable enabled. Use `?`, `?.`, `??`

```csharp
public string? ReadLine() => OnInputLine?.Invoke();
if (session == null) return NotFound();
```

### JavaScript/React

**Imports:** React → Third-party → Local

```javascript
import React, { useState, useEffect, useCallback } from 'react';
import { emulatorApi } from './services/api';
```

**Naming:** PascalCase for components, camelCase for functions/variables

**Hooks:** Use destructuring, useCallback for handlers

```javascript
const handleReset = async () => {
    if (!sessionId) return;
    await emulatorApi.resetCpu(sessionId);
};
```

---

## Project Patterns

### CPU Emulator
- Use `byte`/`ushort` for 8/16-bit values (not int)
- Flags as `[Flags]` enum
- Memory access via Memory class

### CP/M Components
- Events for I/O: `OnOutput`, `OnInput`, `OnHalt`
- DMA address defaults to 0x0080

### API Controllers
- DI for SessionManager
- `ActionResult<T>` for responses
- Route: `api/[controller]`

---

## Notes
- .NET 10 is in preview
- Frontend auto-detects available port (3000+)
- Backend runs on port 5000
- Sessions are in-memory (no persistence)
- Virtual disk in OS temp directory

---

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

---

## CP/M Commands Available
- `DIR` / `LS` - List files
- `TYPE` / `CAT` - Display file  
- `ED` / `EDIT` - Text editor
- `ASM` - 8080 Assembler
- `BASIC` - BASIC interpreter
- `HELP` - Show help
