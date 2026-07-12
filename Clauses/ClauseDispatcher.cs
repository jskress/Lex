namespace Lex.Clauses;

/// <summary>
/// This class provides a simple way to route a parsed <see cref="Clause"/> to one of several
/// handlers based on its <see cref="Clause.Tag"/>.  This is exactly the pattern Lex uses
/// internally to interpret its own DSL specifications; it is exposed here so consumers don't
/// need to reimplement it for their own DSLs.
/// </summary>
public class ClauseDispatcher
{
    private readonly Dictionary<string, Action<Clause>> _handlers = new ();

    private Action<Clause>? _fallback;

    /// <summary>
    /// This method is used to register the handler to invoke when a clause tagged with the
    /// given tag is dispatched.
    /// </summary>
    /// <param name="tag">The tag to register a handler for.</param>
    /// <param name="handler">The handler to invoke for clauses with the given tag.</param>
    /// <returns>This object, for fluency.</returns>
    public ClauseDispatcher On(string tag, Action<Clause> handler)
    {
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        if (!_handlers.TryAdd(tag, handler))
            throw new ArgumentException($"A handler has already been registered for tag '{tag}'.", nameof(tag));

        return this;
    }

    /// <summary>
    /// This method is used to register a fallback handler to invoke when a dispatched
    /// clause's tag (or the lack of one) has no registered handler.  If this is never
    /// called, <see cref="Dispatch"/> will throw when it can't find a handler to use.
    /// </summary>
    /// <param name="handler">The fallback handler to use.</param>
    /// <returns>This object, for fluency.</returns>
    public ClauseDispatcher OnUnhandled(Action<Clause> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _fallback = handler;

        return this;
    }

    /// <summary>
    /// This method is used to route the given clause to whichever handler was registered
    /// for its tag.
    /// </summary>
    /// <param name="clause">The clause to dispatch.</param>
    public void Dispatch(Clause clause)
    {
        ArgumentNullException.ThrowIfNull(clause, nameof(clause));

        if (clause.Tag != null && _handlers.TryGetValue(clause.Tag, out Action<Clause>? handler))
        {
            handler(clause);

            return;
        }

        if (_fallback != null)
        {
            _fallback(clause);

            return;
        }

        throw new ArgumentException(
            clause.Tag == null
                ? "The clause has no tag to dispatch on and no fallback handler was registered."
                : $"No handler has been registered for tag '{clause.Tag}' and no fallback handler was registered.",
            nameof(clause));
    }
}

/// <summary>
/// This class provides the same tag-based routing as <see cref="ClauseDispatcher"/>, but for
/// handlers that need to produce a result (e.g., a node in an AST you're building up as you
/// interpret clauses) rather than just act on the clause.
/// </summary>
/// <typeparam name="TResult">The type of result each handler produces.</typeparam>
public class ClauseDispatcher<TResult>
{
    private readonly Dictionary<string, Func<Clause, TResult>> _handlers = new ();

    private Func<Clause, TResult>? _fallback;

    /// <summary>
    /// This method is used to register the handler to invoke when a clause tagged with the
    /// given tag is dispatched.
    /// </summary>
    /// <param name="tag">The tag to register a handler for.</param>
    /// <param name="handler">The handler to invoke for clauses with the given tag.</param>
    /// <returns>This object, for fluency.</returns>
    public ClauseDispatcher<TResult> On(string tag, Func<Clause, TResult> handler)
    {
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        if (!_handlers.TryAdd(tag, handler))
            throw new ArgumentException($"A handler has already been registered for tag '{tag}'.", nameof(tag));

        return this;
    }

    /// <summary>
    /// This method is used to register a fallback handler to invoke when a dispatched
    /// clause's tag (or the lack of one) has no registered handler.  If this is never
    /// called, <see cref="Dispatch"/> will throw when it can't find a handler to use.
    /// </summary>
    /// <param name="handler">The fallback handler to use.</param>
    /// <returns>This object, for fluency.</returns>
    public ClauseDispatcher<TResult> OnUnhandled(Func<Clause, TResult> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _fallback = handler;

        return this;
    }

    /// <summary>
    /// This method is used to route the given clause to whichever handler was registered
    /// for its tag and return what that handler produces.
    /// </summary>
    /// <param name="clause">The clause to dispatch.</param>
    /// <returns>Whatever the resolved handler produces.</returns>
    public TResult Dispatch(Clause clause)
    {
        ArgumentNullException.ThrowIfNull(clause, nameof(clause));

        if (clause.Tag != null && _handlers.TryGetValue(clause.Tag, out Func<Clause, TResult>? handler))
            return handler(clause);

        if (_fallback != null)
            return _fallback(clause);

        throw new ArgumentException(
            clause.Tag == null
                ? "The clause has no tag to dispatch on and no fallback handler was registered."
                : $"No handler has been registered for tag '{clause.Tag}' and no fallback handler was registered.",
            nameof(clause));
    }
}
