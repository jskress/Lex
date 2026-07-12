using Lex.Clauses;

namespace Tests.ClauseTests;

[TestClass]
public class ClauseDispatcherTests
{
    [TestMethod]
    public void TestDispatchRoutesByTag()
    {
        List<string> seen = [];
        ClauseDispatcher dispatcher = new ClauseDispatcher()
            .On("foo", clause => seen.Add($"foo:{clause.Tokens.Count}"))
            .On("bar", clause => seen.Add($"bar:{clause.Tokens.Count}"));

        dispatcher.Dispatch(new Clause { Tag = "bar", Tokens = [] });
        dispatcher.Dispatch(new Clause { Tag = "foo", Tokens = [] });

        CollectionAssert.AreEqual(new[] { "bar:0", "foo:0" }, seen);
    }

    [TestMethod]
    public void TestOnRejectsDuplicateTag()
    {
        ClauseDispatcher dispatcher = new ClauseDispatcher()
            .On("foo", _ => { });

        Exception exception = Assert.ThrowsExactly<ArgumentException>(
            () => dispatcher.On("foo", _ => { }));

        Assert.AreEqual(
            "A handler has already been registered for tag 'foo'. (Parameter 'tag')",
            exception.Message);
    }

    [TestMethod]
    public void TestDispatchThrowsWhenUnhandled()
    {
        ClauseDispatcher dispatcher = new ClauseDispatcher()
            .On("foo", _ => { });

        Exception exception = Assert.ThrowsExactly<ArgumentException>(
            () => dispatcher.Dispatch(new Clause { Tag = "bar", Tokens = [] }));

        Assert.AreEqual(
            "No handler has been registered for tag 'bar' and no fallback handler was registered. (Parameter 'clause')",
            exception.Message);
    }

    [TestMethod]
    public void TestDispatchThrowsForUntaggedClauseWhenUnhandled()
    {
        ClauseDispatcher dispatcher = new ClauseDispatcher()
            .On("foo", _ => { });

        Exception exception = Assert.ThrowsExactly<ArgumentException>(
            () => dispatcher.Dispatch(new Clause { Tokens = [] }));

        Assert.AreEqual(
            "The clause has no tag to dispatch on and no fallback handler was registered. (Parameter 'clause')",
            exception.Message);
    }

    [TestMethod]
    public void TestOnUnhandledCatchesUnmatchedTags()
    {
        List<string> fallbackTags = [];
        ClauseDispatcher dispatcher = new ClauseDispatcher()
            .On("foo", _ => { })
            .OnUnhandled(clause => fallbackTags.Add(clause.Tag ?? "<null>"));

        dispatcher.Dispatch(new Clause { Tag = "bar", Tokens = [] });
        dispatcher.Dispatch(new Clause { Tokens = [] });

        CollectionAssert.AreEqual(new[] { "bar", "<null>" }, fallbackTags);
    }

    [TestMethod]
    public void TestGenericDispatchRoutesByTagAndReturnsResult()
    {
        ClauseDispatcher<int> dispatcher = new ClauseDispatcher<int>()
            .On("foo", clause => clause.Tokens.Count)
            .On("bar", _ => -1);

        Assert.AreEqual(0, dispatcher.Dispatch(new Clause { Tag = "foo", Tokens = [] }));
        Assert.AreEqual(-1, dispatcher.Dispatch(new Clause { Tag = "bar", Tokens = [] }));
    }

    [TestMethod]
    public void TestGenericDispatchThrowsWhenUnhandled()
    {
        ClauseDispatcher<int> dispatcher = new ClauseDispatcher<int>()
            .On("foo", _ => 1);

        Assert.ThrowsExactly<ArgumentException>(
            () => dispatcher.Dispatch(new Clause { Tag = "bar", Tokens = [] }));
    }

    [TestMethod]
    public void TestGenericOnUnhandledCatchesUnmatchedTags()
    {
        ClauseDispatcher<int> dispatcher = new ClauseDispatcher<int>()
            .On("foo", _ => 1)
            .OnUnhandled(_ => -1);

        Assert.AreEqual(-1, dispatcher.Dispatch(new Clause { Tag = "bar", Tokens = [] }));
    }

    [TestMethod]
    public void TestNullArgumentChecks()
    {
        ClauseDispatcher dispatcher = new ();

        Assert.ThrowsExactly<ArgumentNullException>(() => dispatcher.On(null!, _ => { }));
        Assert.ThrowsExactly<ArgumentNullException>(() => dispatcher.On("foo", null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => dispatcher.OnUnhandled(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => dispatcher.Dispatch(null!));
    }
}
