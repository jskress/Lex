using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating based numeric literals.
/// </summary>
public class BasedNumberTokenizer : Tokenizer
{
    private const string BinaryDigits = "01";
    private const string OctalDigits = "01234567";
    private const string HexDigits = "0123456789abcdefABCDEF";

    private readonly string _markers;

    public BasedNumberTokenizer(LexicalParser parser, bool supportHex = true,
        bool supportOctal = true, bool supportBinary = true)
        : base(parser)
    {
        string text = string.Empty;

        if (supportHex)
            text += "xX";

        if (supportOctal)
            text += OctalDigits;

        if (supportBinary)
            text += "bB";

        if (text == string.Empty)
            throw new ArgumentException("Support for at least one of hex, octal or binary must be requested.");

        _markers = text;
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
        if (ch == '0')
        {
            (int data, char next) = Read();
            bool result = false;

            if (_markers.Contains(next))
                result = char.IsDigit(next) || GetValidChars(next).Contains(Peek().Item2);

            ReturnChar(data);

            return result;
        }

        return false;
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

        (_, char radixMarker) = Read();
        int start = 1;
        int data = 0;

        string validChars = GetValidChars(radixMarker);

        if (char.IsDigit(radixMarker))
            ch = radixMarker;
        else
        {
            Builder.Append(radixMarker);

            (data, ch) = Read();

            start++;
        }

        while (ch == '_' || validChars.Contains(ch))
        {
            if (ch != '_')
                Builder.Append(ch);

            (data, ch) = Read();
        }

        ReturnChar(data);

        string text = Builder.ToString();

        return new NumberToken(text, Convert.ToInt64(text[start..], GetRadix(radixMarker)));
    }

    /// <summary>
    /// This method returns the proper character set for the base type indicated by the
    /// given character.
    /// </summary>
    /// <param name="ch">The character that indicates the appropriate base.</param>
    /// <returns>The set of valid characters for that numeric base.</returns>
    private static string GetValidChars(char ch)
    {
        return ch switch
        {
            'X' => HexDigits,
            'x' => HexDigits,
            'B' => BinaryDigits,
            'b' => BinaryDigits,
            _ => OctalDigits
        };
    }

    /// <summary>
    /// This method returns the proper radix for the base type indicated by the given
    /// character.
    /// </summary>
    /// <param name="ch">The character that indicates the appropriate base.</param>
    /// <returns>The appropriate for that numeric base.</returns>
    private static int GetRadix(char ch)
    {
        return ch switch
        {
            'X' => 16,
            'x' => 16,
            'B' => 2,
            'b' => 2,
            _ => 8
        };
    }
}
