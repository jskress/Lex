using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class BasedNumberTokenizerTests : TokenizerTestsBase
{
    [TestMethod]
    public void TestBinaryNumberParsing()
    {
        LexicalParser parser = new();

        _ = new BasedNumberTokenizer(parser);

        Verify(parser, "0b1", new NumberToken("0b1", 1));
        Verify(parser, "0B1101", new NumberToken("0B1101", 0b1101));

        AssertTokenException(
            () => Verify(parser, "0b2", new NumberToken("", 0)),
            "The character, '0', is not recognized by any tokenizer.");
        AssertTokenException(
            () => Verify(parser, "0b12", new NumberToken("0b1", 1)),
            "The character, '2', is not recognized by any tokenizer.");

        LexicalParser parser2 = new ();

        _ = new BasedNumberTokenizer(parser2, supportBinary: false);

        AssertTokenException(
            () => Verify(parser2, "0b1101", new NumberToken("", 0)),
            "The character, '0', is not recognized by any tokenizer.");
    }

    [TestMethod]
    public void TestOctalNumberParsing()
    {
        LexicalParser parser = new();

        _ = new BasedNumberTokenizer(parser);

        Verify(parser, "01", new NumberToken("01", 1));
        Verify(parser, "07", new NumberToken("07", 7));

        AssertTokenException(
            () => Verify(parser, "08", new NumberToken("", 0)),
            "The character, '0', is not recognized by any tokenizer.");
        AssertTokenException(
            () => Verify(parser, "018", new NumberToken("01", 1)),
            "The character, '8', is not recognized by any tokenizer.");

        LexicalParser parser2 = new ();

        _ = new BasedNumberTokenizer(parser2, supportOctal: false);

        AssertTokenException(
            () => Verify(parser2, "07", new NumberToken("", 0)),
            "The character, '0', is not recognized by any tokenizer.");
    }

    [TestMethod]
    public void TestHexNumberParsing()
    {
        LexicalParser parser = new();

        _ = new BasedNumberTokenizer(parser);
        _ = new IdTokenizer(parser);

        Verify(parser, "0x1", new NumberToken("0x1", 1));
        Verify(parser, "0X1A2b", new NumberToken("0X1A2b", 0x1a2b));

        AssertTokenException(
            () => Verify(parser, "0Xx", new NumberToken("", 0)),
            "The character, '0', is not recognized by any tokenizer.");

        Verify(parser, "0X1A2bx", new NumberToken("0X1A2b", 0x1a2b), new IdToken("x"));

        LexicalParser parser2 = new ();

        _ = new BasedNumberTokenizer(parser2, supportHex: false);

        AssertTokenException(
            () => Verify(parser2, "0x1A2b", new NumberToken("", 0)),
            "The character, '0', is not recognized by any tokenizer.");
    }
}
