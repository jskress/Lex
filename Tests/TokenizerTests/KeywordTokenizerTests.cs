using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class KeywordTokenizerTests : TokenizerTestsBase
{
    [TestMethod]
    public void TestAsIsKeywordParsing()
    {
        LexicalParser parser = new();

        _ = new KeywordTokenizer(parser, "Select", "Insert", "Update", "Delete");
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "Select insert from Insert",
            new KeywordToken("Select"),
            new IdToken("insert"),
            new IdToken("from"),
            new KeywordToken("Insert"));
    }

    [TestMethod]
    public void TestLowerCaseKeywordParsing()
    {
        LexicalParser parser = new();

        _ = new KeywordTokenizer(parser, "select", "insert", "update", "delete")
        {
            Style = LetterCaseStyle.LowerCase
        };
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "Select insert from Insert",
            new KeywordToken("select"),
            new KeywordToken("insert"),
            new IdToken("from"),
            new KeywordToken("insert"));
    }

    [TestMethod]
    public void TestUpperCaseKeywordParsing()
    {
        LexicalParser parser = new();

        _ = new KeywordTokenizer(parser, "SELECT", "INSERT", "UPDATE", "DELETE")
        {
            Style = LetterCaseStyle.UpperCase
        };
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "Select insert from Insert",
            new KeywordToken("SELECT"),
            new KeywordToken("INSERT"),
            new IdToken("from"),
            new KeywordToken("INSERT"));
    }
}
