using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class provides a default implementation for the expression builder interface.
/// </summary>
public class DefaultTreeBuilder : IExpressionTreeBuilder
{
    /// <summary>
    /// This method is used to create a term in an expression tree.  It is provided the
    /// list of tokens that are not part of a sub-expression or decoration tokens and the
    /// list of any sub-expressions.
    /// </summary>
    /// <param name="tokens">The list of relevant tokens that make up the term.</param>
    /// <param name="expressions">The list of any sub-expression objects.</param>
    /// <param name="tag">The tag that goes with the type of term parsed.</param>
    /// <returns>The created term.</returns>
    public IExpressionTerm CreateTerm(List<Token> tokens, List<IExpressionTerm> expressions, string tag)
    {
        return new DefaultBasicTerm
        {
            Tokens = tokens,
            Terms = expressions.Cast<DefaultExpressionTerm>().ToList(),
            Tag = tag
        };
    }

    /// <summary>
    /// This method is used to create a term that represents a unary operation.
    /// </summary>
    /// <param name="tokens">The list of tokens that define the operator.</param>
    /// <param name="term">The term the operator should act on.</param>
    /// <param name="isPrefix">A flag that indicates whether the operator preceded the term
    /// or followed it.</param>
    /// <returns>A term that represents a unary operation.</returns>
    public IExpressionTerm CreateUnaryOperation(List<Token> tokens, IExpressionTerm term, bool isPrefix)
    {
        return new DefaultUnaryOperation
        {
            Tokens = tokens,
            Term = (DefaultExpressionTerm) term,
            IsPrefix = isPrefix
        };
    }

    /// <summary>
    /// This method is used to create a term that represents a binary operation.
    /// </summary>
    /// <param name="tokens">The list of tokens that define the operator.</param>
    /// <param name="left">The left-hand term the operator should act on.</param>
    /// <param name="right">The right-hand term the operator should act on.</param>
    /// <returns>A term that represents a binary operation.</returns>
    public IExpressionTerm CreateBinaryOperation(List<Token> tokens, IExpressionTerm left, IExpressionTerm right)
    {
        return new DefaultBinaryOperation
        {
            Tokens = tokens,
            LeftTerm = (DefaultExpressionTerm) left,
            RightTerm = (DefaultExpressionTerm) right
        };
    }

    /// <summary>
    /// This method is used to create a term that represents a trinary operation.
    /// </summary>
    /// <param name="leftTokens">The list of tokens that define the left operator.</param>
    /// <param name="rightTokens">The list of tokens that define the right operator.</param>
    /// <param name="left">The left-hand term the operator should act on.</param>
    /// <param name="middle">The middle term the operator should act on.</param>
    /// <param name="right">The right-hand term the operator should act on.</param>
    /// <returns>A term that represents a trinary operation.</returns>
    public IExpressionTerm CreateTrinaryOperation(List<Token> leftTokens, List<Token> rightTokens, IExpressionTerm left, IExpressionTerm middle, IExpressionTerm right)
    {
        return new DefaultTrinaryOperation
        {
            Tokens = leftTokens,
            RightTokens = rightTokens,
            LeftTerm = (DefaultExpressionTerm) left,
            MiddleTerm = (DefaultExpressionTerm) middle,
            RightTerm = (DefaultExpressionTerm) right
        };
    }
}
