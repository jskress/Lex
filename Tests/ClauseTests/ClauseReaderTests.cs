using Lex.Clauses;
using Lex.Expressions;
using Lex.Tokens;

namespace Tests.ClauseTests;

[TestClass]
public class ClauseReaderTests
{
    private static readonly Token OpenParen = BounderToken.LeftParen;
    private static readonly Token CloseParen = BounderToken.RightParen;
    private static readonly Token Name = new IdToken("bob");
    private static readonly Token Age = new NumberToken("42", 42);

    [TestMethod]
    public void TestNullClauseRejected()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ClauseReader(null!));
    }

    [TestMethod]
    public void TestReadingTokensSequentially()
    {
        ClauseReader reader = new Clause { Tokens = [OpenParen, Name, Age, CloseParen] }.Reader();

        Assert.IsTrue(reader.HasMoreTokens);
        Assert.AreSame(OpenParen, reader.PeekToken());

        Assert.IsTrue(reader.SkipIfNextTextIs("("));
        Assert.IsFalse(reader.SkipIfNextTextIs("("));

        Assert.AreEqual("bob", reader.NextText());

        NumberToken age = reader.NextToken<NumberToken>();

        Assert.AreEqual(42, age.IntegralNumber);

        Assert.IsTrue(reader.SkipIfNextTextIs(")"));
        Assert.IsFalse(reader.HasMoreTokens);
        Assert.IsNull(reader.PeekToken());
    }

    [TestMethod]
    public void TestNextTokenThrowsWhenExhausted()
    {
        ClauseReader reader = new Clause { Tokens = [Name] }.Reader();

        reader.NextToken();

        Assert.ThrowsExactly<InvalidOperationException>(() => reader.NextToken());
    }

    [TestMethod]
    public void TestTypedNextTokenThrowsOnTypeMismatch()
    {
        ClauseReader reader = new Clause { Tokens = [Name] }.Reader();

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => reader.NextToken<NumberToken>());

        Assert.AreEqual(
            "Expecting the next token to be a number but found an identifier instead.",
            exception.Message);
    }

    [TestMethod]
    public void TestReadingExpressionsSequentially()
    {
        IExpressionTerm first = NoOpTerm.Instance;
        IExpressionTerm second = NoOpTerm.Instance;
        ClauseReader reader = new Clause { Expressions = [first, second] }.Reader();

        Assert.IsTrue(reader.HasMoreExpressions);
        Assert.AreSame(first, reader.PeekExpression());
        Assert.AreSame(first, reader.NextExpression());
        Assert.AreSame(second, reader.NextExpression());
        Assert.IsFalse(reader.HasMoreExpressions);
        Assert.IsNull(reader.PeekExpression());
    }

    [TestMethod]
    public void TestNextExpressionThrowsWhenExhausted()
    {
        ClauseReader reader = new Clause().Reader();

        Assert.ThrowsExactly<InvalidOperationException>(() => reader.NextExpression());
    }

    [TestMethod]
    public void TestReadingDoesNotMutateTheClause()
    {
        Clause clause = new () { Tokens = [Name, Age] };
        ClauseReader reader = clause.Reader();

        reader.NextToken();
        reader.NextToken();

        Assert.AreEqual(2, clause.Tokens.Count);

        // A fresh reader over the same clause starts over from the beginning.
        Assert.AreSame(Name, clause.Reader().NextToken());
    }
}
