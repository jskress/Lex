using Lex.Parser;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class represents a clause that may repeat some number of times.  With a minimum,
/// that may repeats are required.  The wrapped clause may repeat up to the maximum number
/// of times.
/// </summary>
public class RepeatingClauseParser : ClauseParser, IClauseParserParent
{
    /// <summary>
    /// This property provides access to the parent's children.
    /// </summary>
    public List<ClauseParser> Children => [_wrapped];

    private readonly ClauseParser _wrapped;
    private readonly int _min;
    private readonly int? _max;
    private readonly string _errorMessage;

    public RepeatingClauseParser(ClauseParser wrapped, int min = 0, int? max = null, string errorMessage = null)
    {
        ArgumentNullException.ThrowIfNull(wrapped, nameof(wrapped));

        if (min < 0)
            throw new ArgumentException($"The minimum value cannot be less than 0.");

        if (min > max)
            throw new ArgumentException($"Min ({min}) cannot be larger than max ({max}).");

        _wrapped = wrapped;
        _min = min;
        _max = max;
        _errorMessage = errorMessage;
    }

    /// <summary>
    /// This method tries to match our wrapped clause as many times as is present and that
    /// our range allows,
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    protected override Clause TryParseClause(LexicalParser parser)
    {
        List<Token> tokens = [];
        string lastMatchedTag = null;
        int count = 0;

        while (true)
        {
            Clause wrappedResult = _wrapped.TryParse(parser);

            if (wrappedResult != null)
            {
                tokens.AddRange(wrappedResult.Tokens);

                count++;

                if (count >= _max)
                    return new Clause { Tag = wrappedResult.Tag, Tokens = tokens };

                lastMatchedTag = wrappedResult.Tag ?? lastMatchedTag;
            }
            else
            {
                if (count >= _min)
                    return new Clause { Tag = lastMatchedTag, Tokens = tokens };

                parser.ReturnTokens(tokens);

                if (_errorMessage != null)
                    throw new TokenException(_errorMessage) { Token = parser.GetNextToken() };

                return null;
            }
        }
    }
}
