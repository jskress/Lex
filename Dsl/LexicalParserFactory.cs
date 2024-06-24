using Lex.Clauses;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This class implements a DSL (go figure) for defining and configuring a lexical parser.
/// </summary>
public static partial class LexicalParserFactory
{
    private static readonly Dictionary<string, Func<List<Token>, Dsl, LexicalParser, ISet<string>, Tokenizer>>
        TagHandlers = new ()
        {
            { nameof(HandleWhitespaceClause), HandleWhitespaceClause },
            { nameof(HandleBasedNumbersClause), HandleBasedNumbersClause },
            { nameof(HandleBoundersClause), HandleBoundersClause },
            { nameof(HandleSourcedOperatorsClause), HandleSourcedOperatorsClause },
            { nameof(HandleOperatorsClause), HandleOperatorsClause },
            { nameof(HandleIdentifiersClause), HandleIdentifiersClause },
            { nameof(HandleSourcedKeywordsClause), HandleSourcedKeywordsClause },
            { nameof(HandleKeywordsClause), HandleKeywordsClause },
            { nameof(HandleNumbersClause), HandleNumbersClause },
            { nameof(HandleStringsClause), HandleStringsClause },
            { nameof(HandleStandardCommentsClause), HandleStandardCommentsClause },
            { nameof(HandleCommentsClause), HandleCommentsClause }
        };

    private static readonly string[] IncludeExcludeOptions = ["including", "excluding"];

    /// <summary>
    /// This method is used to create a lexical parser from a parser specification.
    /// </summary>
    /// <param name="parserSpec">The specification for the parser to parse it.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <returns>The prepared DSL.</returns>
    public static LexicalParser CreateFrom(string parserSpec, Dsl dsl = null)
    {
        HashSet<string> usedTypes = [];
        LexicalParser result = new ();

        using LexicalParser parser = CreateAndConfigureParser(parserSpec);

        while (!parser.IsAtEnd())
            ProcessNextClause(ParserDsl.ParseNextClause(parser), dsl, result, usedTypes);

        return result;
    }

    /// <summary>
    /// This method creates a lexical parser for parsing our DSL DSL.
    /// </summary>
    /// <returns>An appropriate parser.</returns>
    private static LexicalParser CreateAndConfigureParser(string source)
    {
        LexicalParser parser = new ();

        _ = new CommentTokenizer(parser).AddStandardMarkers();
        _ = new KeywordTokenizer(parser, ParserDsl.Keywords);
        _ = new IdTokenizer(parser);
        _ = new SingleQuotedStringTokenizer(parser)
        {
            RepresentsCharacter = false
        };
        _ = new DoubleQuotedStringTokenizer(parser);
        _ = new NumberTokenizer(parser)
        {
            SupportFraction = false,
            SupportScientificNotation = false
        };
        _ = new BounderTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource(source.AsReader());

        return parser;
    }

    /// <summary>
    /// This method is used to interpret the next clause from the source DSL specification.
    /// </summary>
    /// <param name="clause">The result of parsing the next clause.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    private static void ProcessNextClause(
        Clause clause, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        if (TagHandlers.TryGetValue(clause.Tag, out Func<List<Token>, Dsl, LexicalParser, ISet<string>, Tokenizer> handler))
        {
            List<Token> tokens = [..clause.Tokens];
            Tokenizer tokenizer = handler.Invoke(tokens, dsl, parser, usedTypes);

            if (tokens.Count > 0)
                tokenizer.ReportTokens = tokens[0].Text == "report";
        }
    }

    /// <summary>
    /// This is a helper method that ensures we only add one instance of any given tokenizer
    /// type to a parser.
    /// </summary>
    /// <param name="type">The type of tokenizer that should be noted as used.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <param name="tokens">The list of tokens for the tokenizer to add.</param>
    private static void EnsureFirstTime(Type type, ISet<string> usedTypes, IEnumerable<Token> tokens)
    {
        string typeName = type.FullName;

        if (!usedTypes.Add(typeName!))
        {
            string message = $"A tokenizer of type {typeName} has already been added to the parser.";

            throw new TokenException(message) { Token = tokens.FirstOrDefault() };
        }
    }

    /// <summary>
    /// This is a helper method that will take the given list of tokens and parse out the
    /// representative list of strings.
    /// </summary>
    /// <param name="tokens">The list of tokens to parse.</param>
    /// <returns>The resulting list of strings.</returns>
    private static List<Token> GetStringOrIdList(List<Token> tokens)
    {
        List<Token> result = [tokens.RemoveFirst()];

        while (OperatorToken.Comma.Matches(tokens.FirstOrDefault()))
        {
            tokens.RemoveFirst();

            result.Add(tokens.RemoveFirst());
        }

        return result;
    }
}
