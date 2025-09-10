using System.Collections.Generic;
using System.Linq;

namespace BasicM6502.Core;

/// <summary>
/// Represents a BASIC program with numbered lines
/// Based on the program storage format from the original assembly code
/// </summary>
public class BasicProgram
{
    private readonly SortedDictionary<int, BasicProgramLine> _lines = new();

    /// <summary>
    /// Store or update a program line
    /// </summary>
    public void StoreLine(int lineNumber, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            // Empty content - delete the line
            _lines.Remove(lineNumber);
        }
        else
        {
            _lines[lineNumber] = new BasicProgramLine(lineNumber, content);
        }
    }

    /// <summary>
    /// Delete a program line
    /// </summary>
    public void DeleteLine(int lineNumber)
    {
        _lines.Remove(lineNumber);
    }

    /// <summary>
    /// Get a program line by line number
    /// </summary>
    public BasicProgramLine? GetLine(int lineNumber)
    {
        return _lines.TryGetValue(lineNumber, out var line) ? line : null;
    }

    /// <summary>
    /// Get all program lines in order
    /// </summary>
    public IEnumerable<BasicProgramLine> GetAllLines()
    {
        return _lines.Values;
    }

    /// <summary>
    /// Find the next line number after the given line number
    /// </summary>
    public int? FindNextLineNumber(int currentLineNumber)
    {
        foreach (var lineNum in _lines.Keys)
        {
            if (lineNum > currentLineNumber)
                return lineNum;
        }
        return null;
    }

    /// <summary>
    /// Clear all program lines
    /// </summary>
    public void Clear()
    {
        _lines.Clear();
    }

    /// <summary>
    /// Check if program is empty
    /// </summary>
    public bool IsEmpty => _lines.Count == 0;

    /// <summary>
    /// Get the first line number in the program
    /// </summary>
    public int? FirstLineNumber => _lines.Count > 0 ? _lines.Keys.First() : null;
}

/// <summary>
/// Represents a single line of BASIC program code
/// </summary>
public class BasicProgramLine
{
    public int LineNumber { get; }
    public string Content { get; }
    public string[] Tokens { get; }

    public BasicProgramLine(int lineNumber, string content)
    {
        LineNumber = lineNumber;
        Content = content;
        // Basic tokenization - split on spaces but preserve quoted strings
        Tokens = TokenizeBasicLine(content);
    }

    private static string[] TokenizeBasicLine(string content)
    {
        var tokens = new List<string>();
        var current = "";
        bool inQuotes = false;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current += c;
            }
            else if (c == ' ' && !inQuotes)
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

        return tokens.ToArray();
    }
}