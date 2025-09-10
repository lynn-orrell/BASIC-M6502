using BasicM6502.Hardware;

namespace BasicM6502.IO;

/// <summary>
/// Abstract base class for handling input/output operations
/// Based on the platform-specific I/O routines in the original assembly code
/// </summary>
public abstract class IOHandler
{
    protected PlatformConfiguration Config { get; }

    protected IOHandler(PlatformConfiguration config)
    {
        Config = config;
    }

    /// <summary>
    /// Print a string to output
    /// </summary>
    public abstract void Print(string text);

    /// <summary>
    /// Print a single character
    /// </summary>
    public abstract void PrintChar(char c);

    /// <summary>
    /// Read a line of input
    /// </summary>
    public abstract string? ReadLine();

    /// <summary>
    /// Read a single character
    /// </summary>
    public abstract char ReadChar();

    /// <summary>
    /// Check if input is available
    /// </summary>
    public abstract bool InputAvailable();

    /// <summary>
    /// Clear the screen (if supported)
    /// </summary>
    public virtual void ClearScreen()
    {
        // Default implementation - most platforms don't support this
    }
}

/// <summary>
/// Console-based I/O handler for modern systems
/// </summary>
public class ConsoleIOHandler : IOHandler
{
    private int _column = 0;

    public ConsoleIOHandler(PlatformConfiguration config) : base(config)
    {
    }

    public override void Print(string text)
    {
        foreach (char c in text)
        {
            PrintChar(c);
        }
    }

    public override void PrintChar(char c)
    {
        if (c == '\r')
        {
            _column = 0;
            return;
        }
        
        if (c == '\n')
        {
            Console.WriteLine();
            _column = 0;
            return;
        }

        // Handle line wrapping based on terminal width
        if (_column >= Config.TerminalWidth && c != ' ')
        {
            Console.WriteLine();
            _column = 0;
        }

        Console.Write(c);
        _column++;
    }

    public override string? ReadLine()
    {
        return Console.ReadLine();
    }

    public override char ReadChar()
    {
        ConsoleKeyInfo key = Console.ReadKey(true);
        return key.KeyChar;
    }

    public override bool InputAvailable()
    {
        return Console.KeyAvailable;
    }

    public override void ClearScreen()
    {
        Console.Clear();
        _column = 0;
    }
}