using Lex.Expressions;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class provides a convenient way to sequentially read the tokens and expressions
/// captured by a <see cref="Clause"/>, in the order they were captured, without having to
/// hand-index into its <see cref="Clause.Tokens"/> and <see cref="Clause.Expressions"/>
/// lists yourself.  Reading from a <see cref="ClauseReader"/> does not modify the
/// <see cref="Clause"/> it was created from, so the same clause may be read more than once,
/// including with more than one reader at a time.
/// </summary>
public class ClauseReader
{
    /// <summary>
    /// This property notes whether there are any tokens left to read.
    /// </summary>
    public bool HasMoreTokens => _tokenIndex < _clause.Tokens.Count;

    /// <summary>
    /// This property notes whether there are any expressions left to read.
    /// </summary>
    public bool HasMoreExpressions => _expressionIndex < _clause.Expressions.Count;

    private readonly Clause _clause;

    private int _tokenIndex;
    private int _expressionIndex;

    public ClauseReader(Clause clause)
    {
        ArgumentNullException.ThrowIfNull(clause, nameof(clause));

        _clause = clause;
    }

    /// <summary>
    /// This method returns the next token without consuming it; i.e., the next call to
    /// <see cref="NextToken()"/> (or the other token-reading methods) will still return it.
    /// </summary>
    /// <returns>The next token to be read, or <c>null</c>, if there isn't one.</returns>
    public Token? PeekToken()
    {
        return HasMoreTokens ? _clause.Tokens[_tokenIndex] : null;
    }

    /// <summary>
    /// This method reads and returns the next token.
    /// </summary>
    /// <returns>The next token.</returns>
    public Token NextToken()
    {
        if (!HasMoreTokens)
            throw new InvalidOperationException("There are no more tokens to read from this clause.");

        return _clause.Tokens[_tokenIndex++];
    }

    /// <summary>
    /// This method reads the next token and returns its text.  This is a convenience
    /// wrapper around <see cref="NextToken()"/>.
    /// </summary>
    /// <returns>The text of the next token.</returns>
    public string NextText()
    {
        return NextToken().Text;
    }

    /// <summary>
    /// This method reads the next token, requiring it to be of the given type.
    /// </summary>
    /// <typeparam name="T">The type the next token is required to be.</typeparam>
    /// <returns>The next token, cast to the required type.</returns>
    public T NextToken<T>()
        where T : Token
    {
        Token token = NextToken();

        if (token is not T typed)
        {
            throw new InvalidOperationException(
                $"Expecting the next token to be {Token.Describe(typeof(T))} but found " +
                $"{Token.Describe(token.GetType())} instead.");
        }

        return typed;
    }

    /// <summary>
    /// This method is used to skip over the next token, if its text matches the one given.
    /// This is useful for skipping past punctuation or other bounding tokens you don't
    /// otherwise need to look at.
    /// </summary>
    /// <param name="text">The text to match the next token's text against.</param>
    /// <returns><c>true</c>, if the next token matched and was skipped, or <c>false</c>,
    /// if not.</returns>
    public bool SkipIfNextTextIs(string text)
    {
        if (PeekToken()?.Text != text)
            return false;

        _tokenIndex++;

        return true;
    }

    /// <summary>
    /// This method returns the next expression without consuming it; i.e., the next call
    /// to <see cref="NextExpression"/> will still return it.
    /// </summary>
    /// <returns>The next expression to be read, or <c>null</c>, if there isn't one.</returns>
    public IExpressionTerm? PeekExpression()
    {
        return HasMoreExpressions ? _clause.Expressions[_expressionIndex] : null;
    }

    /// <summary>
    /// This method reads and returns the next expression.
    /// </summary>
    /// <returns>The next expression.</returns>
    public IExpressionTerm NextExpression()
    {
        if (!HasMoreExpressions)
            throw new InvalidOperationException("There are no more expressions to read from this clause.");

        return _clause.Expressions[_expressionIndex++];
    }
}
