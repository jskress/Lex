using Lex.Dsl;
using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class KeywordTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("dsl keywords", "Data was requested from a DSL but no DSL was provided."),
        new ErrorEntry("dsl keywords including", "Expecting a string or identifier here."),
        new ErrorEntry("dsl keywords excluding 1", "Expecting a string or identifier here."),
        new ErrorEntry("dsl keywords including 'a',", "Expecting a string or identifier here."),
        new ErrorEntry("dsl keywords excluding 'a', 1", "Expecting a string or identifier here.")
    ];

    private static readonly KeywordToken LowerSelect = new KeywordToken("select");
    private static readonly KeywordToken UpperSelect = new KeywordToken("SELECT");
    private static readonly KeywordToken MixedSelect = new KeywordToken("Select");

    [TestMethod]
    public void TestKeywordsConfiguration()
    {
        Verify("keywords 'select'", "");
        Verify("keywords 'select' redact", "select");
        Verify("keywords 'select'", "select",
            expectedTokens: LowerSelect);
        Verify("keywords 'Select' asIs", "Select",
            expectedTokens: MixedSelect);
        Verify("keywords 'select' lowerCase", "Select",
            expectedTokens: LowerSelect);
        Verify("keywords 'select' upperCase", "Select",
            expectedTokens: UpperSelect);
    }

    [TestMethod]
    public void TestKeywordsConfigurationWithInclude()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom("_keywords: 'select'");

        Verify("dsl keywords including 'from'", "from", dsl,
            expectedTokens: new KeywordToken("from"));
    }

    [TestMethod]
    public void TestKeywordsConfigurationWithExclude()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom("_keywords: 'select'");

        AssertTokenException(
            () => Verify("dsl keywords excluding select", "select", dsl,
                expectedTokens: new KeywordToken("select")),
            "The character, 's', is not recognized by any tokenizer.");
    }

    [TestMethod]
    public void TestKeywordsConfigurationFromDsl()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom("_keywords: 'select'");

        Assert.IsNotNull(dsl.Operators);
        Assert.AreEqual(1, dsl.Keywords.Length);
        Assert.IsTrue(LowerSelect.Matches(dsl.Keywords[0]));

        Verify("dsl keywords", "select", dsl,
            expectedTokens: LowerSelect);
    }

    [TestMethod]
    public void TestOperatorsConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
