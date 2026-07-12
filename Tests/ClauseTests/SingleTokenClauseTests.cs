using Lex;
using Lex.Clauses;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.ClauseTests;

[TestClass]
public class SingleTokenClauseTests : ClauseTestsBase
{
    [TestMethod]
    public void TestFailureTrackerReceivesRejectedMatches()
    {
        LexicalParser parser = new ();
        SingleTokenClauseParser clauseParser = new (This, That, Other);
        FailureTracker tracker = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("thing".AsReader());
        parser.FailureTracker = tracker;

        Clause clause = clauseParser.TryParse(parser);

        Assert.IsNull(clause);
        Assert.AreEqual(1, tracker.Line);
        Assert.AreEqual(1, tracker.Column);
        CollectionAssert.AreEqual(
            new[] { "an identifier of \"this\" or an identifier of \"that\" or an identifier of \"other\"" },
            tracker.Expectations.ToList());
        Assert.AreEqual(
            "Expecting an identifier of \"this\" or an identifier of \"that\" or an identifier of \"other\" here.",
            tracker.BuildMessage());
    }

    [TestMethod]
    public void TestFailureTrackerIsNotConsultedWhenErrorMessageIsSet()
    {
        LexicalParser parser = new ();
        SingleTokenClauseParser clauseParser = new ("bad token", This, That, Other);
        FailureTracker tracker = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("thing".AsReader());
        parser.FailureTracker = tracker;

        AssertTokenException(() => clauseParser.TryParse(parser), "bad token");

        // The clause parser threw instead of reporting to the tracker.
        Assert.AreEqual(0, tracker.Expectations.Count);
    }

    [TestMethod]
    public void TestSingleTokenMatchingClause()
    {
        LexicalParser parser = new ();
        SingleTokenClauseParser clauseParser = new (This, That, Other);

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "this thing", clauseParser, Thing, new Clause
        {
            Tokens = [This]
        });
        Verify(parser, "that thing", clauseParser, Thing, new Clause
        {
            Tokens = [That]
        });
        Verify(parser, "other thing", clauseParser, Thing, new Clause
        {
            Tokens = [Other]
        });
        Verify(parser, "thing that", clauseParser, Thing, null);

        clauseParser = new SingleTokenClauseParser("bad token", This, That, Other);

        AssertTokenException(
            () => Verify(parser, "thing that", clauseParser, null, null),
            "bad token");
    }

    [TestMethod]
    public void TestSingleTokenTypeMatchingClause()
    {
        LexicalParser parser = new ();
        SingleTokenClauseParser clauseParser = new (typeof(IdToken), typeof(OperatorToken));

        _ = new IdTokenizer(parser);
        _ = new NumberTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "this thing", clauseParser, Thing, new Clause
        {
            Tokens = [This]
        });
        Verify(parser, "+ thing", clauseParser, Thing, new Clause
        {
            Tokens = [OperatorToken.Plus]
        });
        Verify(parser, "1 that", clauseParser, new NumberToken("1", 1), null);

        clauseParser = new SingleTokenClauseParser("bad token", typeof(IdToken), typeof(OperatorToken));

        AssertTokenException(
            () => Verify(parser, "1 that", clauseParser, null, null),
            "bad token");
    }
}
