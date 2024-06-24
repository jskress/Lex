using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class NumberTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("integral", "Expecting \"numbers\" to follow \"integral\" or \"decimal\" here."),
        new ErrorEntry("integral or", "Expecting \"numbers\" to follow \"integral\" or \"decimal\" here."),
        new ErrorEntry("decimal", "Expecting \"numbers\" to follow \"integral\" or \"decimal\" here."),
        new ErrorEntry("decimal or", "Expecting \"numbers\" to follow \"integral\" or \"decimal\" here."),
        new ErrorEntry("numbers with", "Expecting \"signs\" to follow \"with\" here."),
        new ErrorEntry("numbers with or", "Expecting \"signs\" to follow \"with\" here.")
    ];

    [TestMethod]
    public void TestNumbersConfiguration()
    {
        Verify("numbers", "");
        Verify("numbers redact", "1");
        Verify("numbers predefined operators", "+1",
            expectedTokens: [OperatorToken.Plus, NumberToken.FromText("1")]);
        Verify("numbers with signs predefined operators", "+1",
            expectedTokens: NumberToken.FromText("+1"));
        Verify("numbers predefined operators", "1.0",
            expectedTokens: NumberToken.FromText("1.0"));
        Verify("numbers predefined operators", "1.0e1",
            expectedTokens: NumberToken.FromText("1.0e1"));
        Verify("decimal numbers identifiers predefined operators", "1.0e1",
            expectedTokens: [
                NumberToken.FromText("1.0"), new IdToken("e1")]);
        Verify("integral numbers identifiers predefined operators", "1.0e1",
            expectedTokens: [
                NumberToken.FromText("1"), OperatorToken.Dot,
                NumberToken.FromText("0"), new IdToken("e1")]);
    }

    [TestMethod]
    public void TestNumbersConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
