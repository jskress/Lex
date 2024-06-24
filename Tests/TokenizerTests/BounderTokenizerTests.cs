using Lex;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class BounderTokenizerTests : TokenizerTestsBase
{
    // Some useful constants.
    private static readonly Token LeftAngleBracket = new BounderToken('<');
    private static readonly Token RightAngleBracket = new BounderToken('>');

    [TestMethod]
    public void TestStandardBounders()
    {
        LexicalParser parser = new ();

        _ = new BounderTokenizer(parser);
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a(b", IdAToken, BounderToken.LeftParen, IdBToken);
        Verify(parser, "a)b", IdAToken, BounderToken.RightParen, IdBToken);
        Verify(parser, "a[b", IdAToken, BounderToken.OpenBracket, IdBToken);
        Verify(parser, "a]b", IdAToken, BounderToken.CloseBracket, IdBToken);
        Verify(parser, "a{b", IdAToken, BounderToken.OpenBrace, IdBToken);
        Verify(parser, "a}b", IdAToken, BounderToken.CloseBrace, IdBToken);
        Verify(parser, "a({ab})b",
            IdAToken, BounderToken.LeftParen, BounderToken.OpenBrace, IdAbToken,
            BounderToken.CloseBrace, BounderToken.RightParen, IdBToken);
        Verify(parser, "a  (  {  ab  }  )  b",
            IdAToken, BounderToken.LeftParen, BounderToken.OpenBrace, IdAbToken,
            BounderToken.CloseBrace, BounderToken.RightParen, IdBToken);
    }

    [TestMethod]
    public void TestCustomBounders()
    {
        LexicalParser parser = new ();

        _ = new BounderTokenizer(parser, "<>");
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "a<b", IdAToken, LeftAngleBracket, IdBToken);
        Verify(parser, "a>b", IdAToken, RightAngleBracket, IdBToken);
        Verify(parser, "a<<ab>>b",
            IdAToken,
            LeftAngleBracket,
            LeftAngleBracket,
            IdAbToken,
            RightAngleBracket,
            RightAngleBracket,
            IdBToken);

        parser.SetSource("a(b".AsReader());

        AssertTokenException(
            () =>
            {
                Assert.IsTrue(IdAToken.Matches(parser.GetNextToken()));

                _ = parser.GetNextToken();
            },
            "The character, '(', is not recognized by any tokenizer.");
    }

    [TestMethod]
    public void TestErrorChecking()
    {
        LexicalParser parser = new ();

        ArgumentException exception = Assert.ThrowsException<ArgumentException>(
            () => _ = new BounderTokenizer(parser, "( )"));

        Assert.AreEqual("Whitespace is not allowed in the set of bounder characters.", exception.Message);
    }
}
