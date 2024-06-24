using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class represents an item within a term choice.
/// </summary>
public abstract class TermChoiceItem;

/// <summary>
/// This class represents the matching of a token.
/// </summary>
internal class TokenTermChoiceItem : TermChoiceItem
{
    internal Token Token { get; init; }
    internal bool Suppress { get; init; }
}

/// <summary>
/// This class represents the matching of a token type.
/// </summary>
internal class TypeTermChoiceItem : TermChoiceItem
{
    internal Type Type { get; init; }
}

/// <summary>
/// This class represents a collection of expressions.
/// </summary>
public class ExpressionTermChoiceItem : TermChoiceItem
{
    /// <summary>
    /// This property notes the minimum number of expressions that are to be required.
    /// </summary>
    public int Minimum { get; }

    /// <summary>
    /// This property notes the maximum number of expressions that are to be allowed.
    /// </summary>
    public int Maximum { get; }

    /// <summary>
    /// This property holds the set of possible separators that can occur between expressions.
    /// If this is present, then a separator will be required.
    /// </summary>
    public ExpressionPossibilitySet Separators { get; }

    public ExpressionTermChoiceItem(int minimum, int maximum, ExpressionPossibilitySet separators)
    {
        if (minimum < 0)
            throw new ArgumentException("Minimum must be zero or greater.", nameof(minimum));

        if (maximum < 1)
            throw new ArgumentException("Maximum must be 1 or greater.", nameof(maximum));

        if (maximum < minimum)
            throw new ArgumentException("Maximum cannot be smaller than minimum.", nameof(maximum));

        if (maximum < 2 && separators != null)
            throw new ArgumentException("Cannot specify separators when maximum is 1.", nameof(separators));

        Minimum = minimum;
        Maximum = maximum;
        Separators = separators;
    }
}
