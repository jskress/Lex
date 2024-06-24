using System.Globalization;

namespace Lex.Tokens;

/// <summary>
/// This class represents a number token.
/// </summary>
public class NumberToken : Token
{
    /// <summary>
    /// This method creates a number token based on the content of the given text.
    /// </summary>
    /// <param name="text">The text to convert to a number token.</param>
    /// <returns>The representative number token.</returns>
    public static NumberToken FromText(string text)
    {
        return text.Contains('.') || text.Contains('e') || text.Contains('E')
            ? new NumberToken(text, double.Parse(text, NumberStyles.Float))
            : new NumberToken(text, long.Parse(text));
    }

    /// <summary>
    /// This property holds the floating point number that the literal token
    /// resolves to.
    /// </summary>
    public long IntegralNumber { get; }

    /// <summary>
    /// This property holds the floating point number that the literal token
    /// resolves to.
    /// </summary>
    public double FloatingPointNumber { get; }

    /// <summary>
    /// This property notes whether the parsed number is an integral number (i.e., no
    /// fractional part) or a floating point number.
    /// </summary>
    public bool IsFloatingPoint { get; }

    public NumberToken(string text, long number) : base(text)
    {
        IntegralNumber = number;
        IsFloatingPoint = false;
    }

    public NumberToken(string text, double number) : base(text)
    {
        FloatingPointNumber = number;
        IsFloatingPoint = true;
    }
}
