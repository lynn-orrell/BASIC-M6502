namespace BasicM6502.Hardware;

/// <summary>
/// Platform-specific configuration based on the original REALIO settings
/// from the assembly code (Apple II, Commodore PET, OSI, KIM-1, etc.)
/// </summary>
public class PlatformConfiguration
{
    public string Name { get; init; } = "";
    public int RealIO { get; init; }
    public int RomLocation { get; init; }
    public int RamLocation { get; init; }
    public int RamSize { get; init; }
    public int TerminalWidth { get; init; }
    public int BufferLength { get; init; }
    public bool DiskSupport { get; init; }
    public bool ExtendedIO { get; init; }
    public bool TimeSupport { get; init; }

    /// <summary>
    /// Apple II configuration (REALIO=4)
    /// </summary>
    public static PlatformConfiguration AppleII => new()
    {
        Name = "Apple II",
        RealIO = 4,
        RomLocation = 0x4000,
        RamLocation = 0x0800,
        RamSize = 48 * 1024, // 48K typical for Apple II
        TerminalWidth = 40,
        BufferLength = 72,
        DiskSupport = false,
        ExtendedIO = false,
        TimeSupport = false
    };

    /// <summary>
    /// Commodore PET configuration (REALIO=3)
    /// </summary>
    public static PlatformConfiguration CommodorePET => new()
    {
        Name = "Commodore PET",
        RealIO = 3,
        RomLocation = 0xC000,
        RamLocation = 0x0400,
        RamSize = 32 * 1024, // 32K typical for PET
        TerminalWidth = 40,
        BufferLength = 81,
        DiskSupport = true,
        ExtendedIO = true,
        TimeSupport = true
    };

    /// <summary>
    /// Ohio Scientific configuration (REALIO=2)
    /// </summary>
    public static PlatformConfiguration OSI => new()
    {
        Name = "Ohio Scientific",
        RealIO = 2,
        RomLocation = 0xA000,
        RamLocation = 0x0200,
        RamSize = 8 * 1024, // 8K typical for OSI
        TerminalWidth = 32,
        BufferLength = 72,
        DiskSupport = false,
        ExtendedIO = false,
        TimeSupport = false
    };

    /// <summary>
    /// KIM-1 configuration (REALIO=1)
    /// </summary>
    public static PlatformConfiguration KIM1 => new()
    {
        Name = "MOS Technology KIM-1",
        RealIO = 1,
        RomLocation = 0x2000,
        RamLocation = 0x0200,
        RamSize = 1 * 1024, // 1K typical for KIM-1
        TerminalWidth = 20,
        BufferLength = 72,
        DiskSupport = true,
        ExtendedIO = false,
        TimeSupport = false
    };

    /// <summary>
    /// PDP-10 simulation configuration (REALIO=0)
    /// </summary>
    public static PlatformConfiguration PDP10Sim => new()
    {
        Name = "PDP-10 Simulation",
        RealIO = 0,
        RomLocation = 0x2000,
        RamLocation = 0x1400,
        RamSize = 64 * 1024, // Large memory for simulation
        TerminalWidth = 72,
        BufferLength = 72,
        DiskSupport = false,
        ExtendedIO = false,
        TimeSupport = false
    };
}