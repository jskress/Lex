using System.Text.RegularExpressions;
using Lex.Parser;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating single-quoted string literals.  Such
/// /// literals cannot span lines.
/// </summary>
public class SingleQuotedStringTokenizer : StringTokenizer
{
    private static readonly Regex SingleCharacterSlashUPattern = new Regex(@"^\\u(?:\d|[a-fA-F]){4}$");

    /// <summary>
    /// This property notes whether a single quoted string must represent a single character.
    /// </summary>
    public bool RepresentsCharacter { get; set; }

    public SingleQuotedStringTokenizer(LexicalParser parser) : base(parser, "'")
    {
        RepresentsCharacter = true;
    }

    /// <summary>
    /// This method is used to obtain the string literal the token represents.  By default,
    /// the current content of the text buffer is returned.  Some subclasses may want to
    /// post-process the text before it gets put into a token.
    /// </summary>
    /// <returns>The literal text to put in the token.</returns>
    protected override string GetText()
    {
        string value = base.GetText();

        if (RepresentsCharacter && !SingleCharacterSlashUPattern.IsMatch(value))
        {
            if (value == string.Empty)
                throw new TokenException("Character literal is empty.");

            if (!(value.Length == 1 || (value.Length == 2 && char.IsSurrogatePair(value, 0))))
                throw new TokenException("Character literal has too many characters.");
        }

        return value;
    }
}
