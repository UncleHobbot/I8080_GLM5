# Quick Reference Card

## CP/M Commands

| Command | Alias | Description |
|---------|-------|-------------|
| `DIR` | `LS` | List files |
| `TYPE file` | `CAT` | Display file |
| `ERA file` | `DEL`, `RM` | Delete file |
| `REN new=old` | - | Rename file |
| `ED file` | `EDIT` | Text editor |
| `ASM file` | - | Assembler |
| `BASIC` | - | BASIC interpreter |
| `HELP` | `?` | Show help |
| `EXIT` | `QUIT` | Exit emulator |

## Editor (ED) Commands

| Command | Description |
|---------|-------------|
| `I` | Insert mode |
| `D n` | Delete line n |
| `L` | List all |
| `L n-m` | List range |
| `S` | Save |
| `Q` | Quit |
| `H` | Help |

## 8080 Instructions

### Data Transfer
```
MOV  dst,src   Move          MVI  r,n      Move immediate
LXI  rp,nn     Load pair     LDA  addr     Load A direct
STA  addr      Store A       LHLD addr     Load HL
SHLD addr      Store HL      LDAX rp       Load indirect
STAX rp        Store indirect XCHG         Exchange
```

### Arithmetic
```
ADD  r         Add           ADC  r        Add with carry
ADI  n         Add imm       ACI  n        Add imm + carry
SUB  r         Subtract      SBB  r        Sub with borrow
SUI  n         Sub imm       SBI  n        Sub imm + borrow
INR  r         Increment     DCR  r        Decrement
INX  rp        Inc pair      DCX  rp       Dec pair
DAD  rp        Add to HL     DAA           Decimal adjust
```

### Logical
```
ANA  r         AND           ANI  n        AND immediate
ORA  r         OR            ORI  n        OR immediate
XRA  r         XOR           XRI  n        XOR immediate
CMA            Complement    CMC           Complement carry
STC            Set carry     CMP  r        Compare
CPI  n         Compare imm
```

### Branch
```
JMP  addr      Jump          JZ   addr     Jump if zero
JNZ  addr      Jump not zero JC   addr     Jump if carry
JNC  addr      Jump no carry JP   addr     Jump positive
JM   addr      Jump minus    CALL addr     Call
RET            Return        RZ            Ret if zero
RNZ            Ret not zero  RC            Ret if carry
RNC            Ret no carry
```

### Stack & I/O
```
PUSH rp       Push pair      POP  rp       Pop pair
XTHL          Exchange SP    SPHL          SP from HL
IN   port     Input          OUT  port     Output
EI            Enable int     DI            Disable int
HLT           Halt           NOP           No operation
```

## BASIC Commands

| Command | Description |
|---------|-------------|
| `NEW` | Clear program |
| `LIST` | Display program |
| `RUN` | Execute |
| `CLR` | Clear variables |
| `QUIT` | Exit BASIC |

## BASIC Statements

| Statement | Description |
|-----------|-------------|
| `PRINT x` | Output value |
| `INPUT v` | Get input |
| `LET v = x` | Assign (optional LET) |
| `IF c THEN s` | Conditional |
| `GOTO n` | Jump to line |
| `GOSUB n` | Call subroutine |
| `RETURN` | Return from subroutine |
| `FOR v=a TO b` | Loop start |
| `NEXT v` | Loop end |
| `END` | Stop program |
| `REM` | Comment |

## BASIC Operators

| Type | Operators |
|------|-----------|
| Arithmetic | `+ - * /` |
| Comparison | `= < > <= >= <>` |

## CPU Registers

| Register | Size | Purpose |
|----------|------|---------|
| A | 8-bit | Accumulator |
| B, C | 8-bit | General/BC pair |
| D, E | 8-bit | General/DE pair |
| H, L | 8-bit | General/HL pair |
| SP | 16-bit | Stack Pointer |
| PC | 16-bit | Program Counter |

## CPU Flags

| Flag | Bit | Meaning |
|------|-----|---------|
| S | 7 | Sign (negative) |
| Z | 6 | Zero |
| AC | 4 | Auxiliary Carry |
| P | 2 | Parity (even) |
| C | 0 | Carry |

## Memory Map

| Range | Purpose |
|-------|---------|
| 0000-00FF | Zero page |
| 0100-FFFF | User programs |

## Number Formats

| Format | Example | Value |
|--------|---------|-------|
| Decimal | 255 | 255 |
| Hex (prefix) | 0xFF | 255 |
| Hex (suffix) | 0FFH | 255 |
