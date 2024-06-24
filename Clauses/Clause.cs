using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class represents the result of an attempt to parse tokens to satisfy clause
/// requirements.
/// </summary>
public class Clause
{
    /// <summary>
    /// This property provides the string tag that the DSL definition noted for the clause
    /// that was parsed.
    /// </summary>
    public string Tag { get; init; }

    /// <summary>
    /// This property provides the list of tokens that matched the clause that was parsed.
    /// </summary>
    public List<Token> Tokens { get; init; }
}
