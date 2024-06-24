using Lex;
using Lex.Parser;

namespace Tests;

[TestClass]
public class RewindReaderTests
{
    private record ReadTest(int Ch, int Line, int Column);

    private static readonly List<ReadTest> BasicReadTests = new ()
    {
        new ReadTest('T', 1, 1),
        new ReadTest('e', 1, 2),
        new ReadTest('s', 1, 3),
        new ReadTest('t', 1, 4),
        new ReadTest(-1, 1, 4)
    };
    private static readonly List<ReadTest> LineEndReadTests = new ()
    {
        new ReadTest('1', 1, 1),
        new ReadTest('\n', 2, 0),
        new ReadTest('2', 2, 1),
        new ReadTest('\n', 3, 0),
        new ReadTest('3', 3, 1),
        new ReadTest('\n', 4, 0),
        new ReadTest('4', 4, 1),
        new ReadTest(-1, 4, 1)
    };

    [TestMethod]
    public void TestReading()
    {
        using RewindingStreamReader reader = new ("Test".AsReader());

        Assert.AreEqual(0, reader.Line);
        Assert.AreEqual(0, reader.Column);

        RunReadTests(BasicReadTests, reader);
    }

    [TestMethod]
    public void TestLineEnds()
    {
        using RewindingStreamReader reader = new ("1\r2\n3\r\n4".AsReader());

        RunReadTests(LineEndReadTests, reader);
    }

    [TestMethod]
    public void TestReturningCharacters()
    {
        using RewindingStreamReader reader = new ("Test".AsReader());

        Assert.AreEqual('T', reader.Read());
        Assert.AreEqual('e', reader.Read());
        Assert.AreEqual('s', reader.Read());

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(3, reader.Column);
        // This leaves 't' as the only character in the stream.  Let's return the three
        // characters we just read and make sure everything restores properly.

        reader.ReturnChar('s');

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(2, reader.Column);

        reader.ReturnChar('e');

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(1, reader.Column);

        reader.ReturnChar('T');

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(0, reader.Column);

        // And now make sure re-reading produces the right results.
        RunReadTests(BasicReadTests, reader);
    }

    [TestMethod]
    public void TestReturningCharactersWithLineEnds()
    {
        using RewindingStreamReader reader = new ("1\r2\n3\r\n4".AsReader());

        Assert.AreEqual('1', reader.Read());
        Assert.AreEqual('\n', reader.Read());
        Assert.AreEqual('2', reader.Read());
        Assert.AreEqual('\n', reader.Read());
        Assert.AreEqual('3', reader.Read());

        Assert.AreEqual(3, reader.Line);
        Assert.AreEqual(1, reader.Column);
        // This leaves the line end for line 3 and the forth line as the only characters in
        // the stream.  Let's return all the characters we just read and make sure everything
        // restores properly.

        reader.ReturnChar('3');

        Assert.AreEqual(3, reader.Line);
        Assert.AreEqual(0, reader.Column);

        reader.ReturnChar('\n');

        Assert.AreEqual(2, reader.Line);
        Assert.AreEqual(1, reader.Column);

        reader.ReturnChar('2');

        Assert.AreEqual(2, reader.Line);
        Assert.AreEqual(0, reader.Column);

        reader.ReturnChar('\n');

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(1, reader.Column);

        reader.ReturnChar('1');

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(0, reader.Column);

        // And now make sure re-reading produces the right results.
        RunReadTests(LineEndReadTests, reader);
    }

    [TestMethod]
    public void TestReturningCrDoesNothing()
    {
        using RewindingStreamReader reader = new ("Test".AsReader());

        Assert.AreEqual('T', reader.Read());
        Assert.AreEqual('e', reader.Read());
        Assert.AreEqual('s', reader.Read());

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(3, reader.Column);

        reader.ReturnChar('\r');

        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(3, reader.Column);
        Assert.AreEqual('t', reader.Read());
        Assert.AreEqual(1, reader.Line);
        Assert.AreEqual(4, reader.Column);
    }

    private static void RunReadTests(List<ReadTest> tests, RewindingStreamReader reader)
    {
        int index = 1;

        foreach (ReadTest test in tests)
        {
            string label = $"Test #{index}";
            int ch = reader.Read();

            Assert.AreEqual(test.Ch, ch, label);
            Assert.AreEqual(test.Line, reader.Line, label);
            Assert.AreEqual(test.Column, reader.Column, label);
        }
    }
}
