using Lex.Parser;
using Lex.Tokens;

namespace Lex.Clauses;

/// <summary>
/// This class encapsulates the result of trying to use a clause parser to match and
/// capture tokens.  It will only be emitted when debugging is enabled.
/// </summary>
public class ClauseParserDebugInfo
{
    /// <summary>
    /// This property holds the name of the clause parser where the information originated.
    /// </summary>
    public string ClauseParserName { get; }

    /// <summary>
    /// This property holds the clause that was produced.  If no clause could be captured,
    /// this will be <c>null</c>.
    /// </summary>
    public Clause Clause { get; }

    /// <summary>
    /// This property holds the first token that could not be captured.  It will be present
    /// only if <see cref="Clause"/> is not <c>null.</c>.
    /// </summary>
    public Token Token { get; }

    internal ClauseParserDebugInfo(LexicalParser parser, Clause clause, string name)
    {
        ClauseParserName = name;
        Clause = clause;
        Token = clause == null ? null : parser.PeekNextToken();
    }

    /// <summary>
    /// This is a helper method for formatting this debug information as a simple, defult
    /// message. 
    /// </summary>
    /// <returns>This debug information as message text.</returns>
    public string AsMessage()
    {
        string content = Clause == null
            ? $"rejected token {Token}"
            : "captured [" + string.Join(", ", Clause.Tokens) + "]";
        string tag = Clause?.Tag;

        tag = tag == null ? string.Empty : $", tagged with '{tag}'";

        return $"Parser {ClauseParserName}{tag} {content}";
    }
}
