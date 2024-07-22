using System.Reflection;
using Lex.Clauses;
using Lex.Dsl;

namespace Tests.ClauseTests;

[TestClass]
public class RecursiveClauseReferenceTests
{
    [TestMethod]
    public void TestRecursiveClauseReference()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom("""
            _keywords: 'test'
            testClause: {}
            testClause: { test }
            """);
        FieldInfo info = dsl.GetType().GetField(
            "_clauses", BindingFlags.NonPublic | BindingFlags.Instance);
        Dictionary<string, ClauseParser> clauses = (Dictionary<string, ClauseParser>) info!.GetValue(dsl);

        Assert.IsNotNull(clauses);
        Assert.AreEqual(1, clauses.Count);
        Assert.IsTrue(clauses.ContainsKey("testClause"));

        SequentialClauseParser sequentialClauseParser = clauses["testClause"] as SequentialClauseParser;

        Assert.IsNotNull(sequentialClauseParser);
        Assert.AreEqual(1, sequentialClauseParser.Children.Count);
    }
}
