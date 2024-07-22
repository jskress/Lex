using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class BasedNumberTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("based", "Expecting \"numbers\" to follow \"based\" here."),
        new ErrorEntry("based numbers no", "Expecting \"hex\", \"octal\" or \"binary\" to follow \"no\" here."),
        new ErrorEntry("based numbers no or", "Expecting \"hex\", \"octal\" or \"binary\" to follow \"no\" here."),
        new ErrorEntry("based numbers no hex no hex", "The \"no hex\" clause has already been specified.")
    ];

    [TestMethod]
    public void TestBasedNumberConfiguration()
    {
        Verify("based numbers", "");
        Verify("based numbers redact", "");
        Verify("based numbers", "0x01",
            expectedTokens: new NumberToken("0x01", 1));
        Verify("based numbers redact", "0x01");
    }

    [TestMethod]
    public void TestBasedNumberConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
