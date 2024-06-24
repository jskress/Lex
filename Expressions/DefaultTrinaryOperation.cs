using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class represents a trinary operation in an expression.
/// </summary>
public class DefaultTrinaryOperation : DefaultExpressionTerm
{
    /// <summary>
    /// This property holds the list of tokens that make up the left hand operator.
    /// </summary>
    public IReadOnlyList<Token> LeftTokens => Tokens;

    /// <summary>
    /// This property holds the list of tokens that make up the right hand operator.
    /// </summary>
    public IReadOnlyList<Token> RightTokens { get; internal init; }

    /// <summary>
    /// This property holds the left argument term for the binary operation.
    /// </summary>
    public DefaultExpressionTerm LeftTerm { get; internal init; }

    /// <summary>
    /// This property holds the middle argument term for the binary operation.
    /// </summary>
    public DefaultExpressionTerm MiddleTerm { get; internal init; }

    /// <summary>
    /// This property holds the right argument term for the binary operation.
    /// </summary>
    public DefaultExpressionTerm RightTerm { get; internal init; }

    /// <summary>
    /// This method produces a text representation of this term.
    /// </summary>
    /// <returns>A text representation of this term.</returns>
    protected override string Format()
    {
        string leftOperator = string.Join(' ', LeftTokens.Select(token => token.Text));
        string rightOperator = string.Join(' ', RightTokens.Select(token => token.Text));

        return $"{LeftTerm.Text} {leftOperator} {MiddleTerm.Text} {rightOperator} {RightTerm.Text}";
    }
}
