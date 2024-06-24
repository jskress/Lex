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
    /// This method is used to update the given parser with identifiers handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the identifiers statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleIdentifiersClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(IdTokenizer), usedTypes, tokens);

        tokens.RemoveFirst();

        string starters = null;
        string members = null;

        if (tokens.GetTokenText() == "starting")
        {
            tokens.RemoveRange(0, 2);

            starters = GetCharactersFromList(tokens, IdTokenizer.DefaultStarters);
        }

        if (tokens.GetTokenText() == "containing")
        {
            tokens.RemoveFirst();

            members = GetCharactersFromList(tokens, IdTokenizer.DefaultMembers);
        }

        IdTokenizer tokenizer = new IdTokenizer(parser, starters, members);
        string style = tokens.GetTokenText();

        if (Enum.TryParse(style, true, out LetterCaseStyle letterCaseStyle))
        {
            tokenizer.Style = letterCaseStyle;

            tokens.RemoveFirst();
        }

        return tokenizer;
    }

    /// <summary>
    /// This is a helper method for converting a comma-delimited list into the appropriate
    /// string of characters that an identifier may either start with or contain.
    /// </summary>
    /// <param name="tokens">The list of tokens to interpret.</param>
    /// <param name="defaults">The string to use when the <c>defaults</c> keyword is encountered.</param>
    /// <returns></returns>
    private static string GetCharactersFromList(List<Token> tokens, string defaults)
    {
        List<char> characters = [];

        foreach (Token token in GetStringOrIdList(tokens))
            characters.AddRange(ResolveToText(token, defaults));

        characters.Sort();

        return new string(characters.Distinct().ToArray());
    }

    /// <summary>
    /// This method resolves a token to the set of characters it represents.
    /// </summary>
    /// <param name="token">The list of tokens to interpret.</param>
    /// <param name="defaults">The string to use when the <c>defaults</c> keyword is encountered.</param>
    /// <returns>The set of characters the token represents.</returns>
    private static string ResolveToText(Token token, string defaults)
    {
        if (token is StringToken)
            return token.Text;

        // it's a keyword...
        return token.Text switch
        {
            "defaults" => defaults,
            "lowerCase" => IdTokenizer.Lowers,
            "upperCase" => IdTokenizer.Uppers,
            "letters" => IdTokenizer.Letters,
            "digits" => IdTokenizer.Digits,
            _ => throw new TokenException
                ("Expecting \"defaults\", \"lowerCase\", \"upperCase\", \"letters\", " +
                 "\"digits\" or a string here.")
            {
                Token = token
            }
        };
    }
}
