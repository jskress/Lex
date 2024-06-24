namespace Lex.Tokens;

/// <summary>
/// This class represents a bounder token.  Bounders are, by definition, single character
/// tokens.
/// </summary>
public class BounderToken : Token
{
    /// <summary>
    /// This constant defines the left parenthesis bounder.
    /// </summary>
    public static readonly BounderToken LeftParen = new ('(');

    /// <summary>
    /// This constant defines the right parenthesis bounder.
    /// </summary>
    public static readonly BounderToken RightParen = new (')');

    /// <summary>
    /// This constant defines the open bracket bounder.
    /// </summary>
    public static readonly BounderToken OpenBracket = new ('[');

    /// <summary>
    /// This constant defines the close bracket bounder.
    /// </summary>
    public static readonly BounderToken CloseBracket = new (']');

    /// <summary>
    /// This constant defines the open brace bounder.
    /// </summary>
    public static readonly BounderToken OpenBrace = new ('{');

    /// <summary>
    /// This constant defines the close brace bounder.
    /// </summary>
    public static readonly BounderToken CloseBrace = new ('}');

    public BounderToken(char ch) : base(Convert.ToString(ch)) {}
}
