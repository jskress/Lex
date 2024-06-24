using System.Data;
using System.Text;
using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the base class for objects that can isolate a token from a stream
/// of characters.
/// </summary>
public abstract class Tokenizer
{
    /// <summary>
    /// This is a helper method for normalizing text to guarantee it is not <c>null</c>
    /// and does not contain whitespace.  An input that is <c>null</c> will become the
    /// empty string.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <param name="noun">The noun to use for errors.</param>
    /// <returns></returns>
    protected static string Normalize(string text, string noun)
    {
        text = text?.Trim() ?? string.Empty;

        if (text.Any(char.IsWhiteSpace))
            throw new ArgumentException($"Whitespace is not allowed in {noun}.");

        return text;
    }

    /// <summary>
    ///This property holds the flag that notes whether tokens produced by this tokenizer
    /// should be reported.
    /// </summary>
    public bool ReportTokens { get; set; }

    /// <summary>
    /// This holds the parser we are supporting.
    /// </summary>
    protected readonly LexicalParser Parser;

    /// <summary>
    /// This buffer is provided for subclasses to use in building up the text from the
    /// source that represents the token being parsed.  It is reset after each token
    /// is parsed to be empty.
    /// </summary>
    protected readonly StringBuilder Builder;

    protected Tokenizer(LexicalParser parser)
    {
        Parser = parser;
        Builder = new StringBuilder();

        ReportTokens = true;

        parser.Register(this);
    }

    /// <summary>
    /// This method is used to inform the parser whether this tokenizer can start a token
    /// with the specified character.  If more characters are required to be read for this
    /// decision to be made, they must be returned to the parser such that the stream of
    /// characters is unchanged.
    /// </summary>
    /// <param name="ch">The character to check (or begin checking with).</param>
    /// <returns><c>true</c> if this tokenizer accepts the job of parsing the next token
    /// or <c>false</c> if not.</returns>
    internal abstract bool CanStart(char ch);

    /// <summary>
    /// This method is called to use this tokenizer to actually parse a token from the
    /// character stream.  It is only called if the <c>CanStart(char)</c> method returned
    /// <c>true</c> for the given character.
    /// </summary>
    /// <param name="ch">The character that started this tokenizer.</param>
    /// <param name="line">The line number in the source of the character.</param>
    /// <param name="column">The column number in the source of the character.</param>
    /// <returns>The token that the tokenizer has parsed.</returns>
    internal Token ParseToken(char ch, int line, int column)
    {
        Token result;

        try
        {
            result = ParseToken(ch);

            if (result == null)
            {
                throw new NoNullAllowedException(
                    $"The {GetType().Name} tokenizer produced a null token at [{line}:{column}].");
            }
        }
        catch (TokenException exception)
        {
            exception.Line = line;
            exception.Column = column;

            Builder.Length = 0;

            throw;
        }

        result.Line = line;
        result.Column = column;

        Builder.Length = 0;

        return result;
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected abstract Token ParseToken(char ch);

    /// <summary>
    /// This method is used to test if the given text is what follows in the character
    /// stream.  The stream state and contents of <c>Builder</c> will remain unchanged.
    /// </summary>
    /// <param name="first">The first character of the text to match, already read from
    /// the character stream.</param>
    /// <param name="text">The text to match.</param>
    /// <returns><c>true</c>, if the given text is what's coming up next in the character
    /// stream, or <c>false</c>, if not.</returns>
    protected bool IsNext(char first, string text)
    {
        if (first != text[0])
            return false;

        if (text.Length == 1)
            return true;

        StringBuilder builder = new ();
        bool result = true;

        builder.Append(first);

        for (int index = 1; index < text.Length; index++)
        {
            (int data, char ch) = Read();

            if (data < 0 || ch != text[index])
            {
                ReturnChar(data);

                result = false;
                break;
            }

            builder.Append(ch);
        }

        Parser.ReturnBuffer(builder, 1);

        return result;
    }

    /// <summary>
    /// This method is used to read a character from the source stream.
    /// </summary>
    /// <returns>The next character from the source stream, or <c>-1</c>, if the stream has
    /// been exhausted.</returns>
    protected (int, char) Read()
    {
        int data = Parser.GetNextChar();

        return (data, data < 0 ? ' ' : (char) data);
    }

    /// <summary>
    /// This method returns the next character from the character stream without consuming
    /// it.
    /// </summary>
    /// <returns>The next character from the source stream, or <c>-1</c>, if the stream has
    /// been exhausted.</returns>
    protected (int, char) Peek()
    {
        (int data, char next) = Read();

        ReturnChar(data);

        return (data, next);
    }

    /// <summary>
    /// This method is used to skip over a known number of characters
    /// </summary>
    /// <param name="count"></param>
    /// <returns><c>true</c>, if we have reached the end of the character stream, or
    /// <c>false</c>, if not</returns>
    protected bool Skip(int count)
    {
        for (int index = 0; index < count; index++)
        {
            (int ch, _) = Read();

            if (ch < 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// this is a helper method for returning the current content of the read buffer back
    /// to the character reader (in the proper order).  The buffer's length is set to 0
    /// as a side effect.
    /// </summary>
    /// <param name="keep">The number of characters in the buffer to keep.</param>
    protected void ReturnBuffer(int keep = 0)
    {
        Parser.ReturnBuffer(Builder, keep);
    }

    /// <summary>
    /// This method is used to return a character to the parser's source stream.
    /// </summary>
    /// <param name="ch">The character to return.</param>
    protected void ReturnChar(int ch)
    {
        Parser.ReturnChar(ch);
    }
}
