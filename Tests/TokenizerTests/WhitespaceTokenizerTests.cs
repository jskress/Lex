using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class WhitespaceTokenizerTests : TokenizerTestsBase
{
    private static readonly Token Space = new WhitespaceToken(" ");
    private static readonly Token Tab = new WhitespaceToken("\t");
    private static readonly Token SpaceTab = new WhitespaceToken(" \t");
    private static readonly Token TabFormFeed = new WhitespaceToken("\t\f");
    private static readonly Token FormFeed = new WhitespaceToken("\f");
    private static readonly Token NewLine = new WhitespaceToken("\n");

    [TestMethod]
    public void TestBasics()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "ab", IdAbToken);
        Verify(parser, "a b", IdAToken, IdBToken);
        Verify(parser, "a\tb", IdAToken, IdBToken);
        Verify(parser, "a\fb", IdAToken, IdBToken);
        Verify(parser, "a\nb", IdAToken, IdBToken);
        Verify(parser, "a\rb", IdAToken, IdBToken);
        Verify(parser, "a\r\nb", IdAToken, IdBToken);
    }

    [TestMethod]
    public void TestNewLineSeparation()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser) { ReportTokens = true, ReportLineEndsSeparately = true };

        Verify(parser, " ", Space);
        Verify(parser, " \t", SpaceTab);
        Verify(parser, "\t\f", TabFormFeed);
        Verify(parser, " \n ", Space, NewLine, Space);
        Verify(parser, " \t\n ", SpaceTab, NewLine, Space);
        Verify(parser, " \t\n\t\f", SpaceTab, NewLine, TabFormFeed);
        Verify(parser, "a \n b", IdAToken, Space, NewLine, Space, IdBToken);
    }

    [TestMethod]
    public void TestReports()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser) { ReportTokens = true };

        Verify(parser, "ab", IdAbToken);
        Verify(parser, "a b", IdAToken, Space, IdBToken);
        Verify(parser, "a\tb", IdAToken, Tab, IdBToken);
        Verify(parser, "a\fb", IdAToken, FormFeed, IdBToken);
        Verify(parser, "a\nb", IdAToken, NewLine, IdBToken);
        Verify(parser, "a\rb", IdAToken, NewLine, IdBToken);
        Verify(parser, "a\r\nb", IdAToken, NewLine, IdBToken);
    }
}
