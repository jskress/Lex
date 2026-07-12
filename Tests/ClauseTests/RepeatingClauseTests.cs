using Lex;
using Lex.Clauses;
using Lex.Dsl;
using Lex.Parser;
using Lex.Tokenizers;

namespace Tests.ClauseTests;

[TestClass]
public class RepeatingClauseTests : ClauseTestsBase
{
    [TestMethod]
    public void TestConstructionErrors()
    {
        SequentialClauseParser parser = new SequentialClauseParser();
        Exception exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => new RepeatingClauseParser(null));
        
        Assert.AreEqual("Value cannot be null. (Parameter 'wrapped')", exception.Message);

        exception = Assert.ThrowsExactly<ArgumentException>(
            () => new RepeatingClauseParser(parser, 2, 1));
        
        Assert.AreEqual("Min (2) cannot be larger than max (1).", exception.Message);
    }

    [TestMethod]
    public void TestAnyNumberOfRepeats()
    {
        LexicalParser parser = new();
        SingleTokenClauseParser singleTokenClauseParser = new SingleTokenClauseParser(This, That);

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        RepeatingClauseParser clauseParser = new RepeatingClauseParser(singleTokenClauseParser);

        Verify(parser, "thing", clauseParser, Thing, new Clause
        {
            Tokens = []
        });

        Verify(parser, "this thing", clauseParser, Thing, new Clause
        {
            Tokens = [This]
        });

        Verify(parser, "this that thing", clauseParser, Thing, new Clause
        {
            Tokens = [This, That]
        });

        Verify(parser, "this that this thing", clauseParser, Thing, new Clause
        {
            Tokens = [This, That, This]
        });
    }

    [TestMethod]
    public void TestAtMostOne()
    {
        LexicalParser parser = new();
        SingleTokenClauseParser singleTokenClauseParser = new SingleTokenClauseParser(This, That);

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        RepeatingClauseParser clauseParser = new RepeatingClauseParser(
            singleTokenClauseParser, max: 1);

        Verify(parser, "thing", clauseParser, Thing, new Clause
        {
            Tokens = []
        });

        Verify(parser, "this thing", clauseParser, Thing, new Clause
        {
            Tokens = [This]
        });

        Verify(parser, "this that thing", clauseParser, That, new Clause
        {
            Tokens = [This]
        });
    }

    [TestMethod]
    public void TestAtLeastTwo()
    {
        LexicalParser parser = new();
        SingleTokenClauseParser singleTokenClauseParser = new SingleTokenClauseParser(This, That);

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        RepeatingClauseParser clauseParser = new RepeatingClauseParser(
            singleTokenClauseParser, min: 2);

        Verify(parser, "thing", clauseParser, Thing, null);
        Verify(parser, "this thing", clauseParser, This, null);
        Verify(parser, "this that thing", clauseParser, Thing, new Clause
        {
            Tokens = [This, That]
        });

        clauseParser = new RepeatingClauseParser(
            singleTokenClauseParser, min: 2, errorMessage: "Not enough repeats");

        AssertTokenException(
            () => Verify(parser, "this thing", clauseParser, This, null),
            "Not enough repeats");
    }

    [TestMethod]
    public void TestNonProgressGuard()
    {
        LexicalParser parser = new();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("thing".AsReader());

        // A clause parser that (incorrectly) reports a match without consuming any input.
        // Even with a small, bounded maximum, RepeatingClauseParser must refuse to accept
        // this rather than silently looping (or, worse, looping forever for an unbounded
        // repeat).
        RepeatingClauseParser clauseParser = new (new ZeroWidthMatchClauseParser(), min: 0, max: 3);

        AssertException(
            () => clauseParser.TryParse(parser),
            "The wrapped ZeroWidthMatchClauseParser matched without consuming any input; " +
            "this would result in an infinite loop.");
    }

    /// <summary>
    /// This clause parser deliberately violates the <see cref="ClauseParser"/> contract by
    /// reporting a successful match without consuming anything from the parser.  It exists
    /// solely to exercise <see cref="RepeatingClauseParser"/>'s guard against that.
    /// </summary>
    private class ZeroWidthMatchClauseParser : ClauseParser
    {
        protected override Clause TryParseClause(LexicalParser parser)
        {
            return new Clause { Tokens = [], Expressions = [] };
        }
    }

    [TestMethod]
    public void TestDslParsing()
    {
        VerifyRange("?", 0, 1);
        VerifyRange("+", 1, null);
        VerifyRange("*", 0, null);
        VerifyRange("..5", 0, 5);
        VerifyRange("2..", 2, null);
        VerifyRange("1..5", 1, 5);
        VerifyRange("5", 5, 5);
    }

    /// <summary>
    /// This is a helper method for verifying DSL parsing of ranges.
    /// </summary>
    /// <param name="spec">The DSL spec to parse.</param>
    /// <param name="minimum">The expected minimum.</param>
    /// <param name="maximum">The expected maximum</param>
    private static void VerifyRange(string spec, int minimum, int? maximum)
    {
        Dsl dsl = LexicalDslFactory.CreateFrom($$"""
            _keywords: 'word'
            clause: { word { {{spec}} } }
            """);
        Dictionary<string, ClauseParser> clauses = dsl.GetClauses();
        SequentialClauseParser sequentialClauseParser =
            (SequentialClauseParser) clauses["clause"];
        RepeatingClauseParser repeatingClauseParser =
            (RepeatingClauseParser) sequentialClauseParser.Children[0];
        (int min, int? max) = repeatingClauseParser.GetMinMax();

        Assert.AreEqual(minimum, min);
        Assert.AreEqual(maximum, max);
    }
}
