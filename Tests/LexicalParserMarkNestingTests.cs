using System.Text;
using Lex;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests;

/// <summary>
/// These tests cover marks nesting: any number of marks may be pending at once, and
/// resolving one must involve only the tokens consumed since that particular mark.
/// </summary>
[TestClass]
public class LexicalParserMarkNestingTests : TestsBase
{
    private const int TokenCount = 40;

    private static readonly string[] Words = Enumerable
        .Range(0, TokenCount)
        .Select(index => $"t{index}")
        .ToArray();

    private static LexicalParser CreateParser()
    {
        LexicalParser parser = new ();

        _ = new IdTokenizer(parser);
        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        parser.SetSource(string.Join(' ', Words).AsReader());

        return parser;
    }

    /// <summary>
    /// Four marks deep, resolved with a mix of rollbacks and releases, checking after every
    /// step that the parser sits exactly where that step should have left it.
    /// </summary>
    [TestMethod]
    public void TestDeeplyNestedMarksEachKeepTheirOwnScope()
    {
        using LexicalParser parser = CreateParser();

        // Depth 1: consume t0.
        parser.MarkPosition();

        Assert.AreEqual("t0", parser.GetNextToken()?.Text);

        // Depth 2: consume t1, t2.
        parser.MarkPosition();

        Assert.AreEqual("t1", parser.GetNextToken()?.Text);
        Assert.AreEqual("t2", parser.GetNextToken()?.Text);

        // Depth 3: consume t3.
        parser.MarkPosition();

        Assert.AreEqual("t3", parser.GetNextToken()?.Text);

        // Depth 4: consume t4, t5, then throw that scope away.
        parser.MarkPosition();

        Assert.AreEqual("t4", parser.GetNextToken()?.Text);
        Assert.AreEqual("t5", parser.GetNextToken()?.Text);

        parser.RollbackToMark();

        Assert.AreEqual("t4", parser.PeekNextToken()?.Text, "depth 4 should have given back only t4 and t5");

        // Keep depth 3's work (t3), and the t4 we are about to take.
        Assert.AreEqual("t4", parser.GetNextToken()?.Text);

        parser.ReleaseMark();

        Assert.AreEqual("t5", parser.PeekNextToken()?.Text, "a release must not move the parser");

        // Depth 2 rolls back: everything since its mark goes, which is t1 through t4 --
        // including the t4 that depth 3 released to it, but not depth 1's t0.
        parser.RollbackToMark();

        Assert.AreEqual("t1", parser.PeekNextToken()?.Text, "depth 2 should have given back t1 through t4");

        // Depth 1 rolls back: t0 as well, and nothing beyond.
        parser.RollbackToMark();

        Assert.AreEqual("t0", parser.PeekNextToken()?.Text, "depth 1 should have given back t0");

        Assert.ThrowsExactly<InvalidOperationException>(() => parser.RollbackToMark());

        StringBuilder builder = new ();

        while (parser.GetNextToken() is { } token)
            builder.Append(token.Text).Append(' ');

        Assert.AreEqual(
            string.Join(' ', Words) + " ", builder.ToString(),
            "the whole stream should be intact after unwinding everything");
    }

    /// <summary>
    /// A released inner scope's tokens must stay on the record, so that an outer mark can
    /// still take them back.  Releasing is not the same as forgetting.
    /// </summary>
    [TestMethod]
    public void TestReleasingAnInnerMarkStillLetsAnOuterOneRollBackPastIt()
    {
        using LexicalParser parser = CreateParser();

        parser.MarkPosition();

        Assert.AreEqual("t0", parser.GetNextToken()?.Text);

        for (int depth = 0; depth < 5; depth++)
        {
            parser.MarkPosition();

            _ = parser.GetNextToken();

            parser.ReleaseMark();
        }

        Assert.AreEqual("t6", parser.PeekNextToken()?.Text);

        parser.RollbackToMark();

        // Draining matters here: checking only the next token would still pass if the
        // released scopes' tokens had been dropped rather than reclaimed.
        List<string> rest = [];

        while (parser.GetNextToken() is { } token)
            rest.Add(token.Text);

        CollectionAssert.AreEqual(
            Words, rest.ToArray(),
            "the outer mark must reclaim everything the released inner marks consumed");
    }

    /// <summary>
    /// The real proof: random sequences of marking, consuming, returning, rolling back and
    /// releasing, checked at every step against a plain model of where the stream should be.
    /// The model is a position and a stack of positions, which is what the machinery is
    /// supposed to amount to.
    /// </summary>
    [TestMethod]
    public void TestNestedMarksMatchAPlainModelUnderRandomUse()
    {
        int deepestSeen = 0;

        for (int seed = 0; seed < 200; seed++)
        {
            Random random = new (seed);

            using LexicalParser parser = CreateParser();

            int position = 0;
            Stack<int> modelMarks = new ();
            Token? lastConsumed = null;

            for (int step = 0; step < 60; step++)
            {
                switch (random.Next(5))
                {
                    case 0: // Mark.
                        parser.MarkPosition();
                        modelMarks.Push(position);
                        deepestSeen = Math.Max(deepestSeen, modelMarks.Count);
                        lastConsumed = null;
                        break;

                    case 1: // Consume.
                    case 2:
                        Token? token = parser.GetNextToken();

                        Assert.AreEqual(
                            position < TokenCount ? Words[position] : null, token?.Text,
                            $"seed {seed}, step {step}: wrong token consumed");

                        if (position < TokenCount)
                            position++;

                        lastConsumed = token;
                        break;

                    case 3: // Hand the last token back, which must not be restored twice later.
                        if (lastConsumed != null)
                        {
                            parser.ReturnToken(lastConsumed);
                            position--;
                            lastConsumed = null;
                        }
                        break;

                    case 4: // Resolve a mark, rolling back or releasing.
                        if (modelMarks.Count > 0)
                        {
                            if (random.Next(2) == 0)
                            {
                                parser.RollbackToMark();
                                position = modelMarks.Pop();
                            }
                            else
                            {
                                parser.ReleaseMark();
                                modelMarks.Pop();
                            }

                            lastConsumed = null;
                        }
                        break;
                }

                Assert.AreEqual(
                    position < TokenCount ? Words[position] : null,
                    parser.PeekNextToken()?.Text,
                    $"seed {seed}, step {step}: parser is not where the model says it should be");
            }

            // Unwind whatever is still pending; the stream must come back whole.
            while (modelMarks.Count > 0)
            {
                parser.RollbackToMark();
                position = modelMarks.Pop();
            }

            List<string> rest = [];

            while (parser.GetNextToken() is { } token)
                rest.Add(token.Text);

            CollectionAssert.AreEqual(
                Words[position..], rest.ToArray(),
                $"seed {seed}: the stream did not come back whole");
        }

        // Guard the test itself: if the random walk stopped producing genuinely nested marks,
        // everything above would still pass while proving nothing about nesting.
        Assert.IsTrue(
            deepestSeen >= 5,
            $"the random walk only nested {deepestSeen} deep, which does not exercise nesting");
    }
}
