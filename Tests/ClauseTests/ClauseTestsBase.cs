using Lex;
using Lex.Clauses;
using Lex.Parser;
using Lex.Tokens;

namespace Tests.ClauseTests;

public class ClauseTestsBase : TestsBase
{
    protected static readonly Token This = new IdToken("this");
    protected static readonly Token That = new IdToken("that");
    protected static readonly Token Other = new IdToken("other");
    protected static readonly Token Thing = new IdToken("thing");

    protected static List<string> Verify(
        LexicalParser parser, string source, ClauseParser clauseParser, Token nextFromParser,
        Clause expected)
    {
        List<string> debugMessages = [];
        Action<ClauseParserDebugInfo> hold = clauseParser.DebugConsumer;

        clauseParser.DebugConsumer = info => debugMessages.Add(info.AsMessage());

        parser.SetSource(source.AsReader());

        Clause clause = clauseParser.TryParse(parser);

        if (expected == null && clause != null)
            Assert.Fail("Clause should not have matched but it did.");

        if (expected != null && clause == null)
            Assert.Fail("Clause should have matched but it didn't.");

        if (expected != null)
        {
            Assert.AreEqual(expected.Tag, clause.Tag, "Clause tags do not match.");

            int expectedCount = expected.Tokens.Count;
            int actualCount = clause.Tokens.Count;

            Assert.AreEqual(expectedCount, actualCount, $"Expected {expectedCount} tokens in clause but found {actualCount}.");

            for (int index = 0; index < expectedCount; index++)
            {
                Token expectedToken = expected.Tokens[index];
                Token actualToken = clause.Tokens[index];

                expectedToken.AssertTokenMatches(actualToken);
            }
        }

        if (nextFromParser == null)
            Assert.IsNull(parser.GetNextToken());
        else
            Assert.IsTrue(nextFromParser.Matches(parser.GetNextToken()));

        clauseParser.DebugConsumer = hold;

        return debugMessages;
    }
}
