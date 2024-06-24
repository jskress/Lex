using Lex.Clauses;
using Lex.Parser;
using Lex.Tokenizers;

namespace Tests.ClauseTests;

[TestClass]
public class SwitchClauseTests : ClauseTestsBase
{
    [TestMethod]
    public void TestSwitchClause()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        SwitchClauseParser clauseParser = new SwitchClauseParser()
            .Matching(This)
            .Or(That)
            .Or(Other);

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

        Verify(parser, "thing thing", clauseParser, Thing, null);

        clauseParser = new SwitchClauseParser()
            .Matching("Tagged", null, This)
            .Or(That)
            .Or(Other);

        Verify(parser, "this thing", clauseParser, Thing, new Clause
        {
            Tag = "Tagged",
            Tokens = [This]
        });
    }

    [TestMethod]
    public void TestSwitchClauseErrors()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        SwitchClauseParser clauseParser = new SwitchClauseParser()
            .Matching(This)
            .Or(That)
            .Or(Other)
            .OnNoClausesMatched("Nothing matched");

        AssertTokenException(
            () => Verify(parser, "thing", clauseParser, null, null),
            "Nothing matched");
    }
}
