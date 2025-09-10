using System;
using System.Collections.Generic;

namespace BasicM6502.Core;

/// <summary>
/// Built-in BASIC functions
/// Based on the function dispatch table from the original assembly code
/// </summary>
public static class BasicFunctions
{
    private static readonly Dictionary<string, Func<BasicValue[], BasicValue>> _functions = new()
    {
        // Numeric functions
        ["ABS"] = ABS,
        ["INT"] = INT,
        ["SGN"] = SGN,
        ["SQR"] = SQR,
        ["RND"] = RND,
        
        // String functions  
        ["LEN"] = LEN,
        ["ASC"] = ASC,
        ["CHR$"] = CHR,
        ["STR$"] = STR,
        ["VAL"] = VAL,
        ["LEFT$"] = LEFT,
        ["RIGHT$"] = RIGHT,
        ["MID$"] = MID,
        
        // System functions
        ["PEEK"] = PEEK,
        ["FRE"] = FRE,
        ["POS"] = POS,
        
        // Math functions (simplified)
        ["SIN"] = SIN,
        ["COS"] = COS,
        ["TAN"] = TAN,
        ["ATN"] = ATN,
        ["LOG"] = LOG,
        ["EXP"] = EXP
    };

    private static Random _random = new Random();

    /// <summary>
    /// Check if a function name exists
    /// </summary>
    public static bool IsFunction(string name)
    {
        return _functions.ContainsKey(name.ToUpperInvariant());
    }

    /// <summary>
    /// Call a built-in function
    /// </summary>
    public static BasicValue CallFunction(string name, BasicValue[] arguments)
    {
        string upperName = name.ToUpperInvariant();
        if (!_functions.TryGetValue(upperName, out var function))
        {
            throw new BasicRuntimeException($"Unknown function: {name}");
        }

        try
        {
            return function(arguments);
        }
        catch (Exception ex)
        {
            throw new BasicRuntimeException($"Error in {name}: {ex.Message}");
        }
    }

    // Numeric functions
    private static BasicValue ABS(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("ABS requires exactly one argument");
        return new BasicValue(Math.Abs(args[0].NumericValue));
    }

    private static BasicValue INT(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("INT requires exactly one argument");
        return new BasicValue(Math.Floor(args[0].NumericValue));
    }

    private static BasicValue SGN(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("SGN requires exactly one argument");
        double val = args[0].NumericValue;
        return new BasicValue(val > 0 ? 1 : val < 0 ? -1 : 0);
    }

    private static BasicValue SQR(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("SQR requires exactly one argument");
        double val = args[0].NumericValue;
        if (val < 0) throw new BasicRuntimeException("Square root of negative number");
        return new BasicValue(Math.Sqrt(val));
    }

    private static BasicValue RND(BasicValue[] args)
    {
        // RND(0) returns last random number, RND(1) returns next random number
        return new BasicValue(_random.NextDouble());
    }

    // String functions
    private static BasicValue LEN(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("LEN requires exactly one argument");
        return new BasicValue(args[0].StringValue.Length);
    }

    private static BasicValue ASC(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("ASC requires exactly one argument");
        string str = args[0].StringValue;
        if (str.Length == 0) throw new BasicRuntimeException("ASC of empty string");
        return new BasicValue((int)str[0]);
    }

    private static BasicValue CHR(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("CHR$ requires exactly one argument");
        int code = (int)args[0].NumericValue;
        if (code < 0 || code > 255) throw new BasicRuntimeException("CHR$ argument out of range");
        return new BasicValue(((char)code).ToString());
    }

    private static BasicValue STR(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("STR$ requires exactly one argument");
        return new BasicValue(args[0].NumericValue.ToString());
    }

    private static BasicValue VAL(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("VAL requires exactly one argument");
        string str = args[0].StringValue.Trim();
        if (double.TryParse(str, out double result))
            return new BasicValue(result);
        return new BasicValue(0);
    }

    private static BasicValue LEFT(BasicValue[] args)
    {
        if (args.Length != 2) throw new BasicRuntimeException("LEFT$ requires exactly two arguments");
        string str = args[0].StringValue;
        int len = (int)args[1].NumericValue;
        if (len < 0) return new BasicValue("");
        if (len >= str.Length) return new BasicValue(str);
        return new BasicValue(str[..len]);
    }

    private static BasicValue RIGHT(BasicValue[] args)
    {
        if (args.Length != 2) throw new BasicRuntimeException("RIGHT$ requires exactly two arguments");
        string str = args[0].StringValue;
        int len = (int)args[1].NumericValue;
        if (len < 0) return new BasicValue("");
        if (len >= str.Length) return new BasicValue(str);
        return new BasicValue(str[^len..]);
    }

    private static BasicValue MID(BasicValue[] args)
    {
        if (args.Length < 2 || args.Length > 3) throw new BasicRuntimeException("MID$ requires 2 or 3 arguments");
        string str = args[0].StringValue;
        int start = (int)args[1].NumericValue - 1; // BASIC uses 1-based indexing
        
        if (start < 0 || start >= str.Length) return new BasicValue("");
        
        if (args.Length == 2)
            return new BasicValue(str[start..]);
        
        int len = (int)args[2].NumericValue;
        if (len <= 0) return new BasicValue("");
        
        int end = Math.Min(start + len, str.Length);
        return new BasicValue(str[start..end]);
    }

    // System functions
    private static BasicValue PEEK(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("PEEK requires exactly one argument");
        // Simplified - always return 0
        return new BasicValue(0);
    }

    private static BasicValue FRE(BasicValue[] args)
    {
        // Return simulated free memory
        return new BasicValue(32768);
    }

    private static BasicValue POS(BasicValue[] args)
    {
        // Return current cursor position (simplified)
        return new BasicValue(0);
    }

    // Math functions (simplified implementations)
    private static BasicValue SIN(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("SIN requires exactly one argument");
        return new BasicValue(Math.Sin(args[0].NumericValue));
    }

    private static BasicValue COS(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("COS requires exactly one argument");
        return new BasicValue(Math.Cos(args[0].NumericValue));
    }

    private static BasicValue TAN(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("TAN requires exactly one argument");
        return new BasicValue(Math.Tan(args[0].NumericValue));
    }

    private static BasicValue ATN(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("ATN requires exactly one argument");
        return new BasicValue(Math.Atan(args[0].NumericValue));
    }

    private static BasicValue LOG(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("LOG requires exactly one argument");
        double val = args[0].NumericValue;
        if (val <= 0) throw new BasicRuntimeException("LOG of non-positive number");
        return new BasicValue(Math.Log(val));
    }

    private static BasicValue EXP(BasicValue[] args)
    {
        if (args.Length != 1) throw new BasicRuntimeException("EXP requires exactly one argument");
        return new BasicValue(Math.Exp(args[0].NumericValue));
    }
}