namespace Lex.Parser;

/// <summary>
/// This class tracks the "furthest" point reached in a token stream across a series of
/// failed clause match attempts, along with a description of what was being looked for at
/// each of those points.  Attach one to a <see cref="LexicalParser"/> via its
/// <see cref="LexicalParser.FailureTracker"/> property to have clause parsers (such as
/// <see cref="Lex.Clauses.SingleTokenClauseParser"/>) report to it whenever they fail to
/// match without an explicit error message of their own.
/// </summary>
/// <remarks>
/// This exists so you can build a helpful top-level error message for your grammar without
/// having to hand-annotate every clause with an error message.  Since failures are recorded
/// as they happen rather than changing how clause parsers behave, attaching a tracker has no
/// effect on parsing itself; it's purely an opt-in diagnostic aid you consult after a
/// top-level parse attempt comes back empty.
/// </remarks>
public sealed class FailureTracker
{
    /// <summary>
    /// This property holds the line of the furthest point reached so far.  This is zero
    /// until the first failure is recorded.
    /// </summary>
    public int Line { get; private set; }

    /// <summary>
    /// This property holds the column of the furthest point reached so far.  This is zero
    /// until the first failure is recorded.
    /// </summary>
    public int Column { get; private set; }

    /// <summary>
    /// This property holds the descriptions of what was expected at the furthest point
    /// reached so far.  There will be more than one entry if multiple, different things
    /// were all being looked for at that same point (e.g., the alternatives of a switch
    /// clause).
    /// </summary>
    public IReadOnlyList<string> Expectations => _expectations;

    private readonly List<string> _expectations = [];

    /// <summary>
    /// This method is used to record that something was expected, but not found, at the
    /// given position.  If this position is further along than any previously recorded,
    /// it becomes the new furthest point and its expectation list starts over.  If it
    /// matches the current furthest point, the expectation is added to the list, provided
    /// it's not already present.  Positions before the current furthest point are ignored.
    /// </summary>
    /// <param name="line">The line of the position the failure occurred at.</param>
    /// <param name="column">The column of the position the failure occurred at.</param>
    /// <param name="expectation">A description of what was expected there.</param>
    public void Record(int line, int column, string expectation)
    {
        ArgumentNullException.ThrowIfNull(expectation, nameof(expectation));

        if (line > Line || (line == Line && column > Column))
        {
            Line = line;
            Column = column;

            _expectations.Clear();
            _expectations.Add(expectation);
        }
        else if (line == Line && column == Column && !_expectations.Contains(expectation))
            _expectations.Add(expectation);
    }

    /// <summary>
    /// This method is used to build a human-readable message from whatever has been
    /// recorded so far.
    /// </summary>
    /// <returns>A message describing the furthest failure(s) recorded, or <c>null</c>, if
    /// nothing has been recorded yet.</returns>
    public string? BuildMessage()
    {
        return _expectations.Count switch
        {
            0 => null,
            1 => $"Expecting {_expectations[0]} here.",
            _ => $"Expecting one of {string.Join(", ", _expectations)} here."
        };
    }

    /// <summary>
    /// This method is used to clear out anything that has been recorded so far, allowing
    /// this tracker to be reused for another top-level parse attempt.
    /// </summary>
    public void Reset()
    {
        Line = 0;
        Column = 0;

        _expectations.Clear();
    }
}
