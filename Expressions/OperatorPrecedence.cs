namespace Lex.Expressions;

/// <summary>
/// This enumeration defines standard precedence levels for binary operators.
/// The higher the value, the higher the evaluation precedence.  The levels
/// defined here loosely follow those of C# as they are relatively standard
/// across various languages.
/// </summary>
public enum OperatorPrecedence
{
    Coalesce = 100,
    Boolean = 200,
    Equality = 300,
    Comparison = 400,
    Shift = 500,
    Additive = 600,
    Multiplicative = 700
}

/// <summary>
/// This class defines some reference information about operators as they relate to
/// expressions.
/// </summary>
public static class OperatorInfo
{
    /// <summary>
    /// This field holds our predefined binary operator precedences as a dictionary, keyed
    /// by the text of the operator.
    /// </summary>
    public static readonly Dictionary<string, OperatorPrecedence> Precedences = new ()
    {
        { "*", OperatorPrecedence.Multiplicative },
        { "/", OperatorPrecedence.Multiplicative },
        { "%", OperatorPrecedence.Multiplicative },
        { "^", OperatorPrecedence.Multiplicative },
        { "+", OperatorPrecedence.Additive },
        { "-", OperatorPrecedence.Additive },
        { "<<", OperatorPrecedence.Shift },
        { ">>", OperatorPrecedence.Shift },
        { ">>>", OperatorPrecedence.Shift },
        { "<", OperatorPrecedence.Comparison },
        { "<=", OperatorPrecedence.Comparison },
        { ">", OperatorPrecedence.Comparison },
        { ">=", OperatorPrecedence.Comparison },
        { "==", OperatorPrecedence.Equality },
        { "!=", OperatorPrecedence.Equality },
        { "&", OperatorPrecedence.Boolean },
        { "&&", OperatorPrecedence.Boolean },
        { "||", OperatorPrecedence.Boolean },
        { "|", OperatorPrecedence.Boolean },
        { "??", OperatorPrecedence.Coalesce }
    };
}
