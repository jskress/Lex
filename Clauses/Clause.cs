using Lex.Expressions;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class represents the result of an attempt to parse tokens to satisfy clause
/// requirements.
/// </summary>
public class Clause
{
    /// <summary>
    /// This property provides the string tag that the DSL definition noted for the clause
    /// that was parsed.
    /// </summary>
    public string? Tag { get; init; }

    /// <summary>
    /// This property provides the list of tokens that matched the clause that was parsed.
    /// If this is present, then <see cref="Expressions"/> will not be.
    /// </summary>
    public List<Token> Tokens { get; init; } = [];

    /// <summary>
    /// This property provide the list of expressions the clause parser captured.  If this
    /// is present, then <see cref="Tokens"/> will not be.
    /// </summary>
    public List<IExpressionTerm> Expressions { get; init; } = [];

    /// <summary>
    /// This method is used to create a <see cref="ClauseReader"/> for sequentially reading
    /// the tokens and expressions captured by this clause.
    /// </summary>
    /// <returns>A reader over this clause.</returns>
    public ClauseReader Reader()
    {
        return new ClauseReader(this);
    }
}
