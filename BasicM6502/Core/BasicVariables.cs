using System;
using System.Collections.Generic;

namespace BasicM6502.Core;

/// <summary>
/// Manages BASIC variables (numeric and string)
/// Based on the variable storage format from the original assembly code
/// </summary>
public class BasicVariables
{
    private readonly Dictionary<string, BasicValue> _variables = new();

    /// <summary>
    /// Set a variable value
    /// </summary>
    public void SetVariable(string name, BasicValue value)
    {
        _variables[NormalizeName(name)] = value;
    }

    /// <summary>
    /// Get a variable value
    /// </summary>
    public BasicValue GetVariable(string name)
    {
        string normalizedName = NormalizeName(name);
        return _variables.TryGetValue(normalizedName, out var value) ? value : BasicValue.Zero;
    }

    /// <summary>
    /// Check if a variable exists
    /// </summary>
    public bool HasVariable(string name)
    {
        return _variables.ContainsKey(NormalizeName(name));
    }

    /// <summary>
    /// Clear all variables
    /// </summary>
    public void Clear()
    {
        _variables.Clear();
    }

    /// <summary>
    /// Get all variable names
    /// </summary>
    public IEnumerable<string> GetVariableNames()
    {
        return _variables.Keys;
    }

    /// <summary>
    /// Normalize variable name (uppercase, handle string suffix)
    /// </summary>
    private static string NormalizeName(string name)
    {
        return name.Trim().ToUpperInvariant();
    }
}

/// <summary>
/// Represents a BASIC value (numeric or string)
/// </summary>
public class BasicValue
{
    public static readonly BasicValue Zero = new BasicValue(0.0);
    public static readonly BasicValue EmptyString = new BasicValue("");

    public double NumericValue { get; }
    public string StringValue { get; }
    public bool IsString { get; }

    public BasicValue(double value)
    {
        NumericValue = value;
        StringValue = value.ToString();
        IsString = false;
    }

    public BasicValue(string value)
    {
        StringValue = value ?? "";
        NumericValue = double.TryParse(StringValue, out double num) ? num : 0.0;
        IsString = true;
    }

    public BasicValue(int value) : this((double)value) { }

    /// <summary>
    /// Convert to string representation
    /// </summary>
    public override string ToString()
    {
        if (IsString)
            return StringValue;
        
        // Format numbers like the original BASIC
        if (NumericValue == Math.Floor(NumericValue) && NumericValue >= int.MinValue && NumericValue <= int.MaxValue)
        {
            return ((int)NumericValue).ToString();
        }
        return NumericValue.ToString("G");
    }

    /// <summary>
    /// Convert to boolean for conditions
    /// </summary>
    public bool ToBoolean()
    {
        if (IsString)
            return !string.IsNullOrEmpty(StringValue);
        return NumericValue != 0.0;
    }

    /// <summary>
    /// Addition operator
    /// </summary>
    public static BasicValue operator +(BasicValue left, BasicValue right)
    {
        if (left.IsString || right.IsString)
        {
            return new BasicValue(left.StringValue + right.StringValue);
        }
        return new BasicValue(left.NumericValue + right.NumericValue);
    }

    /// <summary>
    /// Subtraction operator
    /// </summary>
    public static BasicValue operator -(BasicValue left, BasicValue right)
    {
        return new BasicValue(left.NumericValue - right.NumericValue);
    }

    /// <summary>
    /// Multiplication operator
    /// </summary>
    public static BasicValue operator *(BasicValue left, BasicValue right)
    {
        return new BasicValue(left.NumericValue * right.NumericValue);
    }

    /// <summary>
    /// Division operator
    /// </summary>
    public static BasicValue operator /(BasicValue left, BasicValue right)
    {
        if (right.NumericValue == 0.0)
            throw new BasicRuntimeException("Division by zero");
        return new BasicValue(left.NumericValue / right.NumericValue);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static BasicValue operator ==(BasicValue left, BasicValue right)
    {
        if (left.IsString && right.IsString)
            return new BasicValue(left.StringValue == right.StringValue ? 1 : 0);
        return new BasicValue(Math.Abs(left.NumericValue - right.NumericValue) < 1e-10 ? 1 : 0);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static BasicValue operator !=(BasicValue left, BasicValue right)
    {
        return new BasicValue((left == right).NumericValue == 0 ? 1 : 0);
    }

    /// <summary>
    /// Less than operator
    /// </summary>
    public static BasicValue operator <(BasicValue left, BasicValue right)
    {
        if (left.IsString && right.IsString)
            return new BasicValue(string.Compare(left.StringValue, right.StringValue) < 0 ? 1 : 0);
        return new BasicValue(left.NumericValue < right.NumericValue ? 1 : 0);
    }

    /// <summary>
    /// Greater than operator
    /// </summary>
    public static BasicValue operator >(BasicValue left, BasicValue right)
    {
        if (left.IsString && right.IsString)
            return new BasicValue(string.Compare(left.StringValue, right.StringValue) > 0 ? 1 : 0);
        return new BasicValue(left.NumericValue > right.NumericValue ? 1 : 0);
    }

    /// <summary>
    /// Less than or equal operator
    /// </summary>
    public static BasicValue operator <=(BasicValue left, BasicValue right)
    {
        return new BasicValue((left < right).NumericValue == 1 || (left == right).NumericValue == 1 ? 1 : 0);
    }

    /// <summary>
    /// Greater than or equal operator
    /// </summary>
    public static BasicValue operator >=(BasicValue left, BasicValue right)
    {
        return new BasicValue((left > right).NumericValue == 1 || (left == right).NumericValue == 1 ? 1 : 0);
    }

    public override bool Equals(object? obj)
    {
        if (obj is BasicValue other)
        {
            return (this == other).NumericValue == 1;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return IsString ? StringValue.GetHashCode() : NumericValue.GetHashCode();
    }
}

/// <summary>
/// Exception thrown during BASIC program execution
/// </summary>
public class BasicRuntimeException : Exception
{
    public BasicRuntimeException(string message) : base(message) { }
    public BasicRuntimeException(string message, Exception innerException) : base(message, innerException) { }
}