using Lex.Parser;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class represents a clause made up of a list of alternate sub-clauses.  Each
/// sub-clause may have its own action on it to invoke when that sub-clause is matched.
/// </summary>
public class SwitchClauseParser : ClauseParser, IClauseParserParent
{
    /// <summary>
    /// This record defines the data we need for each of our sub-clauses.
    /// </summary>
    /// <param name="ClauseParser">The particular sub-clause.</param>
    /// <param name="OnMatchTag">The tag, if any, to emit when the clause is matched.</param>
    private record Entry(ClauseParser ClauseParser, string OnMatchTag);

    /// <summary>
    /// This property provides access to the parent's children.
    /// </summary>
    public List<ClauseParser> Children => _clauses.Select(entry => entry.ClauseParser).ToList();

    private readonly List<Entry> _clauses = [];

    private string _moClausesMatchedMessage;

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this switch one.
    /// </summary>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Matching(params Token[] tokens)
    {
        return Matching(onMatchTag: null, errorMessage: null, tokens);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this switch one.
    /// </summary>
    /// <param name="onMatchTag">An optional action to invoke when the token is matched.</param>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Matching(string onMatchTag = null, string errorMessage = null, params Token[] tokens)
    {
        return Matching(new SingleTokenClauseParser(errorMessage, tokens), onMatchTag);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this switch one.
    /// </summary>
    /// <param name="types">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Matching(params Type[] types)
    {
        return Matching(onMatchTag: null, errorMessage: null, types);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this switch one.
    /// </summary>
    /// <param name="onMatchTag">An optional action to invoke when the token is matched.</param>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="types">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Matching(string onMatchTag = null, string errorMessage = null, params Type[] types)
    {
        return Matching(new SingleTokenClauseParser(errorMessage, types), onMatchTag);
    }

    /// <summary>
    /// This method is used to add a potential clause to this switch clause.
    /// </summary>
    /// <param name="clauseParser">The potential clause to add.</param>
    /// <param name="onMatchTag">An optional tag to emit when the clause is matched.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Matching(ClauseParser clauseParser, string onMatchTag = null)
    {
        return AddClause(
            count => count > 0, "The clause has already been started.",
            clauseParser, onMatchTag);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this switch one.
    /// </summary>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Or(params Token[] tokens)
    {
        return Or(onMatchTag: null, errorMessage: null, tokens);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this switch one.
    /// </summary>
    /// <param name="onMatchTag">An optional action to invoke when the token is matched.</param>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Or(string onMatchTag = null, string errorMessage = null, params Token[] tokens)
    {
        return Or(new SingleTokenClauseParser(errorMessage, tokens), onMatchTag);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this switch one.
    /// </summary>
    /// <param name="types">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Or(params Type[] types)
    {
        return Or(onMatchTag: null, errorMessage: null, types);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this switch one.
    /// </summary>
    /// <param name="onMatchTag">An optional action to invoke when the token is matched.</param>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="types">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Or(string onMatchTag = null, string errorMessage = null, params Type[] types)
    {
        return Or(new SingleTokenClauseParser(errorMessage, types), onMatchTag);
    }

    /// <summary>
    /// This method is used to add a potential clause to this switch clause.
    /// </summary>
    /// <param name="clauseParser">The potential clause to add.</param>
    /// <param name="onMatchTag">An optional tag to emit when the clause is matched.</param>
    /// <returns>This object, for fluency.</returns>
    public SwitchClauseParser Or(ClauseParser clauseParser, string onMatchTag = null)
    {
        return AddClause(
            count => count == 0, "The clause has not been started yet.",
            clauseParser, onMatchTag);
    }

    /// <summary>
    /// This is a helper method for adding a new sub-clause to our list.
    /// </summary>
    /// <param name="gate">A test to see if adding is ok.</param>
    /// <param name="gateMessage">The message to use for the exception if adding is not good.</param>
    /// <param name="clauseParser">The clause to add.</param>
    /// <param name="onMatchTag">An optional tag to emit when the clause is matched.</param>
    /// <returns>This object, for fluency.</returns>
    private SwitchClauseParser AddClause(
        Func<int, bool> gate, string gateMessage, ClauseParser clauseParser, string onMatchTag)
    {
        if (gate.Invoke(Children.Count))
            throw new Exception(gateMessage);

        _clauses.Add(new Entry(clauseParser, onMatchTag));

        return this;
    }

    /// <summary>
    /// This method is used to set the error message to use if none of the clauses are
    /// matched.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public SwitchClauseParser OnNoClausesMatched(string errorMessage)
    {
        _moClausesMatchedMessage = errorMessage;

        return this;
    }

    /// <summary>
    /// This method tries to match the next clause of tokens from the parser to one of ours,
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    protected override Clause TryParseClause(LexicalParser parser)
    {
        (Clause parsed, string onMatchTag) = _clauses
            .Select(entry => (entry.ClauseParser.TryParse(parser), entry.OnMatchTag))
            .FirstOrDefault(item => item.Item1 != null);

        if (parsed != null)
        {
            return new Clause
            {
                Tag = onMatchTag,
                Tokens = parsed.Tokens,
                Expressions = parsed.Expressions
            };
        }

        if (_moClausesMatchedMessage != null)
        {
            throw new TokenException(_moClausesMatchedMessage)
            {
                Token = parser.GetNextToken()
            };
        }

        return null;
    }
}
