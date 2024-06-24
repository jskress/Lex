
using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

[TestClass]
public class CommentTokenizerConfigurationTests : LexicalParserDslTestsBase
{
    private static readonly List<ErrorEntry> ErrorTests =
    [
        new ErrorEntry("standard", "Expecting \"comments\" to follow \"standard\" here."),
        new ErrorEntry("standard or", "Expecting \"comments\" to follow \"standard\" here."),
        new ErrorEntry("comments", "Expecting a \"bounded by\" clause here."),
        new ErrorEntry("comments or", "Expecting a \"bounded by\" clause here."),
        new ErrorEntry("comments bounded", "Expecting \"by\" to follow \"bounded\" here."),
        new ErrorEntry("comments bounded or", "Expecting \"by\" to follow \"bounded\" here."),
        new ErrorEntry("comments bounded by", "Expecting a string here."),
        new ErrorEntry("comments bounded by or", "Expecting a string here."),
        new ErrorEntry("comments bounded by '*'", "Expecting \"and\" here."),
        new ErrorEntry("comments bounded by '*' or", "Expecting \"and\" here."),
        new ErrorEntry("comments bounded by '*' and", "Expecting \"lineEnd\" or a string here."),
        new ErrorEntry("comments bounded by '#' and or", "Expecting \"lineEnd\" or a string here."),
        new ErrorEntry("comments bounded by '#' and lineEnd or", "Expecting a string here."),
        new ErrorEntry("comments bounded by '#' and lineEnd or or", "Expecting a string here.")
    ];

    [TestMethod]
    public void TestStandardCommentsConfiguration()
    {
        Verify("standard comments", "");
        Verify("standard comments", "// Comment");
        Verify("standard comments report", "// Comment",
            expectedTokens: new CommentToken("// Comment\n"));

        Verify("standard comments", "/* Comment */");
        Verify("standard comments report", "/* Comment */",
            expectedTokens: new CommentToken("/* Comment */"));

        Verify("standard comments or '#' and lineEnd", "# Comment");
        Verify("standard comments or '#' and lineEnd report", "# Comment",
            expectedTokens: new CommentToken("# Comment\n"));
    }

    [TestMethod]
    public void TestCommentsConfiguration()
    {
        Verify("comments bounded by '/*' and '*/'", "");
        Verify("comments bounded by '/*' and '*/'", "/* Comment */");
        Verify("comments bounded by '/*' and '*/' report", "/* Comment */",
            expectedTokens: new CommentToken("/* Comment */"));

        Verify("comments bounded by '//' and lineEnd", "// Comment");
        Verify("comments bounded by '//' and lineEnd report", "// Comment",
            expectedTokens: new CommentToken("// Comment\n"));

        Verify("comments bounded by '//' and lineEnd or '#' and lineEnd", "# Comment");
        Verify("comments bounded by '//' and lineEnd or '#' and lineEnd report", "# Comment",
            expectedTokens: new CommentToken("# Comment\n"));
    }

    [TestMethod]
    public void TestCommentsConfigurationErrors()
    {
        RunErrorTests(ErrorTests);
    }
}
