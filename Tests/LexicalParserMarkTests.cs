using Lex;
using Lex.Parser;
using Lex.Tokenizers;

namespace Tests;

/// <summary>
/// These tests cover marking a position in the token stream and returning to it.
/// </summary>
[TestClass]
public class LexicalParserMarkTests : TestsBase
{
    private static LexicalParser CreateParser(string source)
    {
        LexicalParser parser = new ();

        _ = new NumberTokenizer(parser);
        _ = new IdTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource(source.AsReader());

        return parser;
    }

    private static List<string> Drain(LexicalParser parser)
    {
        List<string> texts = [];

        while (parser.GetNextToken() is { } token)
            texts.Add(token.Text);

        return texts;
    }

    [TestMethod]
    public void TestRollbackReturnsEverythingConsumed()
    {
        using LexicalParser parser = CreateParser("a + 1 ; b");

        parser.MarkPosition();

        for (int index = 0; index < 3; index++)
            _ = parser.GetNextToken();

        parser.RollbackToMark();

        CollectionAssert.AreEqual(
            new[] { "a", "+", "1", ";", "b" }, Drain(parser),
            "rollback should have put the stream back exactly as it was");
    }

    [TestMethod]
    public void TestReleaseKeepsEverythingConsumed()
    {
        using LexicalParser parser = CreateParser("a + 1 ; b");

        parser.MarkPosition();

        for (int index = 0; index < 3; index++)
            _ = parser.GetNextToken();

        parser.ReleaseMark();

        CollectionAssert.AreEqual(new[] { ";", "b" }, Drain(parser));
    }

    [TestMethod]
    public void TestMarksNest()
    {
        using LexicalParser parser = CreateParser("a + 1 ; b");

        parser.MarkPosition();

        Assert.AreEqual("a", parser.GetNextToken()?.Text);

        parser.MarkPosition();

        Assert.AreEqual("+", parser.GetNextToken()?.Text);
        Assert.AreEqual("1", parser.GetNextToken()?.Text);

        // Rolling the inner one back leaves the outer one's token consumed...
        parser.RollbackToMark();

        Assert.AreEqual("+", parser.GetNextToken()?.Text);

        // ...and rolling the outer one back takes everything, including what was re-read
        // after the inner rollback.
        parser.RollbackToMark();

        CollectionAssert.AreEqual(new[] { "a", "+", "1", ";", "b" }, Drain(parser));
    }

    /// <summary>
    /// Tokens handed back by hand must not also come back on a rollback, or they would land
    /// in the stream twice.  A peek is the everyday case, since it reads and returns.
    /// </summary>
    [TestMethod]
    public void TestTokensReturnedByHandAreNotRestoredTwice()
    {
        using LexicalParser parser = CreateParser("a + 1");

        parser.MarkPosition();

        Assert.AreEqual("a", parser.GetNextToken()?.Text);
        Assert.AreEqual("+", parser.PeekNextToken()?.Text);

        parser.ReturnToken(parser.GetNextToken());

        parser.RollbackToMark();

        CollectionAssert.AreEqual(new[] { "a", "+", "1" }, Drain(parser));
    }

    [TestMethod]
    public void TestResolvingAMarkThatIsNotThereFails()
    {
        using LexicalParser parser = CreateParser("a");

        Assert.AreEqual(
            "There is no marked position to roll back to.",
            Assert.ThrowsExactly<InvalidOperationException>(() => parser.RollbackToMark()).Message);

        Assert.AreEqual(
            "There is no marked position to release.",
            Assert.ThrowsExactly<InvalidOperationException>(() => parser.ReleaseMark()).Message);

        parser.MarkPosition();
        parser.ReleaseMark();

        Assert.ThrowsExactly<InvalidOperationException>(() => parser.ReleaseMark());
    }

    [TestMethod]
    public void TestRollbackAtTheEndOfInputIsHarmless()
    {
        using LexicalParser parser = CreateParser("a");

        parser.MarkPosition();

        Assert.AreEqual("a", parser.GetNextToken()?.Text);
        Assert.IsNull(parser.GetNextToken());

        parser.RollbackToMark();

        CollectionAssert.AreEqual(new[] { "a" }, Drain(parser));
    }
}
