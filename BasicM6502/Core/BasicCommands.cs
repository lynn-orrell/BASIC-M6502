using BasicM6502.Core;
using BasicM6502.IO;

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
    private readonly BasicVariables _variables;
    private readonly ExpressionEvaluator _evaluator;
    private readonly ForLoopManager _forLoopManager;

    public ForCommand(BasicVariables variables, ExpressionEvaluator evaluator, ForLoopManager forLoopManager)
    {
        _variables = variables;
        _evaluator = evaluator;
        _forLoopManager = forLoopManager;
    }

    public override void Execute(CommandContext context)
    {
        // FOR variable = start TO end [STEP step]
        if (context.Arguments.Length < 4)
        {
            throw new BasicRuntimeException("FOR requires variable = start TO end");
        }

        string variable = context.Arguments[0];
        
        // Expected format: variable = start TO end [STEP step]
        if (context.Arguments.Length < 4 || context.Arguments[1] != "=")
        {
            throw new BasicRuntimeException("FOR syntax: variable = start TO end [STEP step]");
        }
        
        // Find TO keyword
        int toIndex = -1;
        for (int i = 2; i < context.Arguments.Length; i++)
        {
            if (context.Arguments[i].ToUpper() == "TO")
            {
                toIndex = i;
                break;
            }
        }

        if (toIndex == -1)
        {
            throw new BasicRuntimeException("FOR missing TO keyword");
        }

        // Parse start value (between = and TO)
        string startExpr = string.Join(" ", context.Arguments.Skip(2).Take(toIndex - 2));
        double startValue = _evaluator.Evaluate(startExpr).NumericValue;

        // Find STEP keyword or use default step of 1
        int stepIndex = -1;
        for (int i = toIndex + 1; i < context.Arguments.Length; i++)
        {
            if (context.Arguments[i].ToUpper() == "STEP")
            {
                stepIndex = i;
                break;
            }
        }

        double endValue, step = 1.0;
        
        if (stepIndex == -1)
        {
            // No STEP - end value goes from TO to end
            string endExpr = string.Join(" ", context.Arguments.Skip(toIndex + 1));
            endValue = _evaluator.Evaluate(endExpr).NumericValue;
        }
        else
        {
            // STEP present - end value is between TO and STEP
            string endExpr = string.Join(" ", context.Arguments.Skip(toIndex + 1).Take(stepIndex - toIndex - 1));
            endValue = _evaluator.Evaluate(endExpr).NumericValue;
            
            // Step value comes after STEP
            string stepExpr = string.Join(" ", context.Arguments.Skip(stepIndex + 1));
            step = _evaluator.Evaluate(stepExpr).NumericValue;
        }

        // Set initial value
        _variables.SetVariable(variable, new BasicValue(startValue));

        // Register the loop
        if (context.LineNumber.HasValue)
        {
            _forLoopManager.StartLoop(variable, startValue, endValue, step, context.LineNumber.Value);
        }
    }
}

public class NextCommand : BasicCommand
{
    private readonly BasicVariables _variables;
    private readonly ForLoopManager _forLoopManager;
    private readonly BasicInterpreter _interpreter;

    public NextCommand(BasicVariables variables, ForLoopManager forLoopManager, BasicInterpreter interpreter)
    {
        _variables = variables;
        _forLoopManager = forLoopManager;
        _interpreter = interpreter;
    }

    public override void Execute(CommandContext context)
    {
        // NEXT [variable]
        string? variable = context.Arguments.Length > 0 ? context.Arguments[0] : null;
        
        var result = _forLoopManager.ProcessNext(variable, _variables);
        
        if (result.ShouldContinue)
        {
            _interpreter.JumpToLine(result.ReturnToLine);
        }
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

// Additional BASIC commands

/// <summary>
/// DIM command - dimensions arrays (simplified implementation)
/// </summary>
public class DimCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        // Simplified DIM - just acknowledge the command
        // Real implementation would allocate array space
        context.IOHandler.Print("DIM processed (arrays not yet fully implemented)\r\n");
    }
}

/// <summary>
/// READ command - reads data from DATA statements
/// </summary>
public class ReadCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("READ not yet implemented\r\n");
    }
}

/// <summary>
/// DATA command - defines data for READ statements
/// </summary>
public class DataCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        // DATA statements are processed during program parsing, not execution
        // So this should generally not be called during normal execution
    }
}

/// <summary>
/// INPUT command - gets input from user
/// </summary>
public class InputCommand : BasicCommand
{
    private readonly BasicVariables _variables;
    private readonly IOHandler _ioHandler;

    public InputCommand(BasicVariables variables, IOHandler ioHandler)
    {
        _variables = variables;
        _ioHandler = ioHandler;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            throw new BasicRuntimeException("INPUT requires a variable name");
        }

        string varName = context.Arguments[0];
        
        // Handle optional prompt
        string prompt = "? ";
        if (context.Arguments.Length > 1)
        {
            // Look for quoted prompt
            string fullArgs = string.Join(" ", context.Arguments);
            if (fullArgs.Contains("\""))
            {
                int firstQuote = fullArgs.IndexOf('"');
                int lastQuote = fullArgs.LastIndexOf('"');
                if (firstQuote != lastQuote)
                {
                    prompt = fullArgs.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                }
            }
        }

        context.IOHandler.Print(prompt);
        string? input = context.IOHandler.ReadLine();
        
        if (input == null) input = "";

        // Try to parse as number, otherwise store as string
        if (double.TryParse(input, out double numValue))
        {
            _variables.SetVariable(varName, new BasicValue(numValue));
        }
        else
        {
            _variables.SetVariable(varName, new BasicValue(input));
        }
    }
}

/// <summary>
/// REM command - remark/comment (does nothing)
/// </summary>
public class RemCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        // Comments do nothing during execution
    }
}

/// <summary>
/// CLEAR command - clears variables but keeps program
/// </summary>
public class ClearCommand : BasicCommand
{
    private readonly BasicVariables _variables;

    public ClearCommand(BasicVariables variables)
    {
        _variables = variables;
    }

    public override void Execute(CommandContext context)
    {
        _variables.Clear();
        context.IOHandler.Print("VARIABLES CLEARED\r\n");
    }
}
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