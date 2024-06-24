using Lex.Dsl;
using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class OperatorTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("dsl operators", "Data was requested from a DSL but no DSL was provided."),
        new ErrorEntry("operators", "Expecting a string or identifier here."),
        new ErrorEntry("predefined operators including", "Expecting a string or identifier here."),
        new ErrorEntry("predefined operators excluding 1", "Expecting a string or identifier here."),
        new ErrorEntry("predefined operators including 'a',", "Expecting a string or identifier here."),
        new ErrorEntry("predefined operators excluding 'a', 1", "Expecting a string or identifier here.")
    ];

    [TestMethod]
    public void TestOperatorsConfiguration()
    {
        Verify("predefined operators", "");
        Verify("predefined operators redact", "+");
        Verify("operators '+' redact", "+");
        Verify("predefined operators", "+",
            expectedTokens: OperatorToken.Plus);
        Verify("operators '+', '-'", "+-",
            expectedTokens: [OperatorToken.Plus, OperatorToken.Minus]);
    }

    [TestMethod]
    public void TestOperatorsConfigurationWithInclude()
    {
        Verify("predefined operators including 'a'", "a",
            expectedTokens: new OperatorToken("a"));
    }

    [TestMethod]
    public void TestOperatorsConfigurationWithExclude()
    {
        AssertTokenException(
            () => Verify("predefined operators excluding comma", ",",
                expectedTokens: new OperatorToken("a")),
            "The character, ',', is not recognized by any tokenizer.");
    }

    [TestMethod]
    public void TestOperatorsConfigurationFromDsl()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom("_operators: comma");

        Assert.IsNotNull(dsl.Operators);
        Assert.AreEqual(1, dsl.Operators.Length);
        Assert.AreSame(OperatorToken.Comma, dsl.Operators[0]);

        Verify("dsl operators", ",", dsl,
            expectedTokens: OperatorToken.Comma);
    }

    [TestMethod]
    public void TestOperatorsConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
