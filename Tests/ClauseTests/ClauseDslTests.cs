using Lex;
using Lex.Clauses;
using Lex.Dsl;
using Lex.Parser;

namespace Tests.ClauseTests;

[TestClass]
public class ClauseDslTests : TestsBase
{
    [TestMethod]
    public void TestRecursiveClauseReference()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom(""""
            _parserSpec: """
                dsl keywords
                whitespace
                """
            _keywords: 'one', 'two', 'three', 'four'
            clause: [ one | two | three ] ?? 'Boom!' 
            """");
        LexicalParser parser = dsl.CreateLexicalParser();

        parser.SetSource("four".AsReader());
        
        AssertTokenException(() => dsl.ParseClause(parser, "clause"),
            "Boom!");
    }

    [TestMethod]
    public void TestTokenClauseTagging()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom(""""
            _parserSpec: """
                dsl keywords
                whitespace
                """
            _keywords: 'background'
            [ background => 'the background' ] ?? 'Boom!' 
            """");
        LexicalParser parser = dsl.CreateLexicalParser();

        parser.SetSource("background".AsReader());

        Clause clause = dsl.ParseNextClause(parser);

        Assert.AreEqual("the background", clause.Tag);
    }
}
