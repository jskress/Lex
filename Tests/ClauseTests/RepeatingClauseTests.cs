using Lex.Clauses;
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
        Exception exception = Assert.ThrowsException<ArgumentNullException>(
            () => new RepeatingClauseParser(null));
        
        Assert.AreEqual("Value cannot be null. (Parameter 'wrapped')", exception.Message);

        exception = Assert.ThrowsException<ArgumentException>(
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
}
