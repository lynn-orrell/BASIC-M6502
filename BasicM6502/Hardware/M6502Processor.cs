namespace BasicM6502.Hardware;

/// <summary>
/// Simplified 6502 processor emulation for running BASIC interpreter
/// Based on the memory layout and page zero usage from the original assembly code
/// </summary>
public class M6502Processor
{
    // 6502 Memory - 64KB address space
    private readonly byte[] _memory = new byte[65536];
    
    // 6502 Registers
    public byte A { get; set; }  // Accumulator
    public byte X { get; set; }  // X Index Register
    public byte Y { get; set; }  // Y Index Register
    public byte SP { get; set; } // Stack Pointer
    public ushort PC { get; set; } // Program Counter
    public byte P { get; set; }  // Processor Status

    // Page Zero locations used by BASIC (from assembly code)
    public const int LINNUM = 0x14;  // Current line number
    public const int TEMPPT = 0x16;  // Temporary pointer
    public const int INDEX = 0x18;   // General index
    public const int DEST = 0x1A;    // Destination pointer
    public const int RESULT = 0x1C;  // Result pointer
    public const int TXTPTR = 0x7A;  // Text pointer
    public const int VARNXT = 0x7C;  // Next variable
    public const int PRGEND = 0x7E;  // End of program

    // Stack is at page 1 (0x0100-0x01FF)
    public const int STACK_BASE = 0x0100;

    public M6502Processor()
    {
        Reset();
    }

    /// <summary>
    /// Reset the processor to initial state
    /// </summary>
    public void Reset()
    {
        A = 0;
        X = 0;
        Y = 0;
        SP = 0xFF; // Stack starts at top
        PC = 0;
        P = 0;
        
        // Clear memory
        Array.Clear(_memory, 0, _memory.Length);
    }

    /// <summary>
    /// Read a byte from memory
    /// </summary>
    public byte ReadByte(ushort address)
    {
        return _memory[address];
    }

    /// <summary>
    /// Write a byte to memory
    /// </summary>
    public void WriteByte(ushort address, byte value)
    {
        _memory[address] = value;
    }

    /// <summary>
    /// Read a 16-bit word from memory (little-endian)
    /// </summary>
    public ushort ReadWord(ushort address)
    {
        return (ushort)(_memory[address] | (_memory[address + 1] << 8));
    }

    /// <summary>
    /// Write a 16-bit word to memory (little-endian)
    /// </summary>
    public void WriteWord(ushort address, ushort value)
    {
        _memory[address] = (byte)(value & 0xFF);
        _memory[address + 1] = (byte)(value >> 8);
    }

    /// <summary>
    /// Push byte onto stack
    /// </summary>
    public void PushByte(byte value)
    {
        _memory[STACK_BASE + SP] = value;
        SP--;
    }

    /// <summary>
    /// Pull byte from stack
    /// </summary>
    public byte PullByte()
    {
        SP++;
        return _memory[STACK_BASE + SP];
    }

    /// <summary>
    /// Push 16-bit word onto stack (high byte first, then low byte)
    /// </summary>
    public void PushWord(ushort value)
    {
        PushByte((byte)(value >> 8));   // High byte
        PushByte((byte)(value & 0xFF)); // Low byte
    }

    /// <summary>
    /// Pull 16-bit word from stack (low byte first, then high byte)
    /// </summary>
    public ushort PullWord()
    {
        byte low = PullByte();
        byte high = PullByte();
        return (ushort)(low | (high << 8));
    }

    /// <summary>
    /// Get pointer to memory for direct access
    /// </summary>
    public Span<byte> GetMemory()
    {
        return _memory.AsSpan();
    }

    /// <summary>
    /// Initialize memory layout for BASIC
    /// </summary>
    public void InitializeForBasic(PlatformConfiguration config)
    {
        // Set up initial page zero pointers based on configuration
        WriteWord(PRGEND, (ushort)config.RamLocation);
        WriteWord(VARNXT, (ushort)config.RamLocation);
        WriteWord(TXTPTR, (ushort)config.RamLocation);
        
        // Initialize stack pointer
        SP = 0xFF;
        
        // Set program counter to ROM start
        PC = (ushort)config.RomLocation;
    }
}