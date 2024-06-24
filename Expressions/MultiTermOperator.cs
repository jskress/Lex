using Lex.Clauses;

namespace Lex.Expressions;

/// <summary>
/// This class represents a multiple term operator.
/// </summary>
internal class MultiTermOperator
{
    /// <summary>
    /// This holds the clause parser that represent the first operator.
    /// </summary>
    internal ClauseParser Operator1 { get; init; }

    /// <summary>
    /// This holds the clause parser that represent the second operator.  This is ignored
    /// if this represents a binary operator.
    /// </summary>
    internal ClauseParser Operator2 { get; init; }

    /// <summary>
    /// Thils holds the precedence of the operator.  This applies to binary operators only.
    /// </summary>
    internal int Precedence { get; init; }
}
