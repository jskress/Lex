using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating numeric literals.
/// </summary>
public class NumberTokenizer : Tokenizer
{
    /// <summary>
    /// This property controls whether we handle parsing a leading sign.
    /// </summary>
    public bool SupportLeadingSign { get; set; }

    /// <summary>
    /// This property controls whether we handle parsing a fractional part (i.e,
    /// a decimal point and digits to its right).
    /// </summary>
    public bool SupportFraction { get; set; }

    /// <summary>
    /// This property controls whether we handle parsing scientific notation.  It is
    /// relevant only if <c>SupportFraction</c> is set to <c>true</c>.
    /// </summary>
    public bool SupportScientificNotation { get; set; }

    public NumberTokenizer(LexicalParser parser) : base(parser)
    {
        SupportLeadingSign = false;
        SupportFraction = true;
        SupportScientificNotation = true;
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
        if (SupportLeadingSign && ch is '+' or '-')
        {
            (_, char next) = Peek();

            if (SupportFraction && next == '.')
            {
                _ = Read();

                bool isDigit = char.IsDigit(Peek().Item2);

                ReturnChar(next);

                return isDigit;
            }

            return char.IsDigit(next);
        }

        return char.IsDigit(ch) || (SupportFraction && ch == '.' && char.IsDigit(Peek().Item2));
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        bool haveDot = !SupportFraction || ch == '.';

        Builder.Append(ch);

        (int data, ch) = ReadDigits();

        if (SupportFraction && ch == '.' && !haveDot)
        {
            Builder.Append(ch);

            (data, ch) = ReadDigits();
        }

        if (SupportFraction && SupportScientificNotation && ch is 'e' or 'E')
        {
            Builder.Append(ch);

            if (Peek().Item2 is '+' or '-')
                Builder.Append(Read().Item2);

            (data, _) = ReadDigits();
        }

        ReturnChar(data);

        return NumberToken.FromText(Builder.ToString());
    }

    /// <summary>
    /// This is a helper method for reading a series of digits into our buffer.
    /// </summary>
    /// <returns>The first character that is not a digit.</returns>
    private (int, char) ReadDigits()
    {
        (int data, char ch) = Read();

        while (char.IsDigit(ch))
        {
            Builder.Append(ch);

            (data, ch) = Read();
        }

        return (data, ch);
    }
}
