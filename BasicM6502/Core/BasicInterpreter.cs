using BasicM6502.Hardware;
using BasicM6502.IO;

namespace BasicM6502.Core;

/// <summary>
/// Main BASIC interpreter class that orchestrates the execution
/// Based on the original Microsoft BASIC 1.1 for 6502
/// </summary>
public class BasicInterpreter
{
    private readonly M6502Processor _processor;
    private readonly PlatformConfiguration _config;
    private readonly IOHandler _ioHandler;
    private readonly Dictionary<string, BasicCommand> _commands;
    private bool _running = true;

    public BasicInterpreter(M6502Processor processor, PlatformConfiguration config)
    {
        _processor = processor;
        _config = config;
        _ioHandler = new ConsoleIOHandler(config);
        
        // Initialize command dispatch table
        _commands = new Dictionary<string, BasicCommand>
        {
            ["LIST"] = new ListCommand(),
            ["RUN"] = new RunCommand(), 
            ["NEW"] = new NewCommand(),
            ["LOAD"] = new LoadCommand(),
            ["SAVE"] = new SaveCommand(),
            ["PRINT"] = new PrintCommand(),
            ["LET"] = new LetCommand(),
            ["IF"] = new IfCommand(),
            ["FOR"] = new ForCommand(),
            ["NEXT"] = new NextCommand(),
            ["GOTO"] = new GotoCommand(),
            ["GOSUB"] = new GosubCommand(),
            ["RETURN"] = new ReturnCommand(),
            ["END"] = new EndCommand(),
            ["STOP"] = new StopCommand(),
            ["QUIT"] = new QuitCommand(this),
            ["EXIT"] = new QuitCommand(this)
        };
    }

    /// <summary>
    /// Main interpreter loop - equivalent to the original READY routine
    /// </summary>
    public void Run()
    {
        _processor.InitializeForBasic(_config);
        PrintWelcome();

        while (_running)
        {
            try
            {
                _ioHandler.Print("READY\r\n");
                string? input = _ioHandler.ReadLine();
                
                // Handle EOF condition (null input)
                if (input == null)
                {
                    _ioHandler.Print("\r\nBYE\r\n");
                    break;
                }
                
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                ProcessInput(input.Trim());
            }
            catch (Exception ex)
            {
                _ioHandler.Print($"ERROR: {ex.Message}\r\n");
            }
        }
    }

    /// <summary>
    /// Process user input - either immediate command or program line
    /// </summary>
    private void ProcessInput(string input)
    {
        // Check if line starts with a number (program line)
        if (char.IsDigit(input[0]))
        {
            ProcessProgramLine(input);
        }
        else
        {
            ProcessCommand(input);
        }
    }

    /// <summary>
    /// Process a numbered program line
    /// </summary>
    private void ProcessProgramLine(string input)
    {
        // Parse line number
        int spaceIndex = input.IndexOf(' ');
        if (spaceIndex == -1)
        {
            // Line number only - delete line
            if (int.TryParse(input, out int lineNum))
            {
                DeleteProgramLine(lineNum);
            }
        }
        else
        {
            // Line number with content - add/replace line
            string lineNumStr = input[..spaceIndex];
            string content = input[(spaceIndex + 1)..];
            
            if (int.TryParse(lineNumStr, out int lineNum))
            {
                StoreProgramLine(lineNum, content);
            }
        }
    }

    /// <summary>
    /// Process an immediate command
    /// </summary>
    private void ProcessCommand(string input)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        string command = parts[0].ToUpper();
        
        if (_commands.TryGetValue(command, out BasicCommand? cmd))
        {
            var context = new CommandContext
            {
                Processor = _processor,
                IOHandler = _ioHandler,
                Config = _config,
                Arguments = parts.Skip(1).ToArray()
            };
            
            cmd.Execute(context);
        }
        else
        {
            _ioHandler.Print("SYNTAX ERROR\r\n");
        }
    }

    private void PrintWelcome()
    {
        _ioHandler.Print("MICROSOFT BASIC VERSION 1.1\r\n");
        _ioHandler.Print("COPYRIGHT 1976-1978 BY MICROSOFT\r\n");
        _ioHandler.Print($"{_processor.GetMemory().Length / 1024}K BYTES FREE\r\n");
        _ioHandler.Print("\r\n");
    }

    private void StoreProgramLine(int lineNumber, string content)
    {
        // Simplified program storage - in real implementation this would
        // store in the tokenized format like the original
        _ioHandler.Print($"Line {lineNumber} stored\r\n");
    }

    private void DeleteProgramLine(int lineNumber)
    {
        // Simplified line deletion
        _ioHandler.Print($"Line {lineNumber} deleted\r\n");
    }

    /// <summary>
    /// Stop the interpreter
    /// </summary>
    public void Stop()
    {
        _running = false;
    }
}

/// <summary>
/// Context passed to BASIC commands
/// </summary>
public class CommandContext
{
    public M6502Processor Processor { get; init; } = null!;
    public IOHandler IOHandler { get; init; } = null!;
    public PlatformConfiguration Config { get; init; } = null!;
    public string[] Arguments { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Base class for BASIC commands
/// </summary>
public abstract class BasicCommand
{
    public abstract void Execute(CommandContext context);
}