using Lex;
using Lex.Dsl;
using Lex.Parser;
using Lex.Tokens;

namespace Tests.LexicalParserDslTests;

public class LexicalParserDslTestsBase : TestsBase
{
    protected record ErrorEntry(string Source, string ErrorMessage) {}

    protected static void Verify(string parserSpec, string source, Dsl dsl = null, params Token[] expectedTokens)
    {
        List<Token> expected = [..expectedTokens];

        using LexicalParser p = LexicalParserFactory.CreateFrom(parserSpec, dsl);

        p.SetSource(source.AsReader());

        while (expected.Count > 0)
        {
            Token expectedToken = expected.RemoveFirst();
            Token actualToken = p.GetNextToken();

            expectedToken.AssertTokenMatches(actualToken);
        }

        Assert.IsNull(p.GetNextToken());
    }

    protected static void RunErrorTests(List<ErrorEntry> errorTests)
    {
        foreach (ErrorEntry test in errorTests)
        {
            AssertTokenException(
                () => Verify(test.Source, ""), test.ErrorMessage);
        }
    }
}
