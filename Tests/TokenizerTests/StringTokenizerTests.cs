using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class StringTokenizerTests : TokenizerTestsBase
{
    [TestMethod]
    public void TestSingleQuoteTokenizer()
    {
        LexicalParser parser = new();
        SingleQuotedStringTokenizer tokenizer = new (parser);

        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "'a'", new StringToken("'", "a"));
        Verify(parser, "'\\u0021'", new StringToken("'", "!"));

        tokenizer.Raw = true;

        Verify(parser, "'\\u0021'", new StringToken("'", "\\u0021"));

        AssertTokenException(
            () => Verify(parser, "''"),
            "Character literal is empty.");
        AssertTokenException(
            () => Verify(parser, "'aa'"),
            "Character literal has too many characters.");

        tokenizer.RepresentsCharacter = false;

        Verify(parser, "'abc'", new StringToken("'", "abc"));

        tokenizer.RepeatBounderEscapes = true;

        Verify(parser, "'don''t'", new StringToken("'", "don't"));
    }

    [TestMethod]
    public void TestDoubleQuoteTokenizer()
    {
        LexicalParser parser = new();
        DoubleQuotedStringTokenizer tokenizer = new (parser);

        _ = new WhitespaceTokenizer(parser);

        Verify(parser, "\"a\"", new StringToken("\"", "a"));
        Verify(parser, "\"\\u0021\"", new StringToken("\"", "!"));

        tokenizer.Raw = true;

        Verify(parser, "\"\\u0021\"", new StringToken("\"", @"\u0021"));

        tokenizer.Raw = false;
        tokenizer.EscapeCharacter = '+';

        Verify(parser, "\"+u0021\"", new StringToken("\"", "!"));
    }

    [TestMethod]
    public void TestTripleQuoteTokenizer()
    {
        LexicalParser parser = new();
        TripleQuotedStringTokenizer tokenizer = new (parser);

        _ = new WhitespaceTokenizer(parser);

        Verify(parser, """"
                       """a"""
                       """", new StringToken("\"\"\"", "a"));
        Verify(parser, """"
                       """
                       a
                       b
                       """
                       """", new StringToken("\"\"\"", "a\nb\n"));
        Verify(parser, """""
                       """"
                       a
                       b
                       """"
                       """"", new StringToken("\"\"\"\"", "a\nb\n"));

        tokenizer.MarkerCanExtend = false;

        Verify(parser, """""
                       """"
                       a
                       b
                       """
                       """"", new StringToken("\"\"\"", "\"\na\nb\n"));
    }

    [TestMethod]
    public void TestErrorChecking()
    {
        LexicalParser parser = new ();

        AssertArgumentException(
            () => _ = new StringTokenizer(parser, "( )"),
            "Whitespace is not allowed in a string literal bounder.");
        AssertArgumentException(
            () => _ = new StringTokenizer(parser, ""),
            "String literal bounders cannot be empty.");

        LexicalParser parser2 = new ();
        _ = new DoubleQuotedStringTokenizer(parser2);

        AssertTokenException(
            () => Verify(parser2, """
                                  "a
                                  b"
                                  """.Trim()),
            "String literal not properly terminated.");
    }
}
