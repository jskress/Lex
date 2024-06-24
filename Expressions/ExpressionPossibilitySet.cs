using Lex.Clauses;
using Lex.Parser;
using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class represents a set of possibilities of token matching within an expression.
/// It is specifically used for a set of choices, similar to <see cref="SwitchClauseParser"/>,
/// where each choice is a sequence of one or more choices.
/// </summary>
public class ExpressionPossibilitySet
{
    /// <summary>
    /// This property tells us whether any choices have been added to the possibility set
    /// yet or not.
    /// </summary>
    public bool IsEmpty => _choices == null;

    private SwitchClauseParser _choices;

    /// <summary>
    /// This method is used to add a choice to the possibility set.
    /// </summary>
    /// <param name="tokens">The series of tokens that represent the choice.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionPossibilitySet AddChoice(params Token[] tokens)
    {
        ValidateTokens(tokens, nameof(tokens));

        ClauseParser clauseParser = CreateItemClauseParser(tokens);

        _choices = _choices == null ?
            new SwitchClauseParser().Matching(clauseParser)
            : _choices.Or(clauseParser);

        return this;
    }

    /// <summary>
    /// This is a helper method for validating that a token array exists and contains at
    /// least one token.
    /// </summary>
    /// <param name="tokens">The token array to validate.</param>
    /// <param name="name">The name to report when an error is found.</param>
    internal static void ValidateTokens(Token[] tokens, string name)
    {
        ArgumentNullException.ThrowIfNull(tokens, name);

        if (tokens.Length == 0)
            throw new ArgumentException("At least one token must be provided.", name);
    }

    /// <summary>
    /// This is a helper method for creating a suitable clause parser for the given array
    /// of tokens.
    /// </summary>
    /// <param name="tokens">The tokens to wrap with a clause parser.</param>
    /// <returns>An appropriate clause parser.</returns>
    internal static ClauseParser CreateItemClauseParser(Token[] tokens)
    {
        if (tokens.Length == 1)
            return new SingleTokenClauseParser(tokens[0]);

        SequentialClauseParser parser = new SequentialClauseParser()
            .Matching(tokens[0]);

        foreach (Token token in tokens[1..])
            parser.Then(token);

        return parser;
    }

    /// <summary>
    /// This method is used to try to parser tokens that match one of our possibilities. 
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <returns>The parsed match or <c>null</c>.</returns>
    internal Clause TryParse(LexicalParser parser)
    {
        if (_choices == null)
            throw new Exception("Empty possibility set will not match anything.");

        return _choices.TryParse(parser);
    }
}
