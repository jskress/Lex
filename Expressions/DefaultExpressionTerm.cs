using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class provides a default representation for a term in an expression.
/// </summary>
public abstract class DefaultExpressionTerm : IExpressionTerm
{
    /// <summary>
    /// This property holds the list of tokens that make up the term.
    /// </summary>
    public IReadOnlyList<Token> Tokens { get; internal init; }

    /// <summary>
    /// This property provides a text representation of this term.
    /// </summary>
    public string Text => "(" + Format() + ")";

    /// <summary>
    /// Subclasses must provide this to produce a text representation of this term.
    /// </summary>
    /// <returns>A text representation of this term.</returns>
    protected abstract string Format();
}
