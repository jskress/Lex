using Lex;
using Lex.Clauses;
using Lex.Dsl;
using Lex.Expressions;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.ClauseTests;

/// <summary>
/// These tests cover backing out of a clause that has already consumed an expression.
/// </summary>
/// <remarks>
/// A clause that parses an expression gets a term back, not the tokens that went into it, so
/// it has nothing to hand back when a later term of the same clause fails to match.  That
/// used to mean it could not be rolled back at all, and a partial match threw rather than
/// letting the caller try its next alternative, so a switch like <c>[ interval | expression ]</c>
/// could never tell <c>(0, 3]</c> from <c>(balls)</c> by trying one and falling back.
/// </remarks>
[TestClass]
public class ExpressionBacktrackingTests : TestsBase
{
    private const string DslSpec = """"
        _parserSpec: """
            identifiers
            integral numbers
            predefined operators
            bounders
            whitespace
            """
        _operators: predefined
        _expressions:
        {
            term: [ _number, _identifier => 'id' ]
            binary: [ plus, minus, multiply, divide ]
        }
        interval: { [ leftParen | openBracket ] > _expression > comma > _expression > [ rightParen | closeBracket ] } => 'interval'
        thing: [ interval => 'interval' | _expression => 'expression' ]
        """";

    private static LexicalParser CreateParser()
    {
        LexicalParser parser = new ();

        _ = new NumberTokenizer(parser);
        _ = new IdTokenizer(parser);
        _ = new BounderTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        return parser;
    }

    private static ExpressionParser CreateExpressionParser()
    {
        return new ExpressionParser()
            .AddTermChoiceParser(new TermChoiceParser()
                .Matching(typeof(NumberToken)).WithTag("number"))
            .AddTermChoiceParser(new TermChoiceParser()
                .Matching(typeof(IdToken)).WithTag("id"))
            .AddBinaryOperatorParser([OperatorToken.Plus], OperatorPrecedence.Additive)
            .AddBinaryOperatorParser([OperatorToken.Minus], OperatorPrecedence.Additive)
            .AddBinaryOperatorParser([OperatorToken.Multiply], OperatorPrecedence.Multiplicative)
            .AddBinaryOperatorParser([OperatorToken.Divide], OperatorPrecedence.Multiplicative);
    }

    private static List<string> Drain(LexicalParser parser)
    {
        List<string> texts = [];

        while (parser.GetNextToken() is { } token)
            texts.Add(token.Text);

        return texts;
    }

    /// <summary>
    /// This builds the reported grammar: an interval, or failing that, a plain expression.
    /// </summary>
    private static SwitchClauseParser CreateGrammar()
    {
        ExpressionParser expressionParser = CreateExpressionParser();

        SequentialClauseParser interval = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(BounderToken.LeftParen, BounderToken.OpenBracket)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then(OperatorToken.Comma)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then(BounderToken.RightParen, BounderToken.CloseBracket)
            .OnMatchTag("interval");

        return new SwitchClauseParser()
            .Matching(interval, "interval")
            .Or(new ExpressionClauseParser(expressionParser), "expression");
    }

    [TestMethod]
    public void TestFallsThroughToTheNextAlternative()
    {
        LexicalParser parser = CreateParser();

        parser.SetSource("(balls)".AsReader());

        Clause clause = CreateGrammar().TryParse(parser);

        Assert.IsNotNull(clause, "should have fallen through to the expression alternative");
        Assert.AreEqual("expression", clause.Tag);
        Assert.AreEqual(1, clause.Expressions.Count);
        Assert.IsNull(parser.GetNextToken(), "the whole input should have been consumed");
    }

    [TestMethod]
    public void TestFallingThroughLeavesTrailingInputIntact()
    {
        LexicalParser parser = CreateParser();

        parser.SetSource("(balls) ; rest".AsReader());

        Clause clause = CreateGrammar().TryParse(parser);

        Assert.IsNotNull(clause);
        Assert.AreEqual("expression", clause.Tag);
        Assert.AreEqual(";", parser.GetNextToken()?.Text);
        Assert.AreEqual("rest", parser.GetNextToken()?.Text);
        Assert.IsNull(parser.GetNextToken());
    }

    [TestMethod]
    public void TestTheAlternativeThatShouldWinStillDoes()
    {
        foreach (string source in new[] { "(0, 3]", "[0, 3)", "(a + 1, b * 2]" })
        {
            LexicalParser parser = CreateParser();

            parser.SetSource(source.AsReader());

            Clause clause = CreateGrammar().TryParse(parser);

            Assert.IsNotNull(clause, $"parsing \"{source}\"");
            Assert.AreEqual("interval", clause.Tag, $"parsing \"{source}\"");
            Assert.AreEqual(2, clause.Expressions.Count, $"parsing \"{source}\"");
            Assert.IsNull(parser.GetNextToken(), $"parsing \"{source}\"");
        }
    }

    /// <summary>
    /// The second alternative here is a plain run of tokens, so it can only match if the
    /// rollback put every last token back, in the right order.
    /// </summary>
    [TestMethod]
    public void TestRollbackRestoresEveryTokenInOrder()
    {
        ExpressionParser expressionParser = CreateExpressionParser();

        SequentialClauseParser interval = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(BounderToken.LeftParen)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then(OperatorToken.Comma)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then(BounderToken.RightParen)
            .OnMatchTag("interval");

        SequentialClauseParser literal = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(BounderToken.LeftParen)
            .Then(new IdToken("a"))
            .Then(OperatorToken.Plus)
            .Then(new IdToken("b"))
            .Then(BounderToken.RightParen)
            .OnMatchTag("literal");

        SwitchClauseParser grammar = new SwitchClauseParser()
            .Matching(interval, "interval")
            .Or(literal, "literal");

        LexicalParser parser = CreateParser();

        parser.SetSource("(a + b) tail".AsReader());

        Clause clause = grammar.TryParse(parser);

        Assert.IsNotNull(clause, "the second alternative should have seen a pristine stream");
        Assert.AreEqual("literal", clause.Tag);
        Assert.AreEqual(5, clause.Tokens.Count);
        Assert.AreEqual("(a+b)", string.Concat(clause.Tokens.Select(token => token.Text)));
        Assert.AreEqual("tail", parser.GetNextToken()?.Text);
        Assert.IsNull(parser.GetNextToken());
    }

    /// <summary>
    /// Marks nest, so an inner clause backing out must not disturb the mark of the clause
    /// that contains it.
    /// </summary>
    [TestMethod]
    public void TestNestedClausesBackOutIndependently()
    {
        ExpressionParser expressionParser = CreateExpressionParser();

        SequentialClauseParser inner = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(BounderToken.LeftParen)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then(OperatorToken.Comma)
            .OnMatchTag("inner");

        SequentialClauseParser outer1 = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(inner)
            .Then(OperatorToken.SemiColon)
            .OnMatchTag("outer1");

        SequentialClauseParser outer2 = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(BounderToken.LeftParen)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then(OperatorToken.Comma)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then(BounderToken.RightParen)
            .OnMatchTag("outer2");

        SwitchClauseParser grammar = new SwitchClauseParser()
            .Matching(outer1, "outer1")
            .Or(outer2, "outer2");

        LexicalParser parser = CreateParser();

        parser.SetSource("(1, 2)".AsReader());

        Clause clause = grammar.TryParse(parser);

        Assert.IsNotNull(clause);
        Assert.AreEqual("outer2", clause.Tag);
        Assert.AreEqual(2, clause.Expressions.Count);
        Assert.IsNull(parser.GetNextToken());
    }

    /// <summary>
    /// A term carrying its own error message is still a hard error.  This matters for
    /// grammars written while the docs advised annotating every term after an expression,
    /// precisely because the clause could not back out; those must not quietly start
    /// backtracking instead of reporting what they always reported.
    /// </summary>
    [TestMethod]
    public void TestAnExplicitErrorMessageIsStillAHardError()
    {
        ExpressionParser expressionParser = CreateExpressionParser();

        SequentialClauseParser interval = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(BounderToken.LeftParen, BounderToken.OpenBracket)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then("Expecting a comma here.", OperatorToken.Comma)
            .Then(new ExpressionClauseParser(expressionParser))
            .Then("Expecting a closing bound here.", BounderToken.RightParen, BounderToken.CloseBracket)
            .OnMatchTag("interval");

        SwitchClauseParser grammar = new SwitchClauseParser()
            .Matching(interval, "interval")
            .Or(new ExpressionClauseParser(expressionParser), "expression");

        LexicalParser parser = CreateParser();

        parser.SetSource("(balls)".AsReader());

        AssertTokenException(() => grammar.TryParse(parser), "Expecting a comma here.");
    }

    /// <summary>
    /// The item leads with a token so that running out of input fails to match cleanly,
    /// rather than the expression throwing for want of a term.
    /// </summary>
    [TestMethod]
    public void TestRepeatRollsBackWhenItFallsShort()
    {
        ExpressionParser expressionParser = CreateExpressionParser();

        SequentialClauseParser item = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(OperatorToken.SemiColon)
            .Then(new ExpressionClauseParser(expressionParser))
            .OnMatchTag("item");

        RepeatingClauseParser repeating = new (item, min: 2, max: null);

        LexicalParser parser = CreateParser();

        parser.SetSource("; a".AsReader());

        Assert.IsNull(repeating.TryParse(parser), "the repeat fell short and must not match");

        Assert.AreEqual(";", parser.GetNextToken()?.Text, "the token went missing on rollback");
        Assert.AreEqual("a", parser.GetNextToken()?.Text, "the expression's token went missing");
        Assert.IsNull(parser.GetNextToken());
    }

    [TestMethod]
    public void TestRepeatErrorMessagePointsAtTheStartOfTheClause()
    {
        ExpressionParser expressionParser = CreateExpressionParser();

        SequentialClauseParser item = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(OperatorToken.SemiColon)
            .Then(new ExpressionClauseParser(expressionParser))
            .OnMatchTag("item");

        RepeatingClauseParser repeating = new (item, min: 2, max: null, errorMessage: "Need two items.");

        LexicalParser parser = CreateParser();

        parser.SetSource("; a".AsReader());

        TokenException exception = Assert.ThrowsExactly<TokenException>(
            () => repeating.TryParse(parser));

        Assert.AreEqual("Need two items.", exception.Message);
        Assert.AreEqual(";", exception.Token?.Text, "the error should point at the clause's start");
    }

    /// <summary>
    /// Rolling back has to cover every token the attempt consumed, not just the ones the
    /// clause got back.  A term choice may be told to suppress tokens from the term it
    /// builds, and those must still find their way back into the stream.
    /// </summary>
    [TestMethod]
    public void TestRollbackRestoresSuppressedTokens()
    {
        LexicalParser parser = CreateParser();

        parser.SetSource("[a] ,".AsReader());

        ExpressionParser expressionParser = new ExpressionParser()
            .AddTermChoiceParser(new TermChoiceParser()
                .Matching(BounderToken.OpenBracket, suppress: true)
                .Then(typeof(IdToken))
                .Then(BounderToken.CloseBracket, suppress: true)
                .WithTag("bracketed"));

        // The semicolon isn't there, so the clause has to back out of the expression.
        SequentialClauseParser clause = (SequentialClauseParser) new SequentialClauseParser()
            .Matching(new ExpressionClauseParser(expressionParser))
            .Then(OperatorToken.SemiColon)
            .OnMatchTag("nope");

        Assert.IsNull(clause.TryParse(parser));

        CollectionAssert.AreEqual(
            new[] { "[", "a", "]", "," },
            Drain(parser),
            "the suppressed brackets should have come back too");
    }

    /// <summary>
    /// The same grammar, written the way a consumer actually writes one.
    /// </summary>
    [TestMethod]
    public void TestFallingThroughWorksInTheDsl()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom(DslSpec);

        foreach ((string source, string expectedTag) in new[]
                 {
                     ("(0, 3]", "interval"),
                     ("[1, 2)", "interval"),
                     ("(balls)", "expression"),
                     ("balls", "expression"),
                     ("(a + b)", "expression")
                 })
        {
            using LexicalParser parser = dsl.CreateLexicalParser();

            parser.SetSource(source.AsReader());

            Clause clause = dsl.ParseClause(parser, "thing");

            Assert.IsNotNull(clause, $"parsing \"{source}\"");
            Assert.AreEqual(expectedTag, clause.Tag, $"parsing \"{source}\"");
            Assert.IsNull(parser.GetNextToken(), $"parsing \"{source}\"");
        }
    }
}
