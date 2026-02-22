# Intel 8080 Emulator - User Guide

## Table of Contents

1. [Getting Started](#getting-started)
2. [Terminal Interface](#terminal-interface)
3. [CP/M Commands](#cpm-commands)
4. [Text Editor (ED)](#text-editor-ed)
5. [8080 Assembler](#8080-assembler)
6. [BASIC Interpreter](#basic-interpreter)

---

## Getting Started

### System Requirements

- Modern web browser (Chrome, Firefox, Edge, Safari)
- Backend server running on port 5000
- Frontend accessible via web browser

### Starting the Emulator

1. **Start the Backend Server**
   ```bash
   cd src/backend/I8080Emulator.Api
   dotnet run --urls "http://localhost:5000"
   ```

2. **Start the Frontend**
   ```bash
   cd src/frontend
   npm start
   ```

3. **Open in Browser**
   - Navigate to `http://localhost:3000` (or the port shown)
   - The terminal will display the CP/M boot message

### Interface Overview

The emulator interface consists of:

- **Terminal Tab**: CP/M command line interface
- **CPU Status Tab**: View CPU registers and flags
- **Memory Tab**: Inspect memory contents
- **Control Buttons**: Step, Reset, New Session

---

## Terminal Interface

### The Command Prompt

When you start the emulator, you'll see:

```
A>
```

This is the CP/M command prompt. The `A` indicates drive A is the current drive.

### Entering Commands

- Type commands at the prompt and press **Enter**
- Commands are case-insensitive (DIR, dir, Dir all work)
- Use backspace to correct typing errors
- Press Enter to execute commands

### Keyboard Shortcuts

| Key | Function |
|-----|----------|
| Enter | Execute command |
| Backspace | Delete last character |
| Tab | Switch between tabs |

### Terminal Output

All output from programs appears in the terminal window. The terminal supports:
- Scrolling (use scroll bar or mouse wheel)
- Text selection for copying

---

## CP/M Commands

### File Management Commands

#### DIR / LS - List Files

Lists all files on the current drive.

```
A>DIR
A>LS
```

#### TYPE / CAT - Display File

Displays the contents of a text file.

```
A>TYPE README.TXT
A>CAT HELLO.BAS
```

#### ERA / DEL / RM - Delete File

Removes a file from the disk.

```
A>ERA OLDFILE.TXT
A>DEL TEMP.BAK
A>RM TEST.COM
```

#### REN - Rename File

Renames a file.

```
A>REN NEWNAME.TXT=OLDNAME.TXT
```

#### SAVE - Save File

Saves the current memory contents to a file.

```
A>SAVE MYFILE.TXT
```

### System Commands

#### HELP / ? - Show Help

Displays available commands.

```
A>HELP
A>?
```

Output:
```
Available commands:
  DIR / LS     - List files
  TYPE / CAT   - Display file
  ERA / DEL    - Delete file
  REN          - Rename file
  SAVE         - Save file
  ED / EDIT    - Text editor
  ASM          - 8080 Assembler
  BASIC        - BASIC interpreter
  HELP / ?     - This help
  EXIT         - Exit emulator
```

#### EXIT / QUIT - Exit Emulator

Terminates the emulator session.

```
A>EXIT
A>QUIT
```

### Drive Selection

Switch between virtual drives:

```
A>B:
B>
```

---

## Text Editor (ED)

The built-in text editor (ED) is a line-based editor similar to the original CP/M ED.

### Starting the Editor

```
A>ED MYFILE.TXT
A>EDIT PROGRAM.ASM
```

If the file doesn't exist, a new file is created.

### Editor Commands

| Command | Description |
|---------|-------------|
| I | Enter insert mode |
| D n | Delete line n |
| L | List all lines |
| L n-m | List lines n through m |
| S | Save file |
| Q | Quit editor |
| H | Show help |

### Insert Mode

When you press `I`, you enter insert mode:

```
*I
Enter text (empty line to exit insert mode):
   1: This is line one
   2: This is line two
   3: 
*
```

Press Enter on an empty line to exit insert mode.

### Deleting Lines

```
*D 5
Deleted line 5
```

### Listing Lines

List all lines:
```
*L
   1: First line
   2: Second line
   3: Third line
```

List a range:
```
*L 2-4
   2: Second line
   3: Third line
   4: Fourth line
```

### Saving and Quitting

```
*S
Saved 10 lines to MYFILE.TXT
```

```
*Q
A>
```

### Example Session

```
A>ED HELLO.TXT
ED - CP/M Text Editor
Editing: HELLO.TXT
Type SAVE to save, QUIT to exit

*I
Enter text (empty line to exit insert mode):
   1: Hello, World!
   2: This is a test file.
   3: 
*L
   1: Hello, World!
   2: This is a test file.
*S
Saved 2 lines to HELLO.TXT
*Q
A>TYPE HELLO.TXT
Hello, World!
This is a test file.
```

---

## 8080 Assembler

The assembler converts 8080 assembly language into machine code.

### Starting the Assembler

```
A>ASM PROGRAM.ASM
```

### Assembly Language Syntax

#### Comments

Comments start with a semicolon:

```asm
; This is a comment
MVI A, 0   ; Initialize A to zero
```

#### Labels

Labels end with a colon:

```asm
START:  MVI A, 0
LOOP:   ADD B
        DCR B
        JNZ LOOP
        HLT
```

#### Directives

| Directive | Description |
|-----------|-------------|
| ORG nnnn | Set origin address |

Example:
```asm
        ORG 0100H    ; Program starts at 0100H
```

### Instruction Set Reference

#### Data Transfer

| Instruction | Description |
|-------------|-------------|
| MOV dst,src | Move byte |
| MVI r,n | Move immediate |
| LXI rp,nn | Load register pair immediate |
| LDA addr | Load accumulator |
| STA addr | Store accumulator |
| LHLD addr | Load HL direct |
| SHLD addr | Store HL direct |
| LDAX rp | Load accumulator indirect |
| STAX rp | Store accumulator indirect |
| XCHG | Exchange DE and HL |

#### Arithmetic

| Instruction | Description |
|-------------|-------------|
| ADD r | Add register |
| ADC r | Add with carry |
| ADI n | Add immediate |
| ACI n | Add immediate with carry |
| SUB r | Subtract |
| SBB r | Subtract with borrow |
| SUI n | Subtract immediate |
| SBI n | Subtract immediate with borrow |
| INR r | Increment |
| DCR r | Decrement |
| INX rp | Increment register pair |
| DCX rp | Decrement register pair |
| DAD rp | Double add to HL |
| DAA | Decimal adjust |

#### Logical

| Instruction | Description |
|-------------|-------------|
| ANA r | AND |
| ANI n | AND immediate |
| ORA r | OR |
| ORI n | OR immediate |
| XRA r | XOR |
| XRI n | XOR immediate |
| CMA | Complement |
| CMC | Complement carry |
| STC | Set carry |
| CMP r | Compare |
| CPI n | Compare immediate |

#### Branch

| Instruction | Description |
|-------------|-------------|
| JMP addr | Jump |
| JZ addr | Jump if zero |
| JNZ addr | Jump if not zero |
| JC addr | Jump if carry |
| JNC addr | Jump if no carry |
| JP addr | Jump if positive |
| JM addr | Jump if minus |
| JPE addr | Jump if parity even |
| JPO addr | Jump if parity odd |
| CALL addr | Call subroutine |
| CZ addr | Call if zero |
| CNZ addr | Call if not zero |
| CC addr | Call if carry |
| CNC addr | Call if no carry |
| RET | Return |
| RZ | Return if zero |
| RNZ | Return if not zero |
| RC | Return if carry |
| RNC | Return if no carry |
| RST n | Restart |

#### Stack

| Instruction | Description |
|-------------|-------------|
| PUSH rp | Push register pair |
| POP rp | Pop register pair |
| XTHL | Exchange top of stack with HL |
| SPHL | Load SP from HL |

#### I/O and Control

| Instruction | Description |
|-------------|-------------|
| IN port | Input from port |
| OUT port | Output to port |
| EI | Enable interrupts |
| DI | Disable interrupts |
| HLT | Halt |
| NOP | No operation |

#### Register Codes

| Code | Register |
|------|----------|
| A | Accumulator |
| B | Register B |
| C | Register C |
| D | Register D |
| E | Register E |
| H | Register H |
| L | Register L |
| M | Memory (HL) |

#### Register Pairs

| Code | Pair |
|------|------|
| B | BC |
| D | DE |
| H | HL |
| SP | Stack Pointer |
| PSW | Program Status Word |

### Example Programs

#### Hello World (prints character)

```asm
; Print 'A' to console
        MVI A, 'A'
        OUT 0
        HLT
```

#### Simple Loop

```asm
; Count from 10 to 0
        MVI B, 10    ; Counter
LOOP:   DCR B        ; Decrement
        JNZ LOOP     ; Loop if not zero
        HLT          ; Stop
```

#### Addition

```asm
; Add numbers 1 to 10
        MVI A, 0     ; Clear accumulator
        MVI B, 10    ; Counter
ADDLP:  ADD B        ; Add B to A
        DCR B        ; Decrement counter
        JNZ ADDLP    ; Continue if not zero
        HLT          ; Result in A
```

#### Subroutine Call

```asm
; Demonstrate subroutine
        MVI A, 5
        CALL DOUBLE
        HLT

DOUBLE: ADD A        ; A = A * 2
        RET
```

### Number Formats

| Format | Example | Value |
|--------|---------|-------|
| Decimal | 255 | 255 |
| Hex (prefix) | 0xFF | 255 |
| Hex (suffix) | 0FFH | 255 |

---

## BASIC Interpreter

The BASIC interpreter supports a subset of the BASIC programming language.

### Starting BASIC

```
A>BASIC
BASIC Interpreter v1.0
Type LIST to list program, RUN to execute

]
```

### BASIC Commands

| Command | Description |
|---------|-------------|
| NEW | Clear current program |
| LIST | Display program |
| RUN | Execute program |
| CLR | Clear variables |
| HELP | Show help |
| QUIT | Exit BASIC |

### Statement Reference

#### PRINT - Output Values

```
PRINT "Hello World"
PRINT 42
PRINT A
PRINT "Value: "; A
PRINT "X="; X, "Y="; Y
```

#### LET - Assign Variables

```
LET A = 10
LET B = A * 2
LET NAME$ = "John"
```

Note: LET is optional:
```
A = 10
B = A * 2
```

#### INPUT - Get User Input

```
INPUT A
INPUT "Enter name: "; NAME$
INPUT "Age: "; AGE
```

#### IF...THEN - Conditional

```
IF A > 10 THEN PRINT "Big"
IF X = 0 THEN GOTO 100
IF A < B THEN C = A ELSE C = B
```

#### GOTO - Unconditional Jump

```
GOTO 100
```

#### GOSUB / RETURN - Subroutines

```
GOSUB 500
...
500 PRINT "Subroutine"
510 RETURN
```

#### FOR...NEXT - Loops

```
FOR I = 1 TO 10
    PRINT I
NEXT I
```

With STEP:
```
FOR I = 10 TO 0 STEP -1
    PRINT I
NEXT I
```

#### END / STOP - Program Termination

```
END
STOP
```

#### REM - Comments

```
REM This is a comment
```

### Operators

#### Arithmetic

| Operator | Description |
|----------|-------------|
| + | Addition |
| - | Subtraction |
| * | Multiplication |
| / | Division |

#### Comparison

| Operator | Description |
|----------|-------------|
| = | Equal |
| < | Less than |
| > | Greater than |
| <= | Less than or equal |
| >= | Greater than or equal |
| <> | Not equal |

### Variables

- Numeric variables: `A`, `COUNT`, `X1`
- String variables: `NAME$`, `TEXT$`
- Variable names can contain letters and numbers
- First character must be a letter

### Example Programs

#### Hello World

```basic
10 PRINT "Hello, World!"
20 END
```

#### Count to 10

```basic
10 FOR I = 1 TO 10
20 PRINT I
30 NEXT I
40 END
```

Run:
```
]RUN
1
2
3
4
5
6
7
8
9
10
```

#### Sum Numbers

```basic
10 LET S = 0
20 FOR I = 1 TO 100
30 LET S = S + I
40 NEXT I
50 PRINT "Sum = "; S
60 END
```

#### Fibonacci Sequence

```basic
10 REM Fibonacci Sequence
20 LET A = 0
30 LET B = 1
40 FOR I = 1 TO 10
50 PRINT A
60 LET C = A + B
70 LET A = B
80 LET B = C
90 NEXT I
100 END
```

#### Factorial Calculator

```basic
10 PRINT "Factorial Calculator"
20 INPUT "Enter N: "; N
30 LET F = 1
40 FOR I = 2 TO N
50 LET F = F * I
60 NEXT I
70 PRINT N; "! = "; F
80 END
```

#### Number Guessing Game

```basic
10 REM Number Guessing Game
20 LET N = 42
30 PRINT "Guess a number between 1 and 100"
40 INPUT "Your guess: "; G
50 IF G = N THEN PRINT "Correct!" : END
60 IF G < N THEN PRINT "Too low"
70 IF G > N THEN PRINT "Too high"
80 GOTO 40
```

#### Multiplication Table

```basic
10 REM Multiplication Table
20 INPUT "Enter number: "; N
30 FOR I = 1 TO 12
40 PRINT N; " x "; I; " = "; N * I
50 NEXT I
60 END
```

### Complete BASIC Session Example

```
A>BASIC
BASIC Interpreter v1.0
Type LIST to list program, RUN to execute

]10 PRINT "Number Sum Program"
]20 INPUT "How many numbers? "; N
]30 LET S = 0
]40 FOR I = 1 TO N
]50 INPUT "Enter number: "; X
]60 LET S = S + X
]70 NEXT I
]80 PRINT "Sum = "; S
]90 PRINT "Average = "; S / N
]100 END
]LIST
10 PRINT "Number Sum Program"
20 INPUT "How many numbers? "; N
30 LET S = 0
40 FOR I = 1 TO N
50 INPUT "Enter number: "; X
60 LET S = S + X
70 NEXT I
80 PRINT "Sum = "; S
90 PRINT "Average = "; S / N
100 END
]RUN
Number Sum Program
How many numbers? 5
Enter number: 10
Enter number: 20
Enter number: 30
Enter number: 40
Enter number: 50
Sum = 150
Average = 30
]QUIT
A>
```

---

## CPU Status Panel

Click the **CPU Status** tab to view:

### Registers

| Register | Description |
|----------|-------------|
| A | Accumulator |
| B, C, D, E, H, L | General purpose |
| SP | Stack Pointer |
| PC | Program Counter |

### Flags

| Flag | Description |
|------|-------------|
| S | Sign (negative result) |
| Z | Zero (result is zero) |
| AC | Auxiliary Carry |
| P | Parity (even number of 1-bits) |
| C | Carry |

---

## Memory Viewer

Click the **Memory** tab to inspect memory:

1. Enter a hex address (e.g., 256 or 0x100)
2. Click **View Memory**
3. Memory displayed in hex and ASCII format

### Memory Layout

| Address | Contents |
|---------|----------|
| 0000-00FF | Zero page, interrupt vectors |
| 0100-FFFF | Program area |

---

## Troubleshooting

### Terminal Not Responding

- Check that the backend server is running
- Refresh the browser page
- Click "New Session" to start fresh

### Program Won't Run

- Check for syntax errors
- Verify memory address (default 0x0100)
- Use CPU Status to check program counter

### Lost Work

- Remember to SAVE files in the editor
- Use "New Session" only when ready to start fresh

---

## Tips and Best Practices

1. **Save Often**: Use SAVE command or ED's S command
2. **Test Incrementally**: Test programs in small pieces
3. **Use Comments**: Add REM statements in BASIC
4. **Check Register Values**: Use CPU Status panel while debugging
5. **Memory Inspection**: Use Memory Viewer to verify data

---

## Keyboard Reference Card

### CP/M Commands
```
DIR      List files          ERA      Delete file
TYPE     Display file        REN      Rename file
ED       Text editor         ASM      Assembler
BASIC    BASIC interpreter   HELP     Show help
```

### Editor Commands
```
I        Insert mode         D n      Delete line n
L        List lines          S        Save
Q        Quit                H        Help
```

### BASIC Commands
```
NEW      Clear program       LIST     Show program
RUN      Execute             CLR      Clear variables
```

### BASIC Statements
```
PRINT    Output              INPUT    Get input
LET      Assign              IF       Conditional
GOTO     Jump                GOSUB    Call subroutine
FOR      Loop start          NEXT     Loop end
END      Stop                REM      Comment
```
