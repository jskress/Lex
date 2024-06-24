using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating identifiers.
/// </summary>
public class IdTokenizer : Tokenizer
{
    /// <summary>
    /// This constant holds the set of lower case letters.
    /// </summary>
    public const string Lowers = "abcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// This constant holds the set of upper case letters.
    /// </summary>
    public const string Uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// This constant holds the set of letters.
    /// </summary>
    public const string Letters = Lowers + Uppers;

    /// <summary>
    /// This constant holds the default identifier starters we may use.
    /// </summary>
    public const string DefaultStarters = Letters + "_";

    /// <summary>
    /// This constant holds the set of digits.
    /// </summary>
    public const string Digits = "0123456789";

    /// <summary>
    /// This constant holds the default identifier members we may use.
    /// </summary>
    public const string DefaultMembers = DefaultStarters + Digits;

    /// <summary>
    /// This holds the casing style to use when creating tokens.
    /// </summary>
    public LetterCaseStyle Style { get; set; } = LetterCaseStyle.AsIs;

    private readonly string _starters;
    private readonly string _members;

    public IdTokenizer(LexicalParser parser, string starters = null, string members = null) : base(parser)
    {
        _starters = starters ?? DefaultStarters;
        _members = members ?? DefaultMembers;
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
        return _starters.Contains(ch);
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

        (int data, ch) = Read();

        while (data >= 0 && _members.Contains(ch))
        {
            Builder.Append(ch);

            (data, ch) = Read();
        }

        ReturnChar(data);

        return new IdToken(Style.Apply(Builder.ToString()));
    }
}
