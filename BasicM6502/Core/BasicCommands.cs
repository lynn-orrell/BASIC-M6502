using BasicM6502.Core;

namespace BasicM6502.Core;

/// <summary>
/// LIST command - displays program lines
/// Based on the LIST routine from the original assembly code
/// </summary>
public class ListCommand : BasicCommand
{
    private readonly BasicProgram _program;

    public ListCommand(BasicProgram program)
    {
        _program = program;
    }

    public override void Execute(CommandContext context)
    {
        if (_program.IsEmpty)
        {
            context.IOHandler.Print("NO PROGRAM\r\n");
            return;
        }

        foreach (var line in _program.GetAllLines())
        {
            context.IOHandler.Print($"{line.LineNumber} {line.Content}\r\n");
        }
    }
}

/// <summary>
/// RUN command - executes the program
/// </summary>
public class RunCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;

    public RunCommand(BasicInterpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        _interpreter.RunProgram();
    }
}

/// <summary>
/// NEW command - clears the program
/// </summary>
public class NewCommand : BasicCommand
{
    private readonly BasicProgram _program;
    private readonly BasicVariables _variables;

    public NewCommand(BasicProgram program, BasicVariables variables)
    {
        _program = program;
        _variables = variables;
    }

    public override void Execute(CommandContext context)
    {
        _program.Clear();
        _variables.Clear();
        context.IOHandler.Print("NEW PROGRAM\r\n");
    }
}

/// <summary>
/// PRINT command - outputs values
/// Based on the PRINT routine from the original assembly code
/// </summary>
public class PrintCommand : BasicCommand
{
    private readonly ExpressionEvaluator _evaluator;

    public PrintCommand(ExpressionEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            context.IOHandler.Print("\r\n");
            return;
        }

        // Join arguments back into expression and evaluate
        string expression = string.Join(" ", context.Arguments);
        
        try
        {
            var result = _evaluator.Evaluate(expression);
            context.IOHandler.Print(result.ToString() + "\r\n");
        }
        catch (Exception ex)
        {
            throw new BasicRuntimeException($"Error in PRINT statement: {ex.Message}");
        }
    }
}

/// <summary>
/// LET command - assigns variables (implicit in BASIC)
/// </summary>
public class LetCommand : BasicCommand
{
    private readonly BasicVariables _variables;
    private readonly ExpressionEvaluator _evaluator;

    public LetCommand(BasicVariables variables, ExpressionEvaluator evaluator)
    {
        _variables = variables;
        _evaluator = evaluator;
    }

    public override void Execute(CommandContext context)
    {
        // Handle both "LET X = 5" and "X = 5" formats
        string[] args = context.Arguments;
        
        // If first argument is "LET", skip it
        if (args.Length > 0 && args[0].ToUpper() == "LET")
        {
            args = args.Skip(1).ToArray();
        }

        if (args.Length < 3)
        {
            throw new BasicRuntimeException("Syntax error in assignment");
        }

        // Find the equals sign
        int equalsIndex = -1;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "=")
            {
                equalsIndex = i;
                break;
            }
        }

        if (equalsIndex == -1 || equalsIndex == 0)
        {
            throw new BasicRuntimeException("Syntax error in assignment");
        }

        string varName = args[0];
        string expression = string.Join(" ", args.Skip(equalsIndex + 1));

        try
        {
            var value = _evaluator.Evaluate(expression);
            _variables.SetVariable(varName, value);
        }
        catch (Exception ex)
        {
            throw new BasicRuntimeException($"Error in assignment: {ex.Message}");
        }
    }
}

/// <summary>
/// END command - terminates program execution
/// </summary>
public class EndCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;

    public EndCommand(BasicInterpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("END\r\n");
        _interpreter.StopExecution();
    }
}

/// <summary>
/// STOP command - stops program execution
/// </summary>
public class StopCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;

    public StopCommand(BasicInterpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("STOP\r\n");
        _interpreter.StopExecution();
    }
}

// Placeholder commands for other BASIC statements
public class LoadCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("LOAD not yet implemented\r\n");
    }
}

public class SaveCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("SAVE not yet implemented\r\n");
    }
}

public class IfCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;
    private readonly ExpressionEvaluator _evaluator;

    public IfCommand(BasicInterpreter interpreter, ExpressionEvaluator evaluator)
    {
        _interpreter = interpreter;
        _evaluator = evaluator;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length < 3)
        {
            throw new BasicRuntimeException("IF statement requires condition and THEN");
        }

        // Find THEN keyword
        int thenIndex = -1;
        for (int i = 0; i < context.Arguments.Length; i++)
        {
            if (context.Arguments[i].ToUpper() == "THEN")
            {
                thenIndex = i;
                break;
            }
        }

        if (thenIndex == -1)
        {
            throw new BasicRuntimeException("IF statement missing THEN");
        }

        // Evaluate condition
        string condition = string.Join(" ", context.Arguments.Take(thenIndex));
        var result = _evaluator.Evaluate(condition);

        // If condition is true, execute the THEN part
        if (result.ToBoolean())
        {
            string[] thenPart = context.Arguments.Skip(thenIndex + 1).ToArray();
            if (thenPart.Length > 0)
            {
                // If it's just a line number, GOTO it
                if (thenPart.Length == 1 && int.TryParse(thenPart[0], out int lineNum))
                {
                    _interpreter.JumpToLine(lineNum);
                }
                else
                {
                    // Execute the statement after THEN
                    // This is a simplified implementation - real BASIC would need full statement parsing
                    throw new BasicRuntimeException("Complex THEN statements not yet supported");
                }
            }
        }
    }
}

public class ForCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("FOR loop not yet implemented\r\n");
    }
}

public class NextCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("NEXT not yet implemented\r\n");
    }
}

public class GotoCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;

    public GotoCommand(BasicInterpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            throw new BasicRuntimeException("GOTO requires a line number");
        }

        if (!int.TryParse(context.Arguments[0], out int targetLine))
        {
            throw new BasicRuntimeException("Invalid line number in GOTO");
        }

        _interpreter.JumpToLine(targetLine);
    }
}

public class GosubCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;

    public GosubCommand(BasicInterpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            throw new BasicRuntimeException("GOSUB requires a line number");
        }

        if (!int.TryParse(context.Arguments[0], out int targetLine))
        {
            throw new BasicRuntimeException("Invalid line number in GOSUB");
        }

        // Push current line number for RETURN
        if (context.LineNumber.HasValue)
        {
            _interpreter.PushReturn(context.LineNumber.Value);
        }

        _interpreter.JumpToLine(targetLine);
    }
}

public class ReturnCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;

    public ReturnCommand(BasicInterpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        int returnLine = _interpreter.PopReturn();
        
        // Find the next line after the GOSUB
        var nextLine = _interpreter.Program.FindNextLineNumber(returnLine);
        if (nextLine.HasValue)
        {
            _interpreter.JumpToLine(nextLine.Value);
        }
        else
        {
            _interpreter.StopExecution();
        }
    }
}

/// <summary>
/// QUIT/EXIT command - terminates the interpreter
/// </summary>
public class QuitCommand : BasicCommand
{
    private readonly BasicInterpreter _interpreter;

    public QuitCommand(BasicInterpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("BYE\r\n");
        _interpreter.Stop();
    }
}