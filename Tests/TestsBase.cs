using Lex.Parser;

namespace Tests;

public class TestsBase
{
    public static void AssertTokenException(Action action, string expectedMessage, string label = "")
    {
        TokenException exception = Assert.ThrowsException<TokenException>(action);

        Assert.AreEqual(expectedMessage, exception.Message, label);
    }

    public static void AssertArgumentException(Action action, string expectedMessage, string label = "")
    {
        ArgumentException exception = Assert.ThrowsException<ArgumentException>(action);

        Assert.AreEqual(expectedMessage, exception.Message, label);
    }

    public static void AssertArgumentNullException(Action action, string expectedMessage, string label = "")
    {
        ArgumentNullException exception = Assert.ThrowsException<ArgumentNullException>(action);

        Assert.AreEqual(expectedMessage, exception.Message, label);
    }

    public static void AssertException(Action action, string expectedMessage, string label = "")
    {
        Exception exception = Assert.ThrowsException<Exception>(action);

        Assert.AreEqual(expectedMessage, exception.Message, label);
    }
}
