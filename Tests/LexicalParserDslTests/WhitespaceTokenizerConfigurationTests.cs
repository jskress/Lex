using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class WhitespaceTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("whitespace with", "Expecting \"separated\" to follow \"with\" here."),
        new ErrorEntry("whitespace with or", "Expecting \"separated\" to follow \"with\" here."),
        new ErrorEntry("whitespace with separated", "Expecting \"lineEnds\" to follow \"separated\" here."),
        new ErrorEntry("whitespace with separated or", "Expecting \"lineEnds\" to follow \"separated\" here.")
    ];

    [TestMethod]
    public void TestWhitespaceConfiguration()
    {
        Verify("whitespace", "");
        Verify("whitespace", "    ");
        Verify("whitespace with separated lineEnds", "    ");
        Verify("whitespace with separated lineEnds", "  \n  ");
        Verify("whitespace report", "    ",
            expectedTokens: new WhitespaceToken("    "));
        Verify("whitespace report", "  \n  ",
            expectedTokens: new WhitespaceToken("  \n  "));
        Verify("whitespace with separated lineEnds report", "    ",
            expectedTokens: new WhitespaceToken("    "));
        Verify("whitespace with separated lineEnds report", "  \n  ",
            expectedTokens: [new WhitespaceToken("  "),
            new WhitespaceToken("\n"),
            new WhitespaceToken("  ")]);
    }

    [TestMethod]
    public void TestWhitespaceConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
