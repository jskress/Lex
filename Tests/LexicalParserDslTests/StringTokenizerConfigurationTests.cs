using Lex.Dsl;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class StringTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("single", "Expecting \"quoted\" to follow \"single\", \"double\" or \"triple\" here."),
        new ErrorEntry("single or", "Expecting \"quoted\" to follow \"single\", \"double\" or \"triple\" here."),
        new ErrorEntry("single quoted", "Expecting \"strings\" to follow \"quoted\" here."),
        new ErrorEntry("single quoted or", "Expecting \"strings\" to follow \"quoted\" here."),
        new ErrorEntry("double", "Expecting \"quoted\" to follow \"single\", \"double\" or \"triple\" here."),
        new ErrorEntry("double or", "Expecting \"quoted\" to follow \"single\", \"double\" or \"triple\" here."),
        new ErrorEntry("double quoted", "Expecting \"strings\" to follow \"quoted\" here."),
        new ErrorEntry("double quoted or", "Expecting \"strings\" to follow \"quoted\" here."),
        new ErrorEntry("triple", "Expecting \"quoted\" to follow \"single\", \"double\" or \"triple\" here."),
        new ErrorEntry("triple or", "Expecting \"quoted\" to follow \"single\", \"double\" or \"triple\" here."),
        new ErrorEntry("triple quoted", "Expecting \"strings\" to follow \"quoted\" here."),
        new ErrorEntry("triple quoted or", "Expecting \"strings\" to follow \"quoted\" here."),
        new ErrorEntry("triple quoted strings single", "Expecting \"line\" to follow \"single\" here."),
        new ErrorEntry("triple quoted strings single or", "Expecting \"line\" to follow \"single\" here."),
        new ErrorEntry("triple quoted strings not", "Expecting \"extensible\" to follow \"not\" here."),
        new ErrorEntry("triple quoted strings not or", "Expecting \"extensible\" to follow \"not\" here."),
        new ErrorEntry("strings", "Expecting \"bounded\" to follow \"strings\" here."),
        new ErrorEntry("strings or", "Expecting \"bounded\" to follow \"strings\" here."),
        new ErrorEntry("strings bounded", "Expecting \"by\" to follow \"bounded\" here."),
        new ErrorEntry("strings bounded or", "Expecting \"by\" to follow \"bounded\" here."),
        new ErrorEntry("strings bounded by", "Expecting a string to follow \"by\" here."),
        new ErrorEntry("strings bounded by or", "Expecting a string to follow \"by\" here."),
        new ErrorEntry("strings bounded by 'a' repeat", "Expecting \"to\" to follow \"repeat\" here."),
        new ErrorEntry("strings bounded by 'a' repeat or", "Expecting \"to\" to follow \"repeat\" here."),
        new ErrorEntry("strings bounded by 'a' repeat to", "Expecting \"escape\" to follow \"to\" here."),
        new ErrorEntry("strings bounded by 'a' repeat to or", "Expecting \"escape\" to follow \"to\" here."),
    ];

    [TestMethod]
    public void TestSingleQuotedStringsConfiguration()
    {
        Verify("single quoted strings", "");
        Verify("single quoted strings redact", "'a'");
        Verify("single quoted strings multiChar redact", "'abc'");
        Verify("single quoted strings", "'a'",
            expectedTokens: new StringToken("'", "a"));
        Verify("single quoted strings multiChar", "'abc'",
            expectedTokens: new StringToken("'", "abc"));
    }

    [TestMethod]
    public void TestDoubleQuotedStringsConfiguration()
    {
        // Verify("double quoted strings", "");
        Verify("double quoted strings redact", "\"abc\"");
        Verify("double quoted strings", "\"abc\"",
            expectedTokens: new StringToken("\"", "abc"));
    }

    [TestMethod]
    public void TestTripleQuotedStringsConfiguration()
    {
        Verify("triple quoted strings", "");
        Verify("triple quoted strings redact", "\"\"\"abc\"\"\"");
        Verify("triple quoted strings", "\"\"\"abc\"\"\"",
            expectedTokens: new StringToken("\"\"\"", "abc"));
        Verify("triple quoted strings single line", "\"\"\"abc\"\"\"",
            expectedTokens: new StringToken("\"\"\"", "abc"));
        Verify("triple quoted strings", "\"\"\"\"abc\"\"\"\"",
            expectedTokens: new StringToken("\"\"\"\"", "abc"));
        Verify("triple quoted strings not extensible", "\"\"\"\"abc\"\"\"",
            expectedTokens: new StringToken("\"\"\"", "\"abc"));

        LexicalParser parser = LexicalParserFactory.CreateFrom("triple quoted strings single line");
        StringTokenizer tokenizer = parser.GetTokenizer<StringTokenizer>();
        
        Assert.IsFalse(tokenizer.IsMultiLine);

        parser = LexicalParserFactory.CreateFrom("triple quoted strings not extensible");

        TripleQuotedStringTokenizer tripleQuotedStringTokenizer = parser.GetTokenizer<TripleQuotedStringTokenizer>();

        Assert.IsFalse(tripleQuotedStringTokenizer.MarkerCanExtend);
    }

    [TestMethod]
    public void TestStringsConfiguration()
    {
        Verify("strings bounded by '@'", "");
        Verify("strings bounded by '@' redact", "@abc@");
        Verify("strings bounded by '@'", "@abc@",
            expectedTokens: new StringToken("@", "abc"));

        LexicalParser parser = LexicalParserFactory.CreateFrom("strings bounded by '@'");
        StringTokenizer tokenizer = parser.GetTokenizer<StringTokenizer>();

        Assert.IsFalse(tokenizer.Raw);
        Assert.IsFalse(tokenizer.RepeatBounderEscapes);

        parser = LexicalParserFactory.CreateFrom("strings bounded by '@' raw repeat to escape");
        tokenizer = parser.GetTokenizer<StringTokenizer>();

        Assert.IsTrue(tokenizer.Raw);
        Assert.IsTrue(tokenizer.RepeatBounderEscapes);
    }

    [TestMethod]
    public void TestStringsConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
