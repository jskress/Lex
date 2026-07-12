using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class represents a binary operation in an expression.
/// </summary>
public class DefaultBinaryOperation : DefaultExpressionTerm
{
    /// <summary>
    /// This property holds the left argument term for the binary operation.
    /// </summary>
    public DefaultExpressionTerm LeftTerm { get; }

    /// <summary>
    /// This property holds the right argument term for the binary operation.
    /// </summary>
    public DefaultExpressionTerm RightTerm { get; }

    internal DefaultBinaryOperation(
        IReadOnlyList<Token> tokens, DefaultExpressionTerm leftTerm, DefaultExpressionTerm rightTerm)
        : base(tokens)
    {
        LeftTerm = leftTerm;
        RightTerm = rightTerm;
    }

    /// <summary>
    /// This method produces a text representation of this term.
    /// </summary>
    /// <returns>A text representation of this term.</returns>
    protected override string Format()
    {
        string tokens = string.Join(' ', Tokens.Select(token => token.Text));

        return $"{LeftTerm.Text} {tokens} {RightTerm.Text}";
    }
}
