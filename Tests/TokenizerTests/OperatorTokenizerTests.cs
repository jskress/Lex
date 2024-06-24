using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

[TestClass]
public class OperatorTokenizerTests : TokenizerTestsBase
{
    [TestMethod]
    public void TestStandardOperators()
    {
        LexicalParser parser = new ();

        _ = new OperatorTokenizer(parser);
        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        Verify(parser, ">=", OperatorToken.GreaterThanOrEqual);
        Verify(parser, "=>", OperatorToken.DoubleArrow);
        Verify(parser, "---", OperatorToken.DoubleMinus, OperatorToken.Minus);
    }

    [TestMethod]
    public void TestErrorChecking()
    {
        LexicalParser parser = new ();

        AssertArgumentException(
            () => _ = new OperatorTokenizer(parser, new HashSet<string> { "= =" }),
            "Whitespace is not allowed in an operator.");
        AssertArgumentException(
            () =>
            {
                OperatorTokenizer tokenizer = new (parser);
                tokenizer.Including(new HashSet<string> { "= =" });
            },
            "Whitespace is not allowed in an operator.");
        AssertArgumentException(
            () => _ = new OperatorTokenizer(parser, new HashSet<string> { "=", null }),
            "Empty values are not allowed for an operator.");
        AssertArgumentException(
            () =>
            {
                OperatorTokenizer tokenizer = new (parser);
                tokenizer.Including(new HashSet<string> { "=", null });
            },
            "Empty values are not allowed for an operator.");
    }
}
