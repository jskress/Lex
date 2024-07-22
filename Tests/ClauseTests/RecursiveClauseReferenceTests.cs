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
            childClause: { parentClause }
            parentClause: { childClause }
            """);
        FieldInfo info = dsl.GetType().GetField(
            "_clauses", BindingFlags.NonPublic | BindingFlags.Instance);
        Dictionary<string, ClauseParser> clauses = (Dictionary<string, ClauseParser>) info!.GetValue(dsl);

        Assert.IsNotNull(clauses);
        Assert.AreEqual(2, clauses.Count);
        Assert.IsTrue(clauses.ContainsKey("parentClause"));
        Assert.IsTrue(clauses.ContainsKey("childClause"));

        SequentialClauseParser parentClause = clauses["parentClause"] as SequentialClauseParser;
        SequentialClauseParser childClause = clauses["childClause"] as SequentialClauseParser;

        Assert.IsNotNull(parentClause);
        Assert.IsNotNull(childClause);
        Assert.AreEqual(1, parentClause.Children.Count);
        Assert.AreEqual(1, childClause.Children.Count);
        Assert.AreSame(parentClause, childClause.Children[0]);
        Assert.AreSame(childClause, parentClause.Children[0]);
    }
}
