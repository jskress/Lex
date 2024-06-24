using Lex;
using Lex.Dsl;
using Lex.Parser;
using Lex.Tokens;

namespace Tests.TokenizerTests;

public class TokenizerTestsBase : TestsBase
{
    protected static readonly Token IdAToken = new IdToken("a");
    protected static readonly Token IdBToken = new IdToken("b");
    protected static readonly Token IdAbToken = new IdToken("ab");

    protected static void Verify(LexicalParser parser, string source, params Token[] expectedTokens)
    {
        List<Token> expected = [..expectedTokens];

        parser.SetSource(source.AsReader());

        while (expected.Count > 0)
        {
            Token expectedToken = expected.RemoveFirst();
            Token actualToken = parser.GetNextToken();

            expectedToken.AssertTokenMatches(actualToken);
        }

        Assert.IsNull(parser.GetNextToken());
    }
}
