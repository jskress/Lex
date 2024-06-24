using System.Text;
using Lex.Parser;

namespace Lex.Tokenizers;

/// <summary>
/// This class represents a mapping from a character that follows the backslash to the
/// character that escape sequence represents.
/// </summary>
public class EscapeResolver
{
    private const string HexCharacters = "01234556789abcdefABCDEF";

    /// <summary>
    /// This is our default dictionary for mapping an escaped character to the single character
    /// it represents.
    /// </summary>
    private static readonly Dictionary<char, string> DefaultSingleCharMap = new ()
    {
        { '\'', "\\'" },
        { '"', "\"" },
        { '\\', "\\" },
        { '0', "\0" },
        { 'a', "\a" },
        { 'b', "\b" },
        { 'f', "\f" },
        { 'n', "\n" },
        { 'r', "\r" },
        { 't', "\t" },
        { 'v', "\v" }
    };

    /// <summary>
    /// These are the default markers that require extra characters to resolve.
    /// </summary>
    private const string DefaultConsumers = "uxU";

    private readonly Dictionary<char, string> _singleCharMap;
    private readonly string _consumers;

    public EscapeResolver(Dictionary<char, string> singleCharMap = null, string consumers = DefaultConsumers)
    {
        _singleCharMap = singleCharMap ?? DefaultSingleCharMap;
        _consumers = consumers;
    }

    /// <summary>
    /// This method is used to resolve an escape sequence.  The character provided is the
    /// start of the sequence after the backslash.  If more than a single character is required,
    /// they will be pulled from the given parser.  If the escape sequence is not valid in
    /// that case, any characters read will have been returned to the parser.
    /// </summary>
    /// <param name="parser">The parser for consumers to pull extra characters from.</param>
    /// <param name="ch">The character that begins the sequence.</param>
    /// <returns>The characters, if any, that the escape sequence resolves to.</returns>
    public string ResolveEscape(LexicalParser parser, char ch)
    {
        if (_singleCharMap.TryGetValue(ch, out string result))
            return result;

        if (_consumers.Contains(ch))
            return ResolveConsumer(parser, ch);

        return string.Empty;
    }

    /// <summary>
    /// This method handles resolving a multi-character escape sequence.  It may be
    /// overridden if the standard ones are not sufficient.  If you need to change
    /// what characters trigger this method, be sure to pass an appropriate consumers
    /// string to the constructor.
    /// </summary>
    /// <param name="parser">The parser for consumers to pull extra characters from.</param>
    /// <param name="ch">The character that begins the sequence.</param>
    /// <returns>The characters, if any, that the escape sequence resolves to.</returns>
    protected virtual string ResolveConsumer(LexicalParser parser, char ch)
    {
        return ch switch
        {
            'u' => ReadHexValue(parser, 4),
            'x' => ReadHexValue(parser, -1),
            'U' => ReadHexValue(parser, 8),
            _ => string.Empty
        };
    }

    /// <summary>
    /// This method is used to read the given number of hex digits and then convert them
    /// to a string that represents the encoded character(s).
    /// </summary>
    /// <remarks>
    /// If <c>count</c> is less than zero, then up to 4 characters will be read.
    /// </remarks>
    /// <param name="parser">The parser for consumers to pull extra characters from.</param>
    /// <param name="count">The number of characters to read.</param>
    /// <returns></returns>
    private static string ReadHexValue(LexicalParser parser, int count)
    {
        StringBuilder builder = new ();

        while (true)
        {
            int next = parser.GetNextChar();
            char nextChar = next < 0 ? ' ' : (char) next;

            if (HexCharacters.Contains(nextChar))
            {
                builder.Append(nextChar);

                if ((count >= 0 && builder.Length == count) ||
                    (count < 0 && builder.Length == 4))
                    break;
            }
            else
            {
                parser.ReturnChar(next);

                if (count >= 0 || builder.Length == 0)
                {
                    parser.ReturnBuffer(builder);

                    return string.Empty;
                }

                break;
            }
        }

        int codePoint = Convert.ToInt32(builder.ToString(), 16);

        return char.ConvertFromUtf32(codePoint);
    }
}
