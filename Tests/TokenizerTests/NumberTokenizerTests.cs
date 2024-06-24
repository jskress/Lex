using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class NumberTokenizerTests : TokenizerTestsBase
{
    // Some useful constants.
    private static readonly IdToken LowerE = new ("e");
    private static readonly IdToken UpperE = new ("E");

    [TestMethod]
    public void TestIntegralNumberParsing()
    {
        LexicalParser parser = new();

        _ = new NumberTokenizer(parser)
        {
            SupportFraction = false
        };
        _ = new IdTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, " 12 ", new NumberToken("12", 12));
        Verify(parser, "+12", OperatorToken.Plus, new NumberToken("12", 12));
        Verify(parser, "-12", OperatorToken.Minus, new NumberToken("12", 12));
        Verify(parser, "12.", new NumberToken("12", 12), OperatorToken.Dot);
        Verify(parser, "12.7",
            new NumberToken("12", 12), OperatorToken.Dot,
            new NumberToken("7", 7));
        Verify(parser, "1e-2",
            new NumberToken("1", 1), LowerE,
            OperatorToken.Minus, new NumberToken("2", 2));
        Verify(parser, "1.42e3",
            new NumberToken("1", 1), OperatorToken.Dot,
            new NumberToken("42", 42), new IdToken("e3"));
        Verify(parser, "1.6E+5",
            new NumberToken("1", 1), OperatorToken.Dot,
            new NumberToken("6", 6), UpperE, OperatorToken.Plus,
            new NumberToken("5", 5));
    }

    [TestMethod]
    public void TestSignedIntegralNumberParsing()
    {
        LexicalParser parser = new();

        _ = new NumberTokenizer(parser)
        {
            SupportFraction = false,
            SupportLeadingSign = true
        };
        _ = new IdTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, " 12 ", new NumberToken("12", 12));
        Verify(parser, "+12", new NumberToken("+12", 12));
        Verify(parser, "-12", new NumberToken("-12", -12));
        Verify(parser, "12.", new NumberToken("12", 12), OperatorToken.Dot);
        Verify(parser, "12.7",
            new NumberToken("12", 12), OperatorToken.Dot,
            new NumberToken("7", 7));
        Verify(parser, "1e-2",
            new NumberToken("1", 1), LowerE,
            new NumberToken("-2", -2));
        Verify(parser, "1.42e3",
            new NumberToken("1", 1), OperatorToken.Dot,
            new NumberToken("42", 42), new IdToken("e3"));
        Verify(parser, "1.6E+5",
            new NumberToken("1", 1), OperatorToken.Dot,
            new NumberToken("6", 6), UpperE, new NumberToken("+5", 5));
    }

    [TestMethod]
    public void TestFractionalNumberParsing()
    {
        LexicalParser parser = new();

        _ = new NumberTokenizer(parser)
        {
            SupportScientificNotation = false
        };
        _ = new IdTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, " 12 ", new NumberToken("12", 12));
        Verify(parser, "+12", OperatorToken.Plus, new NumberToken("12", 12));
        Verify(parser, "-12", OperatorToken.Minus, new NumberToken("12", 12));
        Verify(parser, "12.", new NumberToken("12.", 12D));
        Verify(parser, "12.7", new NumberToken("12.7", 12.7));
        Verify(parser, "1e-2",
            new NumberToken("1", 1), LowerE,
            OperatorToken.Minus, new NumberToken("2", 2));
        Verify(parser, "1.42e3",
            new NumberToken("1.42", 1.42), new IdToken("e3"));
        Verify(parser, "1.6E+5",
            new NumberToken("1.6", 1.6), UpperE, OperatorToken.Plus,
            new NumberToken("5", 5));
    }

    [TestMethod]
    public void TestFractionalNumberWithSfParsing()
    {
        LexicalParser parser = new();

        _ = new NumberTokenizer(parser);
        _ = new IdTokenizer(parser);
        _ = new OperatorTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, " 12 ", new NumberToken("12", 12));
        Verify(parser, "+12", OperatorToken.Plus, new NumberToken("12", 12));
        Verify(parser, "-12", OperatorToken.Minus, new NumberToken("12", 12));
        Verify(parser, "12.", new NumberToken("12.", 12D));
        Verify(parser, "12.7", new NumberToken("12.7", 12.7));
        Verify(parser, "1e-2", new NumberToken("1e-2", 1e-2));
        Verify(parser, "1.42e3", new NumberToken("1.42e3", 1.42e3));
        Verify(parser, "1.6E+5", new NumberToken("1.6E+5", 1.6e5));
    }
}
