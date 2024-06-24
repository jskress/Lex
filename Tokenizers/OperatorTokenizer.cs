using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating operators.
/// </summary>
public class OperatorTokenizer : FixedListTokenizer
{
    private static readonly HashSet<string> DefaultOperators =
    [
        ";", ":", ",", ".", "@", "|", "||", "&", "&&", "~", "!", "#", "$", "%", "^", "_",
        @"\", "+", "++", "-", "--", "*", "/", "=", "+=", "-=", "*=", "/=", "|=", "&=", "==",
        "!=", "<", "<=", ">", ">=", "->", "=>", "..", "?", "??"
    ];

    public OperatorTokenizer(LexicalParser parser, HashSet<string> operators = null)
        : base(parser, operators ?? DefaultOperators, "an operator") {}

    /// <summary>
    /// This method allows for adding additional operator possibilities after construction.
    /// </summary>
    /// <param name="additionalOperators">The additional operators we want to support.</param>
    public void Including(IEnumerable<string> additionalOperators)
    {
        Including(additionalOperators, "an operator");
    }

    /// <summary>
    /// This method allows for the removal of unwanted operator possibilities after
    /// construction.
    /// </summary>
    /// <param name="unwantedOperators">The operators provided at construction that we don't want.</param>
    public void Excluding(IEnumerable<string> unwantedOperators)
    {
        Excluding(unwantedOperators, "an operator");
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        return new OperatorToken(Builder.ToString());
    }
}
