using Lex.Parser;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class is used for matching a clause containing exactly one token.  The token is
/// matched against one or more possibilities.
/// </summary>
public class SingleTokenClauseParser : ClauseParser
{
    private readonly Func<LexicalParser, Token?> _matcher;
    private readonly string? _errorMessage;
    private readonly string _expectation;

    public SingleTokenClauseParser(params Token[] tokens) : this(errorMessage: null, tokens) {}

    public SingleTokenClauseParser(string? errorMessage = null, params Token[] tokens)
    {
        _matcher = parser => parser.IsNext(tokens) ? parser.GetNextToken() : null;
        _errorMessage = errorMessage;
        _expectation = string.Join(" or ", tokens.Select(Token.Describe));
    }

    public SingleTokenClauseParser(params Type[] types) : this(errorMessage: null, types) {}

    public SingleTokenClauseParser(string? errorMessage = null, params Type[] types)
    {
        _matcher = parser => parser.IsNextOfType(types) ? parser.GetNextToken() : null;
        _errorMessage = errorMessage;
        _expectation = string.Join(" or ", types.Select(Token.Describe));
    }

    /// <summary>
    /// This method tries to match the next token from the parser either by token or type,
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    protected override Clause? TryParseClause(LexicalParser parser)
    {
        Token? token = _matcher.Invoke(parser);

        if (token != null)
            return new Clause { Tokens = [token], Expressions = [] };

        if (_errorMessage != null)
            throw new TokenException(_errorMessage) { Token = parser.GetNextToken() };

        RecordFailure(parser);

        return null;
    }

    /// <summary>
    /// This is a helper method for reporting an unannotated match failure to the parser's
    /// failure tracker, if it has one.  This is a no-op when no tracker is attached.
    /// </summary>
    /// <param name="parser">The parser we failed to match against.</param>
    private void RecordFailure(LexicalParser parser)
    {
        if (parser.FailureTracker == null)
            return;

        Token? next = parser.PeekNextToken();

        parser.FailureTracker.Record(next?.Line ?? parser.Line, next?.Column ?? parser.Column, _expectation);
    }
}
