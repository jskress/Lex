using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class IdTokenizerTests : TokenizerTestsBase
{
    [TestMethod]
    public void TestAsIsIdParsing()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a", IdAToken);
        Verify(parser, "ab", IdAbToken);
        Verify(parser, "a1", new IdToken("a1"));
        Verify(parser, "Testing", new IdToken("Testing"));
        Verify(parser, "test_1", new IdToken("test_1"));
        Verify(parser, "1_Test", NumberToken.FromText("1"), new IdToken("_Test"));
    }

    [TestMethod]
    public void TestLowerCaseIdParsing()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser) { Style = LetterCaseStyle.LowerCase };
        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a", IdAToken);
        Verify(parser, "ab", IdAbToken);
        Verify(parser, "a1", new IdToken("a1"));
        Verify(parser, "Testing", new IdToken("testing"));
        Verify(parser, "test_1", new IdToken("test_1"));
        Verify(parser, "1_Test", NumberToken.FromText("1"), new IdToken("_test"));
    }

    [TestMethod]
    public void TestUpperCaseIdParsing()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser) { Style = LetterCaseStyle.UpperCase };
        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a", new IdToken("A"));
        Verify(parser, "ab", new IdToken("AB"));
        Verify(parser, "a1", new IdToken("A1"));
        Verify(parser, "Testing", new IdToken("TESTING"));
        Verify(parser, "test_1", new IdToken("TEST_1"));
        Verify(parser, "1_Test", NumberToken.FromText("1"), new IdToken("_TEST"));
    }
}
