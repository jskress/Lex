namespace Lex.Clauses;

/// <summary>
/// This interface is used to indicate clause parsers that carry child parsers.
/// </summary>
internal interface IClauseParserParent
{
    /// <summary>
    /// This property provides access to the parent's children.
    /// </summary>
    List<ClauseParser> Children { get; }
}
