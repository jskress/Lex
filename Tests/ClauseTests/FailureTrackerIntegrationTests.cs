using Lex;
using Lex.Clauses;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.ClauseTests;

[TestClass]
public class FailureTrackerIntegrationTests
{
    private static readonly Token This = new IdToken("this");
    private static readonly Token That = new IdToken("that");
    private static readonly Token Number = new IdToken("number");

    [TestMethod]
    public void TestSwitchClauseAccumulatesDistinctAlternativesAtSamePosition()
    {
        LexicalParser parser = new ();
        FailureTracker tracker = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("nope".AsReader());
        parser.FailureTracker = tracker;

        SwitchClauseParser clauseParser = new SwitchClauseParser()
            .Matching(This)
            .Or(That)
            .Or(Number);

        Clause clause = clauseParser.TryParse(parser);

        Assert.IsNull(clause);
        CollectionAssert.AreEqual(
            new[]
            {
                "an identifier of \"this\"",
                "an identifier of \"that\"",
                "an identifier of \"number\""
            },
            tracker.Expectations.ToList());
    }

    [TestMethod]
    public void TestFurthestAlternativeSupersedesAnEarlierShallowerOne()
    {
        LexicalParser parser = new ();
        FailureTracker tracker = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("this nope".AsReader());
        parser.FailureTracker = tracker;

        // The first alternative fails immediately, on the first token.  The second gets
        // past that same first token and fails on the second one instead -- a deeper,
        // further-along failure that should supersede the first alternative's, rather than
        // just being added alongside it.
        SwitchClauseParser clauseParser = new SwitchClauseParser()
            .Matching(That)
            .Or(new SequentialClauseParser().Matching(This).Then(Number));

        Clause clause = clauseParser.TryParse(parser);

        Assert.IsNull(clause);
        CollectionAssert.AreEqual(
            new[] { "an identifier of \"number\"" },
            tracker.Expectations.ToList());
    }
}
