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
    /// This method is used to update the given parser with numbers handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the numbers statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleNumbersClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(NumberTokenizer), usedTypes, tokens);

        bool supportFraction = true;
        bool supportLeadingSign = false;
        bool supportScientificNotation = true;

        switch (tokens.GetTokenText())
        {
            case "integral":
                supportFraction = supportScientificNotation = false;
                tokens.RemoveFirst();
                break;
            case "decimal":
                supportScientificNotation = false;
                tokens.RemoveFirst();
                break;
        }

        tokens.RemoveFirst();

        if (tokens.GetTokenText() == "with")
        {
            supportLeadingSign = true;
 
            tokens.RemoveRange(0, 2);
        }

        return new NumberTokenizer(parser)
        {
            SupportFraction = supportFraction,
            SupportLeadingSign = supportLeadingSign,
            SupportScientificNotation = supportScientificNotation
        };
    }
}
