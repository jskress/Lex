using Lex.Clauses;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This class implements a DSL (go figure) for defining and configuring a DSL.
/// </summary>
public static partial class LexicalDslFactory
{
    private const string BounderTypeText = "_bounder";
    private const string CommentTypeText = "_comment";
    private const string IdentifierTypeText = "_identifier";
    private const string KeywordTypeText = "_keyword";
    private const string NumberTypeText = "_number";
    private const string OperatorTypeText = "_operator";
    private const string StringTypeText = "_string";
    private const string WhitespaceTypeText = "_whitespace";

    private static readonly IdToken Bounder = new (BounderTypeText);
    private static readonly IdToken Comment = new (CommentTypeText);
    private static readonly IdToken Identifier = new (IdentifierTypeText);
    private static readonly IdToken Keyword = new (KeywordTypeText);
    private static readonly IdToken Number = new (NumberTypeText);
    private static readonly IdToken Operator = new (OperatorTypeText);
    private static readonly IdToken String = new (StringTypeText);
    private static readonly IdToken Whitespace = new (WhitespaceTypeText);

    private static readonly IdToken ParserSpec = new ("_parserSpec");
    private static readonly IdToken Keywords = new ("_keywords");
    private static readonly IdToken Operators = new ("_operators");
    private static readonly IdToken Expressions = new ("_expressions");

    private static readonly Token[] TypeTokenList =
    [
        Bounder, Comment, Identifier, Keyword, Number, Operator, String, Whitespace
    ];

    private static readonly Dictionary<string, Type> TokenTypesMap = new ()
    {
        { BounderTypeText, typeof(BounderToken) },
        { CommentTypeText, typeof(CommentToken) },
        { IdentifierTypeText, typeof(IdToken) },
        { KeywordTypeText, typeof(KeywordToken) },
        { NumberTypeText, typeof(NumberToken) },
        { OperatorTypeText, typeof(OperatorToken) },
        { StringTypeText, typeof(StringToken) },
        { WhitespaceTypeText, typeof(WhitespaceToken) }
    };

    private static readonly ClauseParser ParserSpecClause = new SequentialClauseParser()
        .Matching(ParserSpec)
        .Then(OperatorToken.Colon)
        .Then("Expecting a string here.", typeof(StringToken));
    private static readonly ClauseParser ExtraKeywordsClause = new RepeatingClauseParser(
        new SequentialClauseParser()
            .Matching(OperatorToken.Comma)
            .Then("Expecting a string here.", typeof(StringToken)));
    private static readonly ClauseParser KeywordListClause = new SequentialClauseParser()
        .Matching(Keywords)
        .Then(OperatorToken.Colon)
        .Then("Expecting a string here.", typeof(StringToken))
        .Then(ExtraKeywordsClause);
    private static readonly ClauseParser ExtraOperatorsClause = new RepeatingClauseParser(
        new SequentialClauseParser()
            .Matching(OperatorToken.Comma)
            .Then("Expecting an identifier here.", typeof(IdToken)));
    private static readonly ClauseParser OperatorListClause = new SequentialClauseParser()
        .Matching(Operators)
        .Then(OperatorToken.Colon)
        .Then("Expecting an identifier here.", typeof(IdToken))
        .Then(ExtraOperatorsClause);
    private static readonly ClauseParser ExpressionsClause = new SequentialClauseParser()
        .Matching(Expressions)
        .Then(OperatorToken.Colon)
        .Then("Expecting an open brace after \"_expressions:\"", BounderToken.OpenBrace);
    private static readonly ClauseParser TypeClauseParser = new SingleTokenClauseParser(TypeTokenList);
    private static readonly ClauseParser TokenClauseParser = new SequentialClauseParser()
        .Matching(TypeClauseParser)
        .Then(BounderToken.LeftParen)
        .Then("Expecting a string here.", typeof(StringToken))
        .Then("Expecting a right parenthesis here.", BounderToken.RightParen);
    private static readonly ClauseParser SetTagClauseParser = new RepeatingClauseParser(
        new SequentialClauseParser()
            .Matching(OperatorToken.DoubleArrow)
            .Then("Expecting a string here.", typeof(StringToken)),
        max: 1);
    private static readonly ClauseParser ErrorMessageClauseParser = new RepeatingClauseParser(
        new SequentialClauseParser()
            .Matching(OperatorToken.Coalesce)
            .Then("Expecting a string here.", typeof(StringToken)),
        max: 1);
    private static readonly SwitchClauseParser TermChoicesClauseParser = new SwitchClauseParser()
        .Matching(TokenClauseParser)
        .Or(TypeClauseParser)
        .Or(typeof(IdToken))
        .OnNoClausesMatched("Expecting a term here.");
    private static readonly ClauseParser RangeClauseParser = new SequentialClauseParser()
        .Matching(new RepeatingClauseParser(
            new SingleTokenClauseParser(typeof(NumberToken)), max: 1))
        .Then(OperatorToken.Range)
        .Then(new RepeatingClauseParser(
            new SingleTokenClauseParser(typeof(NumberToken)), max: 1));
    private static readonly ClauseParser RepetitionClauseParser = new RepeatingClauseParser(
        new SequentialClauseParser()
            .Matching(BounderToken.OpenBrace)
            .Then(new SwitchClauseParser()
                .Matching(OperatorToken.Plus)
                .Or(OperatorToken.Multiply)
                .Or(OperatorToken.If)
                .Or(RangeClauseParser)
                .Or(typeof(NumberToken))
                .OnNoClausesMatched("Expecting a range specification here."))
            .Then(ErrorMessageClauseParser)
            .Then("Expecting a closing brace here.", BounderToken.CloseBrace)
        , max: 1);
    private static readonly ClauseParser TermClauseParser = new SequentialClauseParser()
        .Matching(TermChoicesClauseParser)
        .Then(RepetitionClauseParser)
        .Then(ErrorMessageClauseParser);
    private static readonly ClauseParser RepeatingSwitchTermClauseParser = new RepeatingClauseParser(
        new SequentialClauseParser()
            .Matching(OperatorToken.Pipe)
            .Then(TermClauseParser)
            .Then(SetTagClauseParser));
    private static readonly ClauseParser SwitchClauseParser = new SequentialClauseParser()
        .Matching(BounderToken.OpenBracket)
        .Then(TermClauseParser)
        .Then(SetTagClauseParser)
        .Then(RepeatingSwitchTermClauseParser)
        .Then("Expecting a closing bracket here.", BounderToken.CloseBracket)
        .Then(ErrorMessageClauseParser);
    private static readonly ClauseParser RepeatingSequentialTermClauseParser = new RepeatingClauseParser(
        new SequentialClauseParser()
            .Matching(OperatorToken.GreaterThan)
            .Then(TermClauseParser));
    private static readonly ClauseParser SequentialClauseParser = new SequentialClauseParser()
        .Matching(BounderToken.OpenBrace)
        .Then(TermClauseParser)
        .Then(RepeatingSequentialTermClauseParser)
        .Then("Expecting a closing brace here.", BounderToken.CloseBrace);

    // Definition clauses
    private static readonly ClauseParser DefineTokenClauseParser = new SequentialClauseParser()
        .Matching(typeof(IdToken))
        .Then("Expecting a colon here.", OperatorToken.Colon)
        .Then(TypeClauseParser)
        .Then("Expecting a left parenthesis here.", BounderToken.LeftParen)
        .Then("Expecting a string here.", typeof(StringToken))
        .Then("Expecting a right parenthesis here.", BounderToken.RightParen);
    private static readonly ClauseParser DefineSequentialClauseParser = new SequentialClauseParser()
        .Matching(typeof(IdToken))
        .Then("Expecting a colon here.", OperatorToken.Colon)
        .Then(new RepeatingClauseParser(new SingleTokenClauseParser(OperatorToken.If), max: 1))
        .Then(SequentialClauseParser)
        .Then(SetTagClauseParser)
        .Then(RepetitionClauseParser);
    private static readonly ClauseParser DefineSwitchClauseParser = new SequentialClauseParser()
        .Matching(typeof(IdToken))
        .Then("Expecting a colon here.", OperatorToken.Colon)
        .Then(new RepeatingClauseParser(new SingleTokenClauseParser(OperatorToken.If), max: 1))
        .Then(SwitchClauseParser)
        .Then(ErrorMessageClauseParser)
        .Then(RepetitionClauseParser);

    // Top-level clause.
    private static readonly ClauseParser TopLevelClauseParser = new SwitchClauseParser()
        .Matching(ParserSpecClause, nameof(HandleParserSpecClause))
        .Or(KeywordListClause, nameof(HandleKeywordListClause))
        .Or(OperatorListClause, nameof(HandleOperatorListClause))
        .Or(ExpressionsClause, nameof(HandleExpressionsClause))
        .Or(DefineTokenClauseParser, nameof(HandleTokenClause))
        .Or(DefineSequentialClauseParser, nameof(HandleDefineSequentialClause))
        .Or(DefineSwitchClauseParser, nameof(HandleDefineSwitchClause))
        .Or(SwitchClauseParser, nameof(HandleTopLevelOrClause))
        .OnNoClausesMatched("Unexpected token found.");
    private static readonly Dsl OurDsl = new (TopLevelClauseParser);

    private static readonly Dictionary<string, Action<DslParsingContext>>
        TagHandlers = new ()
        {
            { nameof(HandleParserSpecClause), HandleParserSpecClause },
            { nameof(HandleKeywordListClause), HandleKeywordListClause },
            { nameof(HandleOperatorListClause), HandleOperatorListClause },
            { nameof(HandleExpressionsClause), HandleExpressionsClause },
            { nameof(HandleTokenClause), HandleTokenClause },
            { nameof(HandleDefineSequentialClause), HandleDefineSequentialClause },
            { nameof(HandleDefineSwitchClause), HandleDefineSwitchClause },
            { nameof(HandleTopLevelOrClause), HandleTopLevelOrClause }
        };

    static LexicalDslFactory()
    {
        // Add the necessary cross-references.
        TermChoicesClauseParser
            .Or(SwitchClauseParser)
            .Or(SequentialClauseParser);
    }

    /// <summary>
    /// This method is used to create a DSL from a language specification that will use
    /// the given parser.
    /// </summary>
    /// <param name="languageSpec">The specification for the DSL.</param>
    /// <returns>The prepared DSL.</returns>
    public static Dsl CreateFrom(string languageSpec)
    {
        using LexicalParser parser = CreateAndConfigureParser(languageSpec);
        DslParsingContext context = new DslParsingContext()
        {
            Parser = parser,
            Variables = CreateVariablePool(),
            Dsl = new Dsl()
        };

        while (!parser.IsAtEnd())
            ProcessNextClause(context, OurDsl.ParseNextClause(parser));

        FillInClauses(context);

        return context.Dsl;
    }

    /// <summary>
    /// This is a helper method for creating a variable pool to use while parsing a DSL
    /// specification.
    /// </summary>
    /// <returns>The initialized variable pool.</returns>
    private static Dictionary<string, object> CreateVariablePool()
    {
        return new Dictionary<string, object>(
            TokenTypesMap.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)),
            StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(BounderToken.LeftParen), BounderToken.LeftParen },
            { nameof(BounderToken.RightParen), BounderToken.RightParen },
            { nameof(BounderToken.OpenBracket), BounderToken.OpenBracket },
            { nameof(BounderToken.CloseBracket), BounderToken.CloseBracket },
            { nameof(BounderToken.OpenBrace), BounderToken.OpenBrace },
            { nameof(BounderToken.CloseBrace), BounderToken.CloseBrace }
        };
    }

    /// <summary>
    /// This method creates a lexical parser for parsing a DSL specification.
    /// </summary>
    /// <returns>An appropriate parser.</returns>
    private static LexicalParser CreateAndConfigureParser(string source)
    {
        LexicalParser parser = new ();

        _ = new CommentTokenizer(parser).AddStandardMarkers();
        _ = new IdTokenizer(parser);
        _ = new TripleQuotedStringTokenizer(parser);
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
    /// <param name="context">The current parsing context.</param>
    /// <param name="clause">The result of parsing the next clause.</param>
    private static void ProcessNextClause(DslParsingContext context, Clause clause)
    {
        if (TagHandlers.TryGetValue(clause.Tag, out Action<DslParsingContext> handler))
        {
            context.Tokens = [..clause.Tokens];

            handler(context);
        }
    }

    /// <summary>
    /// This method is used to fill in all our named sequential and switch clauses.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void FillInClauses(DslParsingContext context)
    {
        foreach (ClauseParserSource<SequentialClauseParser> source in context.SequentialClauseSources)
        {
            context.Tokens = source.Tokens;

            CreateSequentialClause(context, source.Parser);

            source.Stored.SetDebugging(source.Debug);
        }

        foreach (ClauseParserSource<SwitchClauseParser> source in context.SwitchClauseSources)
        {
            context.Tokens = source.Tokens;

            CreateSwitchClause(context, source.Parser);

            source.Stored.SetDebugging(source.Debug);
        }
    }

    /// <summary>
    /// This method is used to process the parser specification clause.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void HandleParserSpecClause(DslParsingContext context)
    {
        context.Dsl.SetLexicalParserSpec(context.Tokens[2].Text);
    }

    /// <summary>
    /// This method is used to process the keywords specification clause.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void HandleKeywordListClause(DslParsingContext context)
    {
        foreach (string text in context.Tokens[2..]
                     .Where(token => token is StringToken)
                     .Select(token => token.Text))
        {
            KeywordToken token = new KeywordToken(text);

            context.Variables[text] = token;
            context.Dsl.AddKeyword(token);
        }
    }

    /// <summary>
    /// This method is used to process the operators specification clause.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void HandleOperatorListClause(DslParsingContext context)
    {
        foreach (string text in context.Tokens[2..]
                     .Where(token => token is IdToken)
                     .Select(token => token.Text))
        {
            string lowered = text.ToLowerInvariant();

            if (lowered == "predefined")
            {
                foreach (KeyValuePair<string,OperatorToken> pair in OperatorToken.NamedOperators)
                {
                    context.Variables[pair.Key] = pair.Value;
                    context.Dsl.AddOperator(pair.Value);
                }
            }
            else if (OperatorToken.NamedOperators.TryGetValue(lowered, out OperatorToken token))
            {
                context.Variables[text] = token;
                context.Dsl.AddOperator(token);
            }
            else if (context.Variables.TryGetValue(text, out object value) &&
                     value is OperatorToken operatorToken)
                context.Dsl.AddOperator(operatorToken);
        }
    }

    /// <summary>
    /// This method is used to process the definition of a type or token term.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void HandleTokenClause(DslParsingContext context)
    {
        string variableName = context.Tokens[0].Text;

        Token token = CreateToken(context.Tokens[2].Text, (StringToken) context.Tokens[4]);

        context.Variables[variableName] = token;

        switch (token)
        {
            case KeywordToken keywordToken:
                context.Dsl.AddKeyword(keywordToken);
                break;
            case OperatorToken operatorToken:
                context.Dsl.AddOperator(operatorToken);
                break;
        }
    }

    /// <summary>
    /// This is a helper method for creating a token from a type name and value.
    /// </summary>
    /// <param name="typeName">The name of the type of token desired.</param>
    /// <param name="valueToken">The string token that holds the value for the token.</param>
    /// <returns>The created token.</returns>
    private static Token CreateToken(string typeName, StringToken valueToken)
    {
        string value = valueToken.Text;

        return typeName switch
        {
            BounderTypeText => new BounderToken(value[0]),
            CommentTypeText => new CommentToken(value),
            IdentifierTypeText => new IdToken(value),
            KeywordTypeText => new KeywordToken(value),
            NumberTypeText => NumberToken.FromText(value),
            OperatorTypeText => new OperatorToken(value),
            StringTypeText => new StringToken(valueToken.Bounder, value),
            WhitespaceTypeText => new WhitespaceToken(value),
            _ => throw new ArgumentException($"Unsupported token type {typeName} found.", nameof(typeName))
        };
    }

    /// <summary>
    /// This method is used to process the definition of an and clause.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void HandleDefineSequentialClause(DslParsingContext context)
    {
        StoreEmptyClauseParser(context, context.SequentialClauseSources);
    }

    /// <summary>
    /// This method is used to process the definition of an or clause.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void HandleDefineSwitchClause(DslParsingContext context)
    {
        StoreEmptyClauseParser(context, context.SwitchClauseSources);
    }

    /// <summary>
    /// This is a helper method used to create and store an empty sequential or switch
    /// clause parser.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <param name="setAsideList">The list to set aside the clause's tokens for later
    /// processing.</param>
    private static void StoreEmptyClauseParser<TParser>(
        DslParsingContext context, List<ClauseParserSource<TParser>> setAsideList)
        where TParser : ClauseParser, new()
    {
        List<Token> tokens = context.Tokens;
        Token nameToken = tokens[0];
        string variableName = nameToken.Text;
        bool debug = false;

        tokens.RemoveRange(0, 2);

        if (tokens[0].Matches(OperatorToken.If))
        {
            debug = true;

            tokens.RemoveFirst();
        }

        TParser clauseParser = new TParser();
        (List<Token> clauseTokens, List<Token> repeatTokens) = IsolateRepeatClauseTokens(tokens);
        (_, _, _, ClauseParser storedParser) = WrapWithRepeatIfNeeded(
            repeatTokens, null, null, null, clauseParser);

        storedParser.Named(variableName);

        context.Dsl.AddNamedClauseParser(variableName, storedParser);
        context.Variables[variableName] = storedParser;

        setAsideList.Add(new ClauseParserSource<TParser>(
            storedParser, clauseParser, clauseTokens, debug));
    }

    /// <summary>
    /// This is a helper method for cleaving off any repeat clause there might be.
    /// </summary>
    /// <param name="tokens">The tokens to start with.</param>
    /// <returns>A tuple containing the token list for the main clause only and a separate
    /// list with any repeat clause tokens.</returns>
    private static (List<Token>, List<Token>) IsolateRepeatClauseTokens(List<Token> tokens)
    {
        Token starter = tokens.First();
        Token ender = BounderToken.OpenBracket.Matches(starter)
            ? BounderToken.CloseBracket
            : BounderToken.CloseBrace;
        int depth = 0;
        int index;

        // This loop will scan the tokens for the final closing bounder.
        for (index = 0; index < tokens.Count; index++)
        {
            if (starter.Matches(tokens[index]))
                depth++;
            else if (ender.Matches(tokens[index]))
            {
                depth--;
                
                if (depth == 0)
                    break;
            }
        }

        // Point to after the main clause.  If there's anything after it, it will
        // need to be a repeat clause.
        index = Math.Min(index + 1, tokens.Count);

        if (index < tokens.Count - 1 &&
            OperatorToken.Coalesce.Matches(tokens[index]) &&
            tokens[index + 1] is StringToken)
            index += 2;

        return (tokens[..index], tokens[index..]);
    }

    /// <summary>
    /// This method is used to process the definition of a top-level or clause.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    private static void HandleTopLevelOrClause(DslParsingContext context)
    {
        try
        {
            context.Dsl.SetTopLevelClause(CreateSwitchClause(context)
                .Named("Top Level Clause"));
        }
        catch (ArgumentException exception)
        {
            throw new TokenException(exception.Message)
            {
                Token = context.Tokens[0]
            };
        }
    }

    /// <summary>
    /// This method creates an "or" (switch) clause parser.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <param name="parser">The parser to fill in, if it already exists.</param>
    /// <returns>The configured switch clause.</returns>
    private static ClauseParser CreateSwitchClause(
        DslParsingContext context, SwitchClauseParser parser = null)
    {
        bool first = true;

        parser ??= new SwitchClauseParser();

        context.Tokens.RemoveFirst();

        do
        {
            (Token token, Type type, string errorMessage, ClauseParser clauseParser) =
                ParseTerm(context);
            string tag = null;

            if (OperatorToken.DoubleArrow.Matches(context.Tokens.FirstOrDefault()))
                (_, tag) = (context.Tokens.RemoveFirst(), context.Tokens.RemoveFirst().Text);

            if (token != null)
            {
                if (first)
                    parser.Matching(tag, errorMessage, token);
                else
                    parser.Or(tag, errorMessage, token);
            }
            else if (type != null)
            {
                if (first)
                    parser.Matching(tag, errorMessage, type);
                else
                    parser.Or(tag, errorMessage, type);
            }
            else
            {
                if (first)
                    parser.Matching(clauseParser, tag);
                else
                    parser.Or(clauseParser, tag);
            }

            first = false;

        } while (OperatorToken.Pipe.Matches(context.Tokens.RemoveFirst()));

        // If we're here, we've just eaten the closing bracket, so check for an error message.
        if (OperatorToken.Coalesce.Matches(context.Tokens.FirstOrDefault()))
        {
            (_, string onNoMatch) = (context.Tokens.RemoveFirst(), context.Tokens.RemoveFirst().Text);

            parser.OnNoClausesMatched(onNoMatch);
        }

        return WrapWithRepeatIfNeeded(context.Tokens, null, null, null, parser).Item4;
    }

    /// <summary>
    /// This method creates an "and" (sequence) clause parser.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <param name="parser">The parser to fill in, if it already exists.</param>
    /// <returns>The configured sequential clause.</returns>
    private static ClauseParser CreateSequentialClause(
        DslParsingContext context, SequentialClauseParser parser = null)
    {
        bool first = true;

        parser ??= new SequentialClauseParser();

        context.Tokens.RemoveFirst();

        do
        {
            (Token token, Type type, string errorMessage, ClauseParser clauseParser) =
                ParseTerm(context);

            if (token != null)
            {
                if (first)
                    parser.Matching(errorMessage, token);
                else
                    parser.Then(errorMessage, token);
            }
            else if (type != null)
            {
                if (first)
                    parser.Matching(errorMessage, type);
                else
                    parser.Then(errorMessage, type);
            }
            else
            {
                if (first)
                    parser.Matching(clauseParser);
                else
                    parser.Then(clauseParser);
            }

            first = false;

        } while (OperatorToken.GreaterThan.Matches(context.Tokens.RemoveFirst()));

        // If we're here, we've just eaten the closing brace, so check for a tag.
        if (OperatorToken.DoubleArrow.Matches(context.Tokens.FirstOrDefault()))
        {
            (_, string tag) = (context.Tokens.RemoveFirst(), context.Tokens.RemoveFirst().Text);

            parser.OnMatchTag(tag);
        }

        return WrapWithRepeatIfNeeded(context.Tokens, null, null, null, parser).Item4;
    }

    /// <summary>
    /// This method is used to determine what sort of term is next in the given token list
    /// and create the appropriate representation.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <returns>A tuple containing one of a token, a type or a clause parser. For the first
    /// two, an error message may also be provided.</returns>
    private static (Token, Type, string, ClauseParser) ParseTerm(DslParsingContext context)
    {
        (Token token, Type type, string errorMessage, ClauseParser parser) =
            ParseBasicTerm(context);

        switch (token)
        {
            case KeywordToken keywordToken:
                context.Dsl.AddKeyword(keywordToken);
                break;
            case OperatorToken operatorToken:
                context.Dsl.AddOperator(operatorToken);
                break;
        }

        return WrapWithRepeatIfNeeded(context.Tokens, token, type, errorMessage, parser);
    }

    /// <summary>
    /// This method is used to create a basic term from our token stream.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <returns>A tuple containing one of a token, a type or a clause parser. For the first
    /// two, an error message may also be provided.</returns>
    private static (Token, Type, string errorMessage, ClauseParser) ParseBasicTerm(
        DslParsingContext context)
    {
        if (BounderToken.OpenBracket.Matches(context.Tokens.FirstOrDefault()))
            return (null, null, null, CreateSwitchClause(context));

        if (BounderToken.OpenBrace.Matches(context.Tokens.FirstOrDefault()))
            return (null, null, null, CreateSequentialClause(context));

        Token token = context.Tokens.RemoveFirst();
        Type type = null;
        ClauseParser parser = null;
        string text = token.Text;
        string errorMessage = null;

        // Handle expression references.
        if (text == "_expression")
        {
            if (context.Dsl.ExpressionParser == null)
            {
                throw new TokenException("The DSL does not contain the specification for an expression.")
                {
                    Token = token
                };
            }

            parser = new ExpressionClauseParser(context.Dsl.ExpressionParser);
            token = null;
        }
        // Handle built-in type usage.
        else if (text[0] == '_' && TokenTypesMap.TryGetValue(text, out type))
            (token, type) = ResolveTypeReference(context.Tokens, text, type);
        else
            (token, type, parser) = ResolveVariableReference(context.Variables, token, text);

        if (OperatorToken.Coalesce.Matches(context.Tokens.FirstOrDefault()))
            (_, errorMessage) = (context.Tokens.RemoveFirst(), context.Tokens.RemoveFirst().Text);

        return (token, type, errorMessage, parser);
    }

    /// <summary>
    /// This is a helper method for resolving the source into either a type or a newly
    /// created token based on a type.
    /// </summary>
    /// <param name="tokens">The list of tokens to interpret.</param>
    /// <param name="text">The text of the token that represents the desired type.</param>
    /// <param name="type">The C# type we're dealing with.</param>
    /// <returns>A tuple containing the resolved token or type.</returns>
    private static (Token, Type) ResolveTypeReference(List<Token> tokens, string text, Type type)
    {
        Token token = tokens.FirstOrDefault();

        // Do we need to create a token to match against?
        if (BounderToken.LeftParen.Matches(token))
        {
            (_, StringToken value, _) = (
                tokens.RemoveFirst(), tokens.RemoveFirst() as StringToken, tokens.RemoveFirst());

            token = CreateToken(text, value);
            type = null;
        }
        // Nope, so it's a type.
        else
            token = null;

        return (token, type);
    }

    /// <summary>
    /// This is a helper method for resolving a variable name to what it represents.
    /// </summary>
    /// <param name="variables">The variable pool to pull from.</param>
    /// <param name="token">The token that represents the variable name.</param>
    /// <param name="text">The actual name of the variable.</param>
    /// <returns>A tuple that contains the token, type or clause parser, that the variable
    /// name points to.</returns>
    private static (Token, Type, ClauseParser) ResolveVariableReference(
        IDictionary<string, object> variables, Token token, string text)
    {
        Type type = null;
        ClauseParser parser = null;

        if (!variables.TryGetValue(text, out object value))
        {
            throw new TokenException($"There is no variable defined with the name, '{text}'.")
            {
                Token = token
            };
        }

        token = null;

        switch (value)
        {
            case Token tokenValue:
                token = tokenValue;
                break;
            case Type typeValue:
                type = typeValue;
                break;
            case ClauseParser clauseParser:
                parser = clauseParser;
                break;
            default:
                throw new Exception($"Programming issue; can't handle variable of type {value.GetType().FullName}.");
        }

        return (token, type, parser);
    }

    /// <summary>
    /// This method is used to apply an optional repetition specification to a term.
    /// </summary>
    /// <param name="tokens">The current list of tokens.</param>
    /// <param name="token">The token, if the term is a token.</param>
    /// <param name="type">The type, if the term is a type.</param>
    /// <param name="errorMessage">The error message to go with the token or type.</param>
    /// <param name="parser">The clause parser, if the term is a parser.</param>
    /// <returns>A tuple containing details about the (potentially) wrapped term.</returns>
    private static (Token, Type, string, ClauseParser) WrapWithRepeatIfNeeded(
        List<Token> tokens, Token token, Type type, string errorMessage, ClauseParser parser)
    {
        Token next = tokens.FirstOrDefault();

        if (!BounderToken.OpenBrace.Matches(next))
            return (token, type, errorMessage, parser);

        tokens.RemoveFirst();

        next = tokens.FirstOrDefault();

        RepeatingClauseParser repeatingClauseParser;
        string repeatingErrorMessage = null;
        int min = 0;
        int? max = null;
        bool removeSingleSymbol = true;

        if (OperatorToken.If.Matches(next))
            max = 1;
        else if (OperatorToken.Plus.Matches(next))
            min = 1;
        else if (!OperatorToken.Multiply.Matches(next))
        {
            (min, max) = ParseMinMax(tokens);
            removeSingleSymbol = false;
        }

        if (removeSingleSymbol)
            tokens.RemoveFirst();

        if (OperatorToken.Coalesce.Matches(tokens.FirstOrDefault()))
            (_, repeatingErrorMessage) = (tokens.RemoveFirst(), tokens.RemoveFirst().Text);

        if (token != null)
        {
            repeatingClauseParser = new RepeatingClauseParser(
                new SingleTokenClauseParser(errorMessage: errorMessage, token), min, max, errorMessage);
        }
        else if (type != null)
        {
            repeatingClauseParser = new RepeatingClauseParser(
                new SingleTokenClauseParser(errorMessage: errorMessage, type), min, max, errorMessage);
        }
        else
            repeatingClauseParser = new RepeatingClauseParser(parser, min, max, repeatingErrorMessage);

        next = tokens.FirstOrDefault();

        if (!BounderToken.CloseBrace.Matches(next))
        {
            throw new TokenException("Expecting a close brace, \"}\", here.")
            {
                Token = next
            };
        }

        // Now remove the closing brace.
        tokens.RemoveFirst();

        return (null, null, null, repeatingClauseParser);
    }

    /// <summary>
    /// This is a helper method for interpreting the given tokens to pull out a range
    /// clause.
    /// </summary>
    /// <param name="tokens">The tokens to interpret.</param>
    /// <param name="defaultMin">The default value for the minimum.</param>
    /// <param name="defaultMax">The default value for the maximum.</param>
    /// <returns>A tuple containing the min and max represented by the token list.</returns>
    private static (int, int?) ParseMinMax(List<Token> tokens, int defaultMin = 0, int? defaultMax = null)
    {
        Token next = tokens.FirstOrDefault();
        int min = defaultMin;
        int? max = defaultMax;
        
        Token errorToken = next;

        if (next is NumberToken ninNumberToken)
        {
            tokens.RemoveFirst();

            min = (int) ninNumberToken.IntegralNumber;
            next = tokens.FirstOrDefault();

            if (!OperatorToken.Range.Matches(next))
                return (min, min);
        }

        if (!OperatorToken.Range.Matches(next))
        {
            throw new TokenException("Expecting the range operator, \"..\", here.")
            {
                Token = next
            };
        }

        tokens.RemoveFirst(); // Eat the range operator.

        if (tokens.FirstOrDefault() is NumberToken maxNumberToken)
        {
            tokens.RemoveFirst();

            max = (int) maxNumberToken.IntegralNumber;
        }

        if (min < 0)
        {
            throw new TokenException($"Invalid range specified, minimum of {min} cannot be less than 0.")
            {
                Token = errorToken
            };
        }

        if (min > max)
        {
            throw new TokenException($"Invalid range specified, minimum of {min} is greater than maximum of {max}.")
            {
                Token = errorToken
            };
        }

        return (min, max);
    }
}
