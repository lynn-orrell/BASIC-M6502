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
    private readonly BasicProgram _program;
    private readonly BasicVariables _variables;
    private readonly ExpressionEvaluator _evaluator;
    private bool _running = true;

    // Program execution state
    private int? _currentLineNumber = null;
    private readonly Stack<int> _returnStack = new(); // For GOSUB/RETURN

    public BasicInterpreter(M6502Processor processor, PlatformConfiguration config)
    {
        _processor = processor;
        _config = config;
        _ioHandler = new ConsoleIOHandler(config);
        _program = new BasicProgram();
        _variables = new BasicVariables();
        _evaluator = new ExpressionEvaluator(_variables);
        
        // Initialize command dispatch table
        _commands = new Dictionary<string, BasicCommand>
        {
            ["LIST"] = new ListCommand(_program),
            ["RUN"] = new RunCommand(this), 
            ["NEW"] = new NewCommand(_program, _variables),
            ["LOAD"] = new LoadCommand(),
            ["SAVE"] = new SaveCommand(),
            ["PRINT"] = new PrintCommand(_evaluator),
            ["LET"] = new LetCommand(_variables, _evaluator),
            ["IF"] = new IfCommand(this, _evaluator),
            ["FOR"] = new ForCommand(),
            ["NEXT"] = new NextCommand(),
            ["GOTO"] = new GotoCommand(this),
            ["GOSUB"] = new GosubCommand(this),
            ["RETURN"] = new ReturnCommand(this),
            ["END"] = new EndCommand(this),
            ["STOP"] = new StopCommand(this),
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
                _program.DeleteLine(lineNum);
                _ioHandler.Print($"Line {lineNum} deleted\r\n");
            }
        }
        else
        {
            // Line number with content - add/replace line
            string lineNumStr = input[..spaceIndex];
            string content = input[(spaceIndex + 1)..];
            
            if (int.TryParse(lineNumStr, out int lineNum))
            {
                _program.StoreLine(lineNum, content);
                _ioHandler.Print($"Line {lineNum} stored\r\n");
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

    /// <summary>
    /// Execute the stored BASIC program
    /// </summary>
    public void RunProgram()
    {
        if (_program.IsEmpty)
        {
            _ioHandler.Print("NO PROGRAM TO RUN\r\n");
            return;
        }

        try
        {
            _currentLineNumber = _program.FirstLineNumber;
            
            while (_currentLineNumber.HasValue)
            {
                var line = _program.GetLine(_currentLineNumber.Value);
                if (line == null)
                    break;

                ExecuteProgramLine(line);
                
                // Move to next line if no jump occurred
                if (_currentLineNumber == line.LineNumber)
                {
                    _currentLineNumber = _program.FindNextLineNumber(_currentLineNumber.Value);
                }
            }
        }
        catch (BasicRuntimeException ex)
        {
            _ioHandler.Print($"ERROR AT LINE {_currentLineNumber}: {ex.Message}\r\n");
        }
        finally
        {
            _currentLineNumber = null;
        }
    }

    /// <summary>
    /// Execute a single program line
    /// </summary>
    private void ExecuteProgramLine(BasicProgramLine line)
    {
        if (line.Tokens.Length == 0)
            return;

        string command = line.Tokens[0].ToUpper();
        
        if (_commands.TryGetValue(command, out BasicCommand? cmd))
        {
            var context = new CommandContext
            {
                Processor = _processor,
                IOHandler = _ioHandler,
                Config = _config,
                Arguments = line.Tokens.Skip(1).ToArray(),
                LineNumber = line.LineNumber
            };
            
            cmd.Execute(context);
        }
        else
        {
            // Check if it's a variable assignment (implicit LET)
            if (line.Content.Contains("=") && char.IsLetter(line.Tokens[0][0]))
            {
                var letCmd = _commands["LET"];
                var context = new CommandContext
                {
                    Processor = _processor,
                    IOHandler = _ioHandler,
                    Config = _config,
                    Arguments = line.Tokens,
                    LineNumber = line.LineNumber
                };
                letCmd.Execute(context);
            }
            else
            {
                throw new BasicRuntimeException($"Unknown command: {command}");
            }
        }
    }

    /// <summary>
    /// Jump to a specific line number
    /// </summary>
    public void JumpToLine(int lineNumber)
    {
        if (_program.GetLine(lineNumber) == null)
            throw new BasicRuntimeException($"Line {lineNumber} does not exist");
        _currentLineNumber = lineNumber;
    }

    /// <summary>
    /// Push return address for GOSUB
    /// </summary>
    public void PushReturn(int lineNumber)
    {
        _returnStack.Push(lineNumber);
    }

    /// <summary>
    /// Pop return address for RETURN
    /// </summary>
    public int PopReturn()
    {
        if (_returnStack.Count == 0)
            throw new BasicRuntimeException("RETURN without GOSUB");
        return _returnStack.Pop();
    }

    /// <summary>
    /// Stop program execution
    /// </summary>
    public void StopExecution()
    {
        _currentLineNumber = null;
    }

    /// <summary>
    /// Stop the interpreter
    /// </summary>
    public void Stop()
    {
        _running = false;
    }

    /// <summary>
    /// Get the program object
    /// </summary>
    public BasicProgram Program => _program;

    /// <summary>
    /// Get the variables object
    /// </summary>
    public BasicVariables Variables => _variables;

    /// <summary>
    /// Get the expression evaluator
    /// </summary>
    public ExpressionEvaluator Evaluator => _evaluator;
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
    public int? LineNumber { get; init; } = null;
}

/// <summary>
/// Base class for BASIC commands
/// </summary>
public abstract class BasicCommand
{
    public abstract void Execute(CommandContext context);
}