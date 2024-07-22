using Lex.Parser;

namespace Lex.Clauses;

/// <summary>
/// This is the base class for objects that can match one or more tokens (a clause).
/// </summary>
public abstract class ClauseParser
{
    /// <summary>
    /// This is the default debugging information consumer that will write debugging
    /// information to the console.
    /// </summary>
    private static readonly Action<ClauseParserDebugInfo> DefaultDebuggingConsumer =
        info => Console.WriteLine(info.AsMessage());

    /// <summary>
    /// This property notes whether this clause parser is currently being debugged.  If you
    /// modify this property directly, any child clause parsers of this one will not be
    /// affected.  To alter the debugging state for the entire graph, use the
    /// <see cref="SetDebugging"/> method.
    /// </summary>
    public bool IsDebugging { get; set; }

    /// <summary>
    /// This property holds the consumer of debug information.  By default, this is a
    /// lambda that converts the information to a string and writes that to the console
    /// (stdout).  If you modify this property directly, any child clause parsers of this
    /// one will not be affected.  To alter the consumer for the entire graph, use the
    /// <see cref="SetDebugConsumer"/> method.
    /// </summary>
    public Action<ClauseParserDebugInfo> DebugConsumer { get; set; } = DefaultDebuggingConsumer;

    private string _name;

    protected ClauseParser()
    {
        _name = GetType().Name;
    }

    /// <summary>
    /// This method is used to set the debugging name for the clause parser.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>This object, for fluency.</returns>
    public ClauseParser Named(string name)
    {
        _name = name;

        return this;
    }

    /// <summary>
    /// This method is used to set the debugging state for the clause parser.  If this parser
    /// contains child parsers, their debugging state will be updated as well.
    /// </summary>
    /// <param name="newState">The new debugging state.</param>
    /// <returns>This object, for fluency.</returns>
    public ClauseParser SetDebugging(bool newState)
    {
        if (IsDebugging != newState)
        {
            IsDebugging = newState;

            if (this is IClauseParserParent parent)
                parent.Children.ForEach(child => child.SetDebugging(newState));
        }

        return this;
    }

    /// <summary>
    /// This method is used to set the debugging information consumer for the clause parser.
    /// If this parser contains child parsers, their consumers will be updated as well.
    /// </summary>
    /// <param name="consumer">The new consumer for debugging information.</param>
    /// <returns>This object, for fluency.</returns>
    public ClauseParser SetDebugConsumer(Action<ClauseParserDebugInfo> consumer)
    {
        consumer ??= DefaultDebuggingConsumer;

        DebugConsumer = consumer;

        if (this is IClauseParserParent parent)
            parent.Children.ForEach(child => child.SetDebugConsumer(consumer));

        return this;
    }

    /// <summary>
    /// Subclasses must provide this method to try to parse themselves.
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    public Clause TryParse(LexicalParser parser)
    {
        Clause clause = TryParseClause(parser);

        if (IsDebugging)
        {
            ClauseParserDebugInfo info = new ClauseParserDebugInfo(parser, clause, _name);

            DebugConsumer?.Invoke(info);
        }

        return clause;
    }

    /// <summary>
    /// Subclasses must provide this method to try to parse themselves.
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    protected abstract Clause TryParseClause(LexicalParser parser);
}
