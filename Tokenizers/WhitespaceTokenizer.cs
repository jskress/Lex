using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating whitespace.
/// </summary>
public class WhitespaceTokenizer : Tokenizer
{
    /// <summary>
    /// This property controls whether line ends are reported separately from other
    /// whitespace.
    /// </summary>
    public bool ReportLineEndsSeparately { get; set; }

    public WhitespaceTokenizer(LexicalParser parser) : base(parser)
    {
        ReportLineEndsSeparately = false;
        ReportTokens = false;
    }

    /// <summary>
    /// This method is used to inform the parser whether this tokenizer can start a token
    /// with the specified character.
    /// </summary>
    /// <param name="ch">The character to check (or begin checking with).</param>
    /// <returns><c>true</c> if this tokenizer accepts the job of parsing the next token
    /// or <c>false</c> if not.</returns>
    protected override bool CanStart(char ch)
    {
        return char.IsWhiteSpace(ch);
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        Builder.Append(ch);

        if (!ReportLineEndsSeparately || ch != '\n')
        {
            (int next, ch) = Read();

            while (next >= 0 && char.IsWhiteSpace(ch))
            {
                if (ReportLineEndsSeparately && ch == '\n')
                    break;

                Builder.Append(ch);

                (next, ch) = Read();
            }

            ReturnChar(next);
        }

        return new WhitespaceToken(Builder.ToString());
    }
}
