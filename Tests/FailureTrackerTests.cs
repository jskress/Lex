using Lex.Parser;

namespace Tests;

[TestClass]
public class FailureTrackerTests
{
    [TestMethod]
    public void TestBuildMessageWithNothingRecorded()
    {
        FailureTracker tracker = new ();

        Assert.IsNull(tracker.BuildMessage());
        Assert.AreEqual(0, tracker.Line);
        Assert.AreEqual(0, tracker.Column);
        Assert.AreEqual(0, tracker.Expectations.Count);
    }

    [TestMethod]
    public void TestSingleExpectation()
    {
        FailureTracker tracker = new ();

        tracker.Record(3, 5, "a number");

        Assert.AreEqual(3, tracker.Line);
        Assert.AreEqual(5, tracker.Column);
        CollectionAssert.AreEqual(new[] { "a number" }, tracker.Expectations.ToList());
        Assert.AreEqual("Expecting a number here.", tracker.BuildMessage());
    }

    [TestMethod]
    public void TestFurtherPositionReplacesEarlierOne()
    {
        FailureTracker tracker = new ();

        tracker.Record(1, 1, "a number");
        tracker.Record(2, 1, "an identifier");

        Assert.AreEqual(2, tracker.Line);
        Assert.AreEqual(1, tracker.Column);
        CollectionAssert.AreEqual(new[] { "an identifier" }, tracker.Expectations.ToList());
    }

    [TestMethod]
    public void TestSameLineFurtherColumnReplacesEarlierOne()
    {
        FailureTracker tracker = new ();

        tracker.Record(1, 3, "a number");
        tracker.Record(1, 7, "an identifier");

        Assert.AreEqual(1, tracker.Line);
        Assert.AreEqual(7, tracker.Column);
        CollectionAssert.AreEqual(new[] { "an identifier" }, tracker.Expectations.ToList());
    }

    [TestMethod]
    public void TestEarlierPositionIsIgnored()
    {
        FailureTracker tracker = new ();

        tracker.Record(5, 5, "an identifier");
        tracker.Record(1, 1, "a number");

        Assert.AreEqual(5, tracker.Line);
        Assert.AreEqual(5, tracker.Column);
        CollectionAssert.AreEqual(new[] { "an identifier" }, tracker.Expectations.ToList());
    }

    [TestMethod]
    public void TestSamePositionAccumulatesDistinctExpectations()
    {
        FailureTracker tracker = new ();

        tracker.Record(2, 4, "a number");
        tracker.Record(2, 4, "an identifier");
        tracker.Record(2, 4, "a number"); // Duplicate; should not be added again.

        CollectionAssert.AreEqual(new[] { "a number", "an identifier" }, tracker.Expectations.ToList());
        Assert.AreEqual("Expecting one of a number, an identifier here.", tracker.BuildMessage());
    }

    [TestMethod]
    public void TestReset()
    {
        FailureTracker tracker = new ();

        tracker.Record(2, 4, "a number");
        tracker.Reset();

        Assert.AreEqual(0, tracker.Line);
        Assert.AreEqual(0, tracker.Column);
        Assert.AreEqual(0, tracker.Expectations.Count);
        Assert.IsNull(tracker.BuildMessage());
    }

    [TestMethod]
    public void TestRecordRejectsNullExpectation()
    {
        FailureTracker tracker = new ();

        Assert.ThrowsExactly<ArgumentNullException>(() => tracker.Record(1, 1, null!));
    }
}
