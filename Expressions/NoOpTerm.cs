namespace Lex.Expressions;

/// <summary>
/// This is a placeholder class used by the no-op expression tree builder to represent a
/// term.  It is a singleton that is always returned by the tree builder methods.
/// </summary>
public class NoOpTerm : IExpressionTerm
{
    /// <summary>
    /// This field provides a reference to the one and only instance we ever need.
    /// </summary>
    public static readonly NoOpTerm Instance = new ();

    private NoOpTerm() {}
}
