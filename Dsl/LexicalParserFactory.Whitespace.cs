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
    /// This method is used to update the given parser with whitespace handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the whitespace statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleWhitespaceClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(WhitespaceTokenizer), usedTypes, tokens);

        bool hasWithClause = tokens.GetTokenText(1) == "with";

        tokens.RemoveRange(0, hasWithClause ? 4 : 1);

        return new WhitespaceTokenizer(parser)
        {
            ReportLineEndsSeparately = hasWithClause
        };
    }
}
