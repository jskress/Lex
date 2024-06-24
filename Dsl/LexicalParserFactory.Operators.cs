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
    /// This method is used to update the given parser with sourced operators handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the operators statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleSourcedOperatorsClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(OperatorTokenizer), usedTypes, tokens);

        (Token source, _) = (tokens.RemoveFirst(), tokens.RemoveFirst());
        OperatorTokenizer tokenizer;

        if (source.Text == "predefined")
            tokenizer = new OperatorTokenizer(parser);
        else // dsl
        {
            if (dsl == null)
            {
                throw new TokenException("Data was requested from a DSL but no DSL was provided.")
                {
                    Token = source
                };
            }

            tokenizer = new OperatorTokenizer(
                parser, dsl.Operators.Select(token => token.Text).ToHashSet());
        }

        if (tokens.Count > 0 && IncludeExcludeOptions.Contains(tokens[0].Text))
        {
            string option = tokens.RemoveFirst().Text;
            HashSet<string> strings = GetOperatorList(tokens);

            if (option == "including")
                tokenizer.Including(strings);
            else // excluding
                tokenizer.Excluding(strings);
        }

        return tokenizer;
    }

    /// <summary>
    /// This method is used to update the given parser with explicit operators handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the operators statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleOperatorsClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(OperatorTokenizer), usedTypes, tokens);

        tokens.RemoveFirst();

        return new OperatorTokenizer(parser, GetOperatorList(tokens));
    }

    /// <summary>
    /// This is a helper method for converting a list of string or ID tokens into a set of
    /// strings.
    /// </summary>
    /// <param name="tokens">The tokens to interpret.</param>
    /// <returns>The set of strings.</returns>
    private static HashSet<string> GetOperatorList(List<Token> tokens)
    {
        HashSet<string> result = [];

        foreach (Token token in GetStringOrIdList(tokens))
        {
            if (token is StringToken)
                result.Add(token.Text);
            else // IdToken
            {
                if (OperatorToken.NamedOperators.TryGetValue(token.Text, out OperatorToken operatorToken))
                    result.Add(operatorToken.Text);
                else
                {
                    throw new TokenException($"There is no operator named {token.Text}")
                    {
                        Token = token
                    };
                }
            }
        }

        return result;
    }
}
