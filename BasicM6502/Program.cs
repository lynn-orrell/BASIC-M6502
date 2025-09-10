using BasicM6502.Core;
using BasicM6502.Hardware;

namespace BasicM6502;

/// <summary>
/// Microsoft BASIC for 6502 Microprocessor - Version 1.1
/// C# implementation of the original 1976-1978 Microsoft BASIC interpreter
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Microsoft BASIC for 6502 - C# Implementation");
        Console.WriteLine("Version 1.1 - Originally by Microsoft Corporation 1976-1978");
        Console.WriteLine("C# Port - 2024");
        Console.WriteLine();

        // Initialize the system with default Apple II configuration
        var platformConfig = PlatformConfiguration.AppleII;
        var processor = new M6502Processor();
        var interpreter = new BasicInterpreter(processor, platformConfig);

        Console.WriteLine($"Ready - {platformConfig.Name} Configuration");
        Console.WriteLine($"Memory: {platformConfig.RamSize} bytes");
        Console.WriteLine();

        // Start the BASIC interpreter
        interpreter.Run();
    }
}
