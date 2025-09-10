using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicM6502.Core;

/// <summary>
/// Evaluates BASIC expressions
/// Based on the expression evaluator from the original assembly code
/// </summary>
public class ExpressionEvaluator
{
    private readonly BasicVariables _variables;

    public ExpressionEvaluator(BasicVariables variables)
    {
        _variables = variables;
    }

    /// <summary>
    /// Evaluate a BASIC expression and return the result
    /// </summary>
    public BasicValue Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return BasicValue.Zero;

        var tokens = TokenizeExpression(expression.Trim());
        return EvaluateTokens(tokens);
    }

    /// <summary>
    /// Tokenize an expression into components
    /// </summary>
    private List<string> TokenizeExpression(string expression)
    {
        var tokens = new List<string>();
        var current = "";
        bool inQuotes = false;

        for (int i = 0; i < expression.Length; i++)
        {
            char c = expression[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                current += c;
            }
            else if (inQuotes)
            {
                current += c;
            }
            else if ("+-*/=<>()".Contains(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = "";
                }
                
                // Handle multi-character operators
                if (i < expression.Length - 1)
                {
                    string twoChar = expression.Substring(i, 2);
                    if (twoChar == "<=" || twoChar == ">=" || twoChar == "<>")
                    {
                        tokens.Add(twoChar);
                        i++; // Skip next character
                        continue;
                    }
                }
                
                tokens.Add(c.ToString());
            }
            else if (c == ' ')
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = "";
                }
            }
            else
            {
                current += c;
            }
        }

        if (current.Length > 0)
        {
            tokens.Add(current);
        }

        return tokens;
    }

    /// <summary>
    /// Evaluate tokenized expression using recursive descent parser
    /// </summary>
    private BasicValue EvaluateTokens(List<string> tokens)
    {
        if (tokens.Count == 0)
            return BasicValue.Zero;

        // Single token cases
        if (tokens.Count == 1)
        {
            return EvaluateToken(tokens[0]);
        }

        // Handle parentheses first
        tokens = ResolveParentheses(tokens);

        // Handle operators by precedence
        // Level 1: = <> < > <= >=
        for (int i = tokens.Count - 2; i >= 1; i -= 2)
        {
            string op = tokens[i];
            if (op == "=" || op == "<>" || op == "<" || op == ">" || op == "<=" || op == ">=")
            {
                var left = EvaluateTokens(tokens.Take(i).ToList());
                var right = EvaluateTokens(tokens.Skip(i + 1).ToList());
                return ApplyOperator(left, op, right);
            }
        }

        // Level 2: + -
        for (int i = tokens.Count - 2; i >= 1; i -= 2)
        {
            string op = tokens[i];
            if (op == "+" || op == "-")
            {
                var left = EvaluateTokens(tokens.Take(i).ToList());
                var right = EvaluateTokens(tokens.Skip(i + 1).ToList());
                return ApplyOperator(left, op, right);
            }
        }

        // Level 3: * /
        for (int i = tokens.Count - 2; i >= 1; i -= 2)
        {
            string op = tokens[i];
            if (op == "*" || op == "/")
            {
                var left = EvaluateTokens(tokens.Take(i).ToList());
                var right = EvaluateTokens(tokens.Skip(i + 1).ToList());
                return ApplyOperator(left, op, right);
            }
        }

        // If we get here, evaluate the first token
        return EvaluateToken(tokens[0]);
    }

    /// <summary>
    /// Resolve parentheses in the token list
    /// </summary>
    private List<string> ResolveParentheses(List<string> tokens)
    {
        while (true)
        {
            int openIndex = -1;
            int closeIndex = -1;

            // Find the innermost parentheses
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == "(")
                {
                    openIndex = i;
                }
                else if (tokens[i] == ")" && openIndex != -1)
                {
                    closeIndex = i;
                    break;
                }
            }

            if (openIndex == -1 || closeIndex == -1)
                break;

            // Evaluate the expression inside parentheses
            var innerTokens = tokens.Skip(openIndex + 1).Take(closeIndex - openIndex - 1).ToList();
            var result = EvaluateTokens(innerTokens);

            // Replace the parenthetical expression with its result
            var newTokens = tokens.Take(openIndex).ToList();
            newTokens.Add(result.ToString());
            newTokens.AddRange(tokens.Skip(closeIndex + 1));
            tokens = newTokens;
        }

        return tokens;
    }

    /// <summary>
    /// Evaluate a single token (number, string, variable)
    /// </summary>
    private BasicValue EvaluateToken(string token)
    {
        // String literal
        if (token.StartsWith("\"") && token.EndsWith("\""))
        {
            return new BasicValue(token[1..^1]); // Remove quotes
        }

        // Numeric literal
        if (double.TryParse(token, out double numValue))
        {
            return new BasicValue(numValue);
        }

        // Variable
        return _variables.GetVariable(token);
    }

    /// <summary>
    /// Apply an operator to two values
    /// </summary>
    private BasicValue ApplyOperator(BasicValue left, string op, BasicValue right)
    {
        return op switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" => left / right,
            "=" => left == right,
            "<>" => left != right,
            "<" => left < right,
            ">" => left > right,
            "<=" => left <= right,
            ">=" => left >= right,
            _ => throw new BasicRuntimeException($"Unknown operator: {op}")
        };
    }
}