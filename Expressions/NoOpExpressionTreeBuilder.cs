using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class provides a tree builder that actually does nothing.  You will use this when
/// you want to parse an expression but will not do anything with the result of the parsing.
/// </summary>
public class NoOpExpressionTreeBuilder : IExpressionTreeBuilder
{
    /// <summary>
    /// This method is used to create a term in an expression tree.  It is provided the
    /// list of tokens that are not part of a sub-expression or decoration tokens and the
    /// list of any sub-expressions.
    /// </summary>
    /// <param name="tokens">The list of relevant tokens that make up the term.</param>
    /// <param name="expressions">The list of any sub-expression objects.</param>
    /// <param name="tag">The tag that goes with the type of term parsed.</param>
    /// <returns>Our no-op term, always.</returns>
    public IExpressionTerm CreateTerm(List<Token> tokens, List<IExpressionTerm> expressions, string tag)
    {
        return NoOpTerm.Instance;
    }

    /// <summary>
    /// This method is used to create a term that represents a unary operation.
    /// </summary>
    /// <param name="tokens">The list of tokens that define the operator.</param>
    /// <param name="term">The term the operator should act on.</param>
    /// <param name="isPrefix">A flag that indicates whether the operator preceded the term
    /// or followed it.</param>
    /// <returns>Our no-op term, always.</returns>
    public IExpressionTerm CreateUnaryOperation(List<Token> tokens, IExpressionTerm term, bool isPrefix)
    {
        return NoOpTerm.Instance;
    }

    /// <summary>
    /// This method is used to create a term that represents a binary operation.
    /// </summary>
    /// <param name="tokens">The list of tokens that define the operator.</param>
    /// <param name="left">The left-hand term the operator should act on.</param>
    /// <param name="right">The right-hand term the operator should act on.</param>
    /// <returns>Our no-op term, always.</returns>
    public IExpressionTerm CreateBinaryOperation(List<Token> tokens, IExpressionTerm left, IExpressionTerm right)
    {
        return NoOpTerm.Instance;
    }

    /// <summary>
    /// This method is used to create a term that represents a trinary operation.
    /// </summary>
    /// <param name="leftTokens">The list of tokens that define the left operator.</param>
    /// <param name="rightTokens">The list of tokens that define the right operator.</param>
    /// <param name="left">The left-hand term the operator should act on.</param>
    /// <param name="middle">The middle term the operator should act on.</param>
    /// <param name="right">The right-hand term the operator should act on.</param>
    /// <returns>Our no-op term, always.</returns>
    public IExpressionTerm CreateTrinaryOperation(List<Token> leftTokens, List<Token> rightTokens, IExpressionTerm left, IExpressionTerm middle, IExpressionTerm right)
    {
        return NoOpTerm.Instance;
    }
}
