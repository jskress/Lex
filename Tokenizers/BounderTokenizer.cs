using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating bounders.
/// </summary>
public class BounderTokenizer : Tokenizer
{
    private const string DefaultBounders = "[{()}]";

    private readonly string _bounders;

    public BounderTokenizer(LexicalParser parser, string bounders = DefaultBounders) : base(parser)
    {
        _bounders = Normalize(bounders ?? DefaultBounders, "the set of bounder characters");
    }

    /// <summary>
    /// This method is used to inform the parser whether this tokenizer can start a token
    /// with the specified character.
    /// </summary>
    /// <param name="ch">The character to check (or begin checking with).</param>
    /// <returns><c>true</c> if this tokenizer accepts the job of parsing the next token
    /// or <c>false</c> if not.</returns>
    internal override bool CanStart(char ch)
    {
        return _bounders.Contains(ch);
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        return new BounderToken(ch);
    }
}
