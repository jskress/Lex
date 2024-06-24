using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This class implements a DSL (go figure) for defining and configuring a lexical parser.
/// </summary>
public static partial class LexicalParserFactory
{
    /// <summary>
    /// This method is used to update the given parser with strings handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the strings statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleStringsClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(StringTokenizer), usedTypes, tokens);

        StringTokenizer tokenizer;
        int count = 3;

        switch (tokens.GetTokenText())
        {
            case "single":
                (tokenizer, count) = CreateSingleQuotedTokenizer(parser, tokens);
                break;
            case "double":
                tokenizer = new DoubleQuotedStringTokenizer(parser);
                break;
            case "triple":
                (tokenizer, count) = CreateTripleQuotedTokenizer(parser, tokens);
                break;
            case "strings":
                (tokenizer, count) = CreateStringTokenizer(parser, tokens);
                break;
            default:
                throw new Exception("Programming error.  We shouldn't be here.");
        }

        if (tokens.GetTokenText(count) == "raw")
        {
            tokenizer.Raw = true;
            count++;
        }

        if (tokens.GetTokenText(count) == "repeat")
        {
            tokenizer.RepeatBounderEscapes = true;
            count += 3;
        }

        tokens.RemoveRange(0, count);

        return tokenizer;
    }

    /// <summary>
    /// This is a helper method for creating a single quoted string tokenizer.
    /// </summary>
    /// <param name="parser">The lexical parser geting configured.</param>
    /// <param name="tokens">The list of tokens to interpret.</param>
    /// <returns>The created and configured single quoted string tokenizer.</returns>
    private static (StringTokenizer, int) CreateSingleQuotedTokenizer(
        LexicalParser parser, List<Token> tokens)
    {
        bool multiChar = tokens.GetTokenText(3) == "multiChar";
        int count = multiChar ? 4 : 3;

        return (new SingleQuotedStringTokenizer(parser)
        {
            RepresentsCharacter = !multiChar
        }, count);
    }

    /// <summary>
    /// This is a helper method for creating a triple quoted string tokenizer.
    /// </summary>
    /// <param name="parser">The lexical parser geting configured.</param>
    /// <param name="tokens">The list of tokens to interpret.</param>
    /// <returns>The created and configured triple quoted string tokenizer.</returns>
    private static (StringTokenizer, int) CreateTripleQuotedTokenizer(
        LexicalParser parser, List<Token> tokens)
    {
        TripleQuotedStringTokenizer tokenizer = new TripleQuotedStringTokenizer(parser);
        int count = 3;

        if (tokens.GetTokenText(count) == "single")
        {
            tokenizer.IsMultiLine = false;
            count += 2;
        }

        if (tokens.GetTokenText(count) == "not")
        {
            tokenizer.MarkerCanExtend = false;
            count += 2;
        }

        return (tokenizer, count);
    }

    /// <summary>
    /// This is a helper method for creating a generic string tokenizer.
    /// </summary>
    /// <param name="parser">The lexical parser geting configured.</param>
    /// <param name="tokens">The list of tokens to interpret.</param>
    /// <returns>The created and configured string tokenizer.</returns>
    private static (StringTokenizer, int) CreateStringTokenizer(
        LexicalParser parser, List<Token> tokens)
    {
        StringTokenizer tokenizer = new StringTokenizer(parser, tokens.GetTokenText(3));
        int count = 4;

        if (tokens.GetTokenText(count) == "multiLine")
        {
            tokenizer.IsMultiLine = true;
            count++;
        }

        return (tokenizer, count);
    }
}
