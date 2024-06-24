using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating string literals.
/// </summary>
public class StringTokenizer : Tokenizer
{
    private static readonly EscapeResolver DefaultEscapeResolver = new ();

    /// <summary>
    /// This property notes whether the string literal is expected to be raw.  If <c>true</c>,
    /// then escape sequences will be treated as plain text.
    /// </summary>
    public bool Raw { get; set; }

    /// <summary>
    /// This property indicates whether doubling the string literal bounder is a way to
    /// escape it, so it's included in the string literal rather than terminating the
    /// literal, ala, SQL single-quoted strings.
    /// </summary>
    public bool RepeatBounderEscapes { get; set; }

    /// <summary>
    /// This property indicates whether the string literal can span lines.
    /// </summary>
    public bool IsMultiLine { get; set; }

    /// <summary>
    /// This is the character that triggers an escape sequence.  It is set to, <c>\</c> by
    /// default.
    /// </summary>
    public char EscapeCharacter { get; set; }

    /// <summary>
    /// This property holds the object that will actually resolve an escape sequence.
    /// </summary>
    public EscapeResolver EscapeResolver { get; set; }

    /// <summary>
    /// This property holds the literal bounder we will look for on both ends of the string.
    /// </summary>
    protected string Bounder;

    public StringTokenizer(LexicalParser parser, string bounder) : base(parser)
    {
        Bounder = Normalize(bounder, "a string literal bounder");

        if (Bounder == string.Empty)
            throw new ArgumentException("String literal bounders cannot be empty.");

        Raw = false;
        RepeatBounderEscapes = false;
        IsMultiLine = false;
        EscapeCharacter = '\\';
        EscapeResolver = DefaultEscapeResolver;
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
        return IsNext(ch, Bounder);
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        int data;

        Skip(Bounder.Length - 1);

        do
        {
            (data, ch) = Read();

            if (data < 0 || (!IsMultiLine && ch == '\n'))
                throw new TokenException("String literal not properly terminated.");

            if (IsNext(ch, Bounder))
            {
                Skip(Bounder.Length - 1);

                (data, ch) = Read();

                if (RepeatBounderEscapes)
                {
                    if (IsNext(ch, Bounder))
                    {
                        ReturnChar(data);
                        Skip(Bounder.Length);
                    }
                    else
                        break;
                }
                else
                    break;
            }

            if (Raw)
                Builder.Append(ch);
            else
                ResolveEscape(ch);
        }
        while (true);

        ReturnChar(data);

        return new StringToken(Bounder, GetText());
    }

    /// <summary>
    /// This method is used to convert character escape sequences into the characters they
    /// represent.  It may be overridden if the default processing is not sufficient.
    /// </summary>
    /// <param name="ch"></param>
    protected virtual void ResolveEscape(char ch)
    {
        if (ch == EscapeCharacter)
        {
            char marker = ch;

            (_, ch) = Read();

            string resolved = EscapeResolver.ResolveEscape(Parser, ch);

            if (resolved == string.Empty)
                Builder.Append(marker).Append(ch);
            else
                Builder.Append(resolved);
        }
        else
            Builder.Append(ch);
    }

    /// <summary>
    /// This method is used to obtain the string literal the token represents.  By default,
    /// the current content of the text buffer is returned.  Some subclasses may want to
    /// post-process the text before it gets put into a token.
    /// </summary>
    /// <returns>The literal text to put in the token.</returns>
    protected virtual string GetText()
    {
        return Builder.ToString();
    }
}
