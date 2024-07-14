using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class IdTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("identifiers starting", "Expecting \"with\" to follow \"start\" here."),
        new ErrorEntry("identifiers starting or", "Expecting \"with\" to follow \"start\" here."),
        new ErrorEntry("identifiers starting with", "Expecting a string or keyword here."),
        new ErrorEntry("identifiers starting with or", "Expecting \"defaults\", \"lowerCase\", \"upperCase\", \"letters\", \"greekLowers\", \"greekUppers\", \"greekLetters\", \"digits\" or a string here."),
        new ErrorEntry("identifiers containing", "Expecting a string or keyword here."),
        new ErrorEntry("identifiers containing or", "Expecting \"defaults\", \"lowerCase\", \"upperCase\", \"letters\", \"greekLowers\", \"greekUppers\", \"greekLetters\", \"digits\" or a string here.")
    ];

    [TestMethod]
    public void TestIdentifiersConfiguration()
    {
        Verify("identifiers", "");
        Verify("identifiers redact", "Id");
        Verify("identifiers", "Id",
            expectedTokens: new IdToken("Id"));
        Verify("identifiers asIs", "Id",
            expectedTokens: new IdToken("Id"));
        Verify("identifiers lowerCase", "Id",
            expectedTokens: new IdToken("id"));
        Verify("identifiers upperCase", "Id",
            expectedTokens: new IdToken("ID"));
        Verify("identifiers starting with 'I'", "Id",
            expectedTokens: new IdToken("Id"));
        Verify("identifiers containing 'd'", "Id",
            expectedTokens: new IdToken("Id"));
        Verify("identifiers starting with 'I' containing 'd'", "Id",
            expectedTokens: new IdToken("Id"));
    }

    [TestMethod]
    public void TestIdentifiersConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
