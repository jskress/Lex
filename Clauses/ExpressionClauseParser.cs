using Lex.Expressions;
using Lex.Parser;

namespace Lex.Clauses;

/// <summary>
/// This class represents a "clause" where some number of expressions are expected to
/// occur. 
/// </summary>
public class ExpressionClauseParser : ClauseParser
{
    private readonly ExpressionParser _expressionParser;

    private bool _optional;

    public ExpressionClauseParser(ExpressionParser expressionParser)
    {
        ArgumentNullException.ThrowIfNull(expressionParser, nameof(expressionParser));

        _expressionParser = expressionParser;
        _optional = false;
    }

    /// <summary>
    /// This method is used to set the optional state of the expression.  By default, an
    /// expression is not optional.
    /// </summary>
    /// <param name="newState">The new state for whether an expression is optional.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionClauseParser SetIsOptional(bool newState)
    {
        _optional = newState;

        return this;
    }

    /// <summary>
    /// This method tries to parse,
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The list of tokens matching the clause, or <c>null</c>, if not.</returns>
    protected override Clause TryParseClause(LexicalParser parser)
    {
        IExpressionTerm expression = _expressionParser.ParseExpressionTerm(parser, _optional);

        return new Clause
        {
            Expressions = [expression]
        };
    }
}
