using BasicM6502.Core;

namespace BasicM6502.Core;

/// <summary>
/// LIST command - displays program lines
/// Based on the LIST routine from the original assembly code
/// </summary>
public class ListCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("PROGRAM LISTING:\r\n");
        context.IOHandler.Print("(Program storage not yet implemented)\r\n");
    }
}

/// <summary>
/// RUN command - executes the program
/// </summary>
public class RunCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("RUNNING PROGRAM...\r\n");
        context.IOHandler.Print("(Program execution not yet implemented)\r\n");
    }
}

/// <summary>
/// NEW command - clears the program
/// </summary>
public class NewCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("NEW PROGRAM\r\n");
        // In the real implementation, this would clear program memory
    }
}

/// <summary>
/// PRINT command - outputs values
/// Based on the PRINT routine from the original assembly code
/// </summary>
public class PrintCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            context.IOHandler.Print("\r\n");
            return;
        }

        string output = string.Join(" ", context.Arguments);
        
        // Handle quoted strings
        if (output.StartsWith('"') && output.EndsWith('"'))
        {
            output = output[1..^1]; // Remove quotes
        }
        
        context.IOHandler.Print(output + "\r\n");
    }
}

/// <summary>
/// LET command - assigns variables (implicit in BASIC)
/// </summary>
public class LetCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("LET statement processed\r\n");
        // Variable assignment would be implemented here
    }
}

/// <summary>
/// END command - terminates program execution
/// </summary>
public class EndCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("END\r\n");
    }
}

/// <summary>
/// STOP command - stops program execution
/// </summary>
public class StopCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("STOP\r\n");
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
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("IF statement not yet implemented\r\n");
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
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("GOTO not yet implemented\r\n");
    }
}

public class GosubCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("GOSUB not yet implemented\r\n");
    }
}

public class ReturnCommand : BasicCommand
{
    public override void Execute(CommandContext context)
    {
        context.IOHandler.Print("RETURN not yet implemented\r\n");
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