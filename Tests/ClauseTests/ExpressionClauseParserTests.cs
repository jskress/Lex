using Lex;
using Lex.Clauses;
using Lex.Expressions;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.ClauseTests;

[TestClass]
public class ExpressionClauseParserTests : TestsBase
{
    private static ExpressionParser CreateNumberExpressionParser()
    {
        TermChoiceParser numberChoice = new TermChoiceParser()
            .Matching(typeof(NumberToken))
            .WithTag("number");

        return new ExpressionParser().AddTermChoiceParser(numberChoice);
    }

    [TestMethod]
    public void TestConstructionErrors()
    {
        Exception exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => new ExpressionClauseParser(null));

        Assert.AreEqual("Value cannot be null. (Parameter 'expressionParser')", exception.Message);
    }

    [TestMethod]
    public void TestRequiredExpressionThrowsWhenMissing()
    {
        LexicalParser parser = new ();

        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("   ".AsReader());

        ExpressionClauseParser clauseParser = new (CreateNumberExpressionParser());

        AssertTokenException(
            () => clauseParser.TryParse(parser),
            "Expecting a term here.");
    }

    [TestMethod]
    public void TestOptionalExpressionReturnsNullWhenMissing()
    {
        LexicalParser parser = new ();

        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("   ".AsReader());

        ExpressionClauseParser clauseParser = new ExpressionClauseParser(CreateNumberExpressionParser())
            .SetIsOptional(true);

        Assert.IsNull(clauseParser.TryParse(parser));
    }

    [TestMethod]
    public void TestOptionalExpressionMatchesWhenPresent()
    {
        LexicalParser parser = new ();

        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("42".AsReader());

        ExpressionClauseParser clauseParser = new ExpressionClauseParser(CreateNumberExpressionParser())
            .SetIsOptional(true);

        Clause clause = clauseParser.TryParse(parser);

        Assert.IsNotNull(clause);
        Assert.AreEqual(1, clause.Expressions.Count);
        Assert.IsNotNull(clause.Expressions[0]);
    }

    [TestMethod]
    public void TestUnboundedRepeatOfOptionalExpressionTerminatesWithMatches()
    {
        LexicalParser parser = new ();

        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("1 2 3".AsReader());

        RepeatingClauseParser clauseParser = new (
            new ExpressionClauseParser(CreateNumberExpressionParser()), min: 0, max: null);

        Clause clause = clauseParser.TryParse(parser);

        Assert.IsNotNull(clause);
        Assert.AreEqual(3, clause.Expressions.Count);
        Assert.IsTrue(clause.Expressions.TrueForAll(expression => expression != null));
    }

    [TestMethod]
    public void TestUnboundedRepeatOfOptionalExpressionTerminatesWithZeroMatches()
    {
        LexicalParser parser = new ();

        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource("   ".AsReader());

        RepeatingClauseParser clauseParser = new (
            new ExpressionClauseParser(CreateNumberExpressionParser()), min: 0, max: null);

        Clause clause = clauseParser.TryParse(parser);

        Assert.IsNotNull(clause);
        Assert.AreEqual(0, clause.Expressions.Count);
    }
}
