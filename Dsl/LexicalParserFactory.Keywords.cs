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
    /// This method is used to update the given parser with sourced keywords handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the keywords statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleSourcedKeywordsClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(KeywordTokenizer), usedTypes, tokens);

        if (dsl == null)
        {
            throw new TokenException("Data was requested from a DSL but no DSL was provided.")
            {
                Token = tokens.FirstOrDefault()
            };
        }

        tokens.RemoveRange(0, 2);

        KeywordTokenizer tokenizer = new KeywordTokenizer(
            parser, dsl.Keywords.Select(token => token.Text).ToHashSet());

        if (tokens.Count > 0 && IncludeExcludeOptions.Contains(tokens[0].Text))
        {
            string option = tokens.RemoveFirst().Text;
            HashSet<string> strings = GetKeywordList(tokens);

            if (option == "including")
                tokenizer.Including(strings);
            else // excluding
                tokenizer.Excluding(strings);
        }

        ApplyAnyStyle(tokenizer, tokens);

        return tokenizer;
    }

    /// <summary>
    /// This method is used to update the given parser with explicit keywords handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the keywords statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleKeywordsClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(KeywordTokenizer), usedTypes, tokens);

        tokens.RemoveFirst();

        KeywordTokenizer tokenizer = new KeywordTokenizer(parser, GetKeywordList(tokens));

        ApplyAnyStyle(tokenizer, tokens);

        return tokenizer;
    }

    /// <summary>
    /// This is a helper method for converting a list of string or ID tokens into a set of
    /// strings.
    /// </summary>
    /// <param name="tokens">The tokens to interpret.</param>
    /// <returns>The set of strings.</returns>
    private static HashSet<string> GetKeywordList(List<Token> tokens)
    {
        return GetStringOrIdList(tokens)
            .Select(token =>
            {
                if (!token.Text.All(char.IsLetter))
                {
                    throw new TokenException($"Cannot use \"{token.Text}\" as a keyword as it contains non-letter characters.")
                    {
                        Token = token
                    };
                }

                return token.Text;
            })
            .ToHashSet();
    }

    /// <summary>
    /// This is a helper method for configuring any style for the keyword tokenizer.
    /// </summary>
    /// <param name="tokenizer">The tokenizer .</param>
    /// <param name="tokens">The current list of tokens.</param>
    private static void ApplyAnyStyle(KeywordTokenizer tokenizer, List<Token> tokens)
    {
        if (Enum.TryParse(tokens.GetTokenText(), true, out LetterCaseStyle letterCaseStyle))
        {
            tokenizer.Style = letterCaseStyle;

            tokens.RemoveFirst();
        }
    }
}
