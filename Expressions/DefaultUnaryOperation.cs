namespace Lex.Expressions;

/// <summary>
/// This class represents a unary operation in an expression.
/// </summary>
public class DefaultUnaryOperation : DefaultExpressionTerm
{
    /// <summary>
    /// This property holds the argument term for the unary operation.
    /// </summary>
    public DefaultExpressionTerm Term { get; internal init; }

    /// <summary>
    /// This property holds the flag that notes whether the operation is prefix or postfix.
    /// </summary>
    public bool IsPrefix { get; internal init; }

    /// <summary>
    /// This method produces a text representation of this term.
    /// </summary>
    /// <returns>A text representation of this term.</returns>
    protected override string Format()
    {
        string tokens = string.Join(' ', Tokens.Select(token => token.Text));

        return IsPrefix ? tokens + Term.Text : Term.Text + tokens;
    }
}
