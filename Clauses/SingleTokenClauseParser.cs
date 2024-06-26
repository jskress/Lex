using Lex.Parser;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class is used for matching a clause containing exactly one token.  The token is
/// matched against one or more possibilities.
/// </summary>
public class SingleTokenClauseParser : ClauseParser
{
    private readonly Func<LexicalParser, Token> _matcher;
    private readonly string _errorMessage;

    public SingleTokenClauseParser(params Token[] tokens) : this(errorMessage: null, tokens) {}

    public SingleTokenClauseParser(string errorMessage = null, params Token[] tokens)
    {
        _matcher = parser => parser.IsNext(tokens) ? parser.GetNextToken() : null;
        _errorMessage = errorMessage;
    }

    public SingleTokenClauseParser(params Type[] types) : this(errorMessage: null, types) {}

    public SingleTokenClauseParser(string errorMessage = null, params Type[] types)
    {
        _matcher = parser => parser.IsNextOfType(types) ? parser.GetNextToken() : null;
        _errorMessage = errorMessage;
    }

    /// <summary>
    /// This method tries to match the next token from the parser either by token or type,
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    protected override Clause TryParseClause(LexicalParser parser)
    {
        Token token = _matcher.Invoke(parser);
        bool haveToken = token != null;

        return haveToken switch
        {
            false when _errorMessage != null => throw new TokenException(_errorMessage) {Token = parser.GetNextToken()},
            true => new Clause { Tokens = [token], Expressions = [] },
            _ => null
        };
    }
}
