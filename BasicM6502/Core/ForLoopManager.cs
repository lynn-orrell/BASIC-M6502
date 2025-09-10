using System.Collections.Generic;

namespace BasicM6502.Core;

/// <summary>
/// Manages FOR/NEXT loop state
/// Based on the FOR loop implementation from the original assembly code
/// </summary>
public class ForLoopManager
{
    private readonly Stack<ForLoopContext> _loopStack = new();

    /// <summary>
    /// Start a new FOR loop
    /// </summary>
    public void StartLoop(string variable, double startValue, double endValue, double step, int lineNumber)
    {
        var context = new ForLoopContext
        {
            Variable = variable.ToUpperInvariant(),
            StartValue = startValue,
            EndValue = endValue,
            Step = step,
            LineNumber = lineNumber
        };

        _loopStack.Push(context);
    }

    /// <summary>
    /// Process NEXT statement
    /// </summary>
    public ForLoopResult ProcessNext(string? variable, BasicVariables variables)
    {
        if (_loopStack.Count == 0)
        {
            throw new BasicRuntimeException("NEXT without FOR");
        }

        var context = _loopStack.Peek();

        // If variable specified, it must match current loop
        if (variable != null && !string.Equals(context.Variable, variable.ToUpperInvariant()))
        {
            throw new BasicRuntimeException("NEXT variable mismatch");
        }

        // Get current value and increment
        var currentValue = variables.GetVariable(context.Variable).NumericValue;
        currentValue += context.Step;
        variables.SetVariable(context.Variable, new BasicValue(currentValue));

        // Check if loop should continue
        bool continueLoop = context.Step > 0 ? 
            currentValue <= context.EndValue : 
            currentValue >= context.EndValue;

        if (continueLoop)
        {
            return new ForLoopResult { ShouldContinue = true, ReturnToLine = context.LineNumber };
        }
        else
        {
            _loopStack.Pop(); // Remove completed loop
            return new ForLoopResult { ShouldContinue = false };
        }
    }

    /// <summary>
    /// Clear all loop state
    /// </summary>
    public void Clear()
    {
        _loopStack.Clear();
    }
}

/// <summary>
/// Context for a FOR loop
/// </summary>
public class ForLoopContext
{
    public string Variable { get; set; } = "";
    public double StartValue { get; set; }
    public double EndValue { get; set; }
    public double Step { get; set; } = 1.0;
    public int LineNumber { get; set; }
}

/// <summary>
/// Result of processing a NEXT statement
/// </summary>
public class ForLoopResult
{
    public bool ShouldContinue { get; set; }
    public int ReturnToLine { get; set; }
}