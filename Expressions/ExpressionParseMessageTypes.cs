namespace Lex.Expressions;

/// <summary>
/// This enumeration notes the various parsing errors the expression support can throw.
/// </summary>
public enum ExpressionParseMessageTypes
{
    /// <summary>
    /// This is used internally and is never used to format an error to throw.
    /// </summary>
    None = 0,

    /// <summary>
    /// The expression parser will throw this when a term ie expected but not found.
    /// </summary>
    MissingRequiredTerm = 1,

    /// <summary>
    /// The expression parser will throw this when an explicitly expected token is not
    /// found.  The parser will provide a single string argument to this that contains a
    /// description of the expected token.
    /// </summary>
    MissingRequiredToken = 2,

    /// <summary>
    /// The expression parser will throw this when a token is not of an expected type.
    /// The parser will provide a single string argument to this that contains a
    /// description of the expected token type.
    /// </summary>
    TokenTypeMismatch = 3,

    /// <summary>
    /// The expression parser will throw this when an expected expression separator could
    /// not be found.
    /// </summary>
    MissingExpressionSeparator = 4,

    /// <summary>
    /// The expression parser will throw this when parsing an expression list that requires
    /// more expression terms than was found.
    /// </summary>
    NotEnoughExpressions = 5,

    /// <summary>
    /// The expression parser will throw this when parsing a parenthetical expression that
    /// is missing the closing right parenthesis.
    /// </summary>
    MissingRightParenthesis = 6,

    /// <summary>
    /// The expression parser will throw this when parsing a trinary operator but doesn't
    /// find the right-hand operator.  The parser will provide a single string argument to
    /// this that contains a description of the left operator that the expected right one
    /// pairs with.
    /// </summary>
    MissingRightOperator = 7
}
