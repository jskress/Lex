using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class BounderTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("bounders of", "Expecting a string to follow \"of\" here."),
        new ErrorEntry("bounders of or", "Expecting a string to follow \"of\" here.")
    ];

    [TestMethod]
    public void TestBoundersConfiguration()
    {
        Verify("bounders", "");
        Verify("bounders of '[]'", "");
        Verify("bounders redact", "");
        Verify("bounders redact", "[");
        Verify("bounders of '[]' redact", "[");
        Verify("bounders of '[]'", "[",
            expectedTokens: new BounderToken('['));
        Verify("bounders of '[]'", "]",
            expectedTokens: new BounderToken(']'));
        Verify("bounders", "[{()}]",
            expectedTokens: [BounderToken.OpenBracket,
            BounderToken.OpenBrace,
            BounderToken.LeftParen,
            BounderToken.RightParen,
            BounderToken.CloseBrace,
            BounderToken.CloseBracket]);
    }

    [TestMethod]
    public void TestBoundersConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
