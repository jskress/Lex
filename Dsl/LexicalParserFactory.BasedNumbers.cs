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
    /// This method is used to update the given parser with based numbers handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the based numbers statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleBasedNumbersClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(BasedNumberTokenizer), usedTypes, tokens);

        tokens.RemoveRange(0, 2);

        Dictionary<string, bool> flags = new Dictionary<string, bool>()
        {
            { "hex", true },
            { "octal", true },
            { "binary", true }
        };

        while (tokens.GetTokenText() == "no")
        {
            string key = tokens[1].Text;

            if (flags[key])
            {
                throw new TokenException($"The \"no {key}\" clause has already been specified.")
                {
                    Token = tokens[0]
                };
            }

            flags[key] = false;

            tokens.RemoveRange(0, 2);
        }

        return new BasedNumberTokenizer(
            parser, flags["hex"], flags["octal"], flags["binary"]
        );
    }
}
