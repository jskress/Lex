using Lex.Expressions;
using Lex.Parser;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class represents a clause made up of a sequential list of sub-clauses.
/// </summary>
public class SequentialClauseParser : ClauseParser, IClauseParserParent
{
    /// <summary>
    /// This property provides access to the parent's children.
    /// </summary>
    public List<ClauseParser> Children { get; } = [];

    private string? _onMatchTag;

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Matching(params Token[] tokens)
    {
        return Matching(errorMessage: null, tokens);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Matching(string? errorMessage = null, params Token[] tokens)
    {
        return Matching(new SingleTokenClauseParser(errorMessage, tokens));
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="types">The list of token type possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Matching(params Type[] types)
    {
        return Matching(errorMessage: null, types);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="types">The list of token type possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Matching(string? errorMessage = null, params Type[] types)
    {
        return Matching(new SingleTokenClauseParser(errorMessage, types));
    }

    /// <summary>
    /// This method is used to add a sub-clause to this sequential one.  Call this to add
    /// the 1st sub-clause.
    /// </summary>
    /// <param name="clauseParser">The clause to add.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Matching(ClauseParser clauseParser)
    {
        return AddClause(
            count => count > 0, "The clause has already been started.",
            clauseParser);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add all but the 1st sub-clause.
    /// </summary>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Then(params Token[] tokens)
    {
        return Then(errorMessage: null, tokens);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add all but the 1st sub-clause.
    /// </summary>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="tokens">The list of token possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Then(string? errorMessage = null, params Token[] tokens)
    {
        return Then(new SingleTokenClauseParser(errorMessage, tokens));
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this sequential one.  Call this to add all but the 1st sub-clause.
    /// </summary>
    /// <param name="types">The list of token type possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Then(params Type[] types)
    {
        return Then(errorMessage: null, types);
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// token types to this sequential one.  Call this to add all but the 1st sub-clause.
    /// </summary>
    /// <param name="errorMessage">An error message to use should the single token fail to
    /// match.</param>
    /// <param name="types">The list of token type possibilities.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Then(string? errorMessage = null, params Type[] types)
    {
        return Then(new SingleTokenClauseParser(errorMessage, types));
    }

    /// <summary>
    /// This method is used to add a sub-clause to this sequential one.  Call this to add
    /// all but the 1st sub-clause.
    /// </summary>
    /// <param name="clauseParser">The clause to add.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser Then(ClauseParser clauseParser)
    {
        return AddClause(
            count => count == 0, "The clause has not been started yet.",
            clauseParser);
    }

    /// <summary>
    /// This is a helper method for adding a new sub-clause to our list.
    /// </summary>
    /// <param name="gate">A test to see if adding is ok.</param>
    /// <param name="gateMessage">The message to use for the exception if adding is not good.</param>
    /// <param name="clauseParser">The clause to add.</param>
    /// <returns>This object, for fluency.</returns>
    private SequentialClauseParser AddClause(Func<int, bool> gate, string gateMessage, ClauseParser clauseParser)
    {
        if (gate.Invoke(Children.Count))
            throw new Exception(gateMessage);

        Children.Add(clauseParser);

        return this;
    }

    /// <summary>
    /// This method is used to set the tag to emit when this clause is matched.
    /// </summary>
    /// <param name="onMatchTag">The tag to emit when we match.</param>
    /// <returns>This object, for fluency.</returns>
    public SequentialClauseParser OnMatchTag(string onMatchTag)
    {
        _onMatchTag = onMatchTag;

        return this;
    }

    /// <summary>
    /// This method tries to match the next sequential clause of tokens from the parser,
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    protected override Clause? TryParseClause(LexicalParser parser)
    {
        List<Token> tokens = [];
        List<IExpressionTerm> expressions = [];
        bool markPending = true;

        // A sub-clause that fails part way through leaves us having consumed tokens we now
        // need to give back, and we cannot do that by hand for all of them: a sub-clause that
        // parsed an expression hands back a term, not the tokens that went into it.  Marking
        // the position lets the parser undo the whole attempt either way, so that a caller
        // trying another alternative sees the parser exactly where it started.
        parser.MarkPosition();

        try
        {
            foreach (ClauseParser child in Children)
            {
                Clause? parsed = child.TryParse(parser);

                if (parsed == null)
                {
                    markPending = false;

                    parser.RollbackToMark();

                    return null;
                }

                tokens.AddRange(parsed.Tokens);
                expressions.AddRange(parsed.Expressions);
            }

            markPending = false;

            parser.ReleaseMark();

            return new Clause
            {
                Tag = _onMatchTag,
                Tokens = tokens,
                Expressions = expressions
            };
        }
        finally
        {
            // A sub-clause may throw for a hard error; unwind the attempt so the parser is
            // left somewhere sane if that exception gets caught.
            if (markPending)
                parser.RollbackToMark();
        }
    }
}
