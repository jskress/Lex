using Lex.Clauses;
using Lex.Parser;
using Lex.Tokenizers;

namespace Tests.ClauseTests;

[TestClass]
public class SequentialClauseTests : ClauseTestsBase
{
    [TestMethod]
    public void TestSequentialClause()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        SequentialClauseParser clauseParser = new SequentialClauseParser()
            .Matching(This)
            .Then(Thing);

        Verify(parser, "this thing", clauseParser, null, new Clause
        {
            Tokens = [This, Thing]
        });

        clauseParser = new SequentialClauseParser()
            .Matching(This)
            .Then(Thing)
            .OnMatchTag("Tagged");

        Verify(parser, "this thing", clauseParser, null, new Clause
        {
            Tag = "Tagged",
            Tokens = [This, Thing]
        });
    }

    [TestMethod]
    public void TestSequentialClauseErrors()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        SequentialClauseParser clauseParser = new SequentialClauseParser()
            .Matching("Needed this", This)
            .Then("Needed thing", Thing);

        AssertTokenException(
            () => Verify(parser, "that thing", clauseParser, null, null),
            "Needed this");

        AssertTokenException(
            () => Verify(parser, "this that", clauseParser, null, null),
            "Needed thing");
    }
}
