using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class CommentTokenizerTests : TokenizerTestsBase
{
    // Some useful constants.
    private static readonly Token CommentToken1 = new CommentToken("/* comment */");
    private static readonly Token CommentToken2 = new CommentToken("// comment\n");

    [TestMethod]
    public void TestStandardMarkers()
    {
        LexicalParser parser = new ();

        _ = new CommentTokenizer(parser).AddStandardMarkers();
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a/* comment */b", IdAToken, IdBToken);
        Verify(parser, "a /* comment */ b", IdAToken, IdBToken);
        Verify(parser, "a /* comment", IdAToken);
        Verify(parser, "a // comment\nb", IdAToken, IdBToken);
        Verify(parser, "a // comment\n b", IdAToken, IdBToken);
        Verify(parser, "a // comment", IdAToken);
    }

    [TestMethod]
    public void TestCustomMarkers()
    {
        LexicalParser parser = new ();

        _ = new CommentTokenizer(parser).AddMarkers("<<", ">>").AddLineCommentMarker("!!");
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a<< comment >>b", IdAToken, IdBToken);
        Verify(parser, "a << comment >> b", IdAToken, IdBToken);
        Verify(parser, "a << comment", IdAToken);
        Verify(parser, "a !! comment\nb", IdAToken, IdBToken);
        Verify(parser, "a !! comment\n b", IdAToken, IdBToken);
        Verify(parser, "a !! comment", IdAToken);
    }

    [TestMethod]
    public void TestReporting()
    {
        LexicalParser parser = new ();

        _ = new CommentTokenizer(parser){ ReportTokens = true }.AddStandardMarkers();
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a/* comment */b", IdAToken, CommentToken1, IdBToken);
        Verify(parser, "a // comment\nb", IdAToken, CommentToken2, IdBToken);
    }

    [TestMethod]
    public void TestErrorChecking()
    {
        LexicalParser parser = new ();
        CommentTokenizer tokenizer = new (parser);

        AssertArgumentException(
            () => tokenizer.AddMarkers(null, null),
            "Start comment marker cannot be empty.");
        AssertArgumentException(
            () => tokenizer.AddMarkers("//", null),
            "End comment marker cannot be empty.");
        AssertArgumentException(
            () => tokenizer.AddMarkers("/ /", null),
            "Whitespace is not allowed in a starting comment marker.");
        AssertArgumentException(
            () => tokenizer.AddMarkers("/*", "/ /"),
            "Whitespace is not allowed in an ending comment marker.");
    }
}
