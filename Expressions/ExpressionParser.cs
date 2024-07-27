using Lex.Clauses;
using Lex.Parser;
using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class is used for parsing expressions into an expression tree.
/// </summary>
public class ExpressionParser
{
    /// <summary>
    /// This is an internal representation of a parsed binary or trinary operator.
    /// </summary>
    /// <param name="MultiTermOperator">The multi-term operator that matched.</param>
    /// <param name="Clause">The clause that holds the tokens that make up the operator.</param>
    private record Operator(MultiTermOperator MultiTermOperator, Clause Clause)
    {
        public static bool operator >=(Operator left, Operator right)
        {
            return left.MultiTermOperator.Precedence >= right.MultiTermOperator.Precedence;
        }

        public static bool operator <=(Operator left, Operator right)
        {
            return left.MultiTermOperator.Precedence <= right.MultiTermOperator.Precedence;
        }
    }

    private static readonly Dictionary<ExpressionParseMessageTypes, string> DefaultMessageMap = new ()
    {
        { ExpressionParseMessageTypes.MissingRequiredTerm, "Expecting a term here." },
        { ExpressionParseMessageTypes.MissingRequiredToken, "Expecting {0} here." },
        { ExpressionParseMessageTypes.TokenTypeMismatch, "Expecting {0} here." },
        { ExpressionParseMessageTypes.MissingExpressionSeparator, "Expecting an expression separator here." },
        { ExpressionParseMessageTypes.NotEnoughExpressions, "Not enough expressions specified." },
        { ExpressionParseMessageTypes.MissingRightParenthesis, "Expecting a right parenthesis here." },
        { ExpressionParseMessageTypes.MissingRightOperator, "Expecting the right trinary operator that goes with \"{0}\" here." }
    };

    /// <summary>
    /// This property holds the builder for actually building an expression tree.  It must
    /// be set prior to actually trying to parse an expression.
    /// </summary>
    public IExpressionTreeBuilder TreeBuilder
    {
        get => _treeBuilder;
        set => _treeBuilder = value ?? throw new ArgumentNullException(nameof(TreeBuilder), "The expression tree builder cannot be set to null.");
    }

    /// <summary>
    /// This property controls whether prefix unary operators have a higher priority than
    /// postfix unary operators.
    /// </summary>
    public bool PrefixFirst { get; set; } = true;

    private readonly List<TermChoiceParser> _termChoiceParsers = [];
    private readonly Dictionary<ExpressionParseMessageTypes, string> _messages = new (DefaultMessageMap);

    private IExpressionTreeBuilder _treeBuilder = new DefaultTreeBuilder();
    private ExpressionPossibilitySet _prefixOperators;
    private ExpressionPossibilitySet _postfixOperators;
    private List<MultiTermOperator> _binaryOperators;
    private List<MultiTermOperator> _trinaryOperators;

    /// <summary>
    /// This method is used to add an option on what a term in an expression can look like.
    /// </summary>
    /// <param name="termChoiceParser">The term choice parser to add.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionParser AddTermChoiceParser(TermChoiceParser termChoiceParser)
    {
        ArgumentNullException.ThrowIfNull(termChoiceParser, nameof(termChoiceParser));

        _termChoiceParsers.Add(termChoiceParser);

        return this;
    }

    /// <summary>
    /// This method is used to add a new unary operator option to the expression parser.
    /// </summary>
    /// <param name="operatorTokens">The set of tokens that make up the operator.</param>
    /// <param name="canBePrefix">Whether the operator can appear before its term argument.</param>
    /// <param name="canBePostfix">Whether the operator can appear after its term argument.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionParser AddUnaryOperatorParser(
        Token[] operatorTokens, bool canBePrefix, bool canBePostfix)
    {
        if (!canBePrefix && !canBePostfix)
            throw new ArgumentException("At least one of canBePrefix or canBePostfix must be true.");

        if (canBePrefix)
        {
            _prefixOperators ??= new ExpressionPossibilitySet();

            _prefixOperators.AddChoice(operatorTokens);
        }

        if (canBePostfix)
        {
            _postfixOperators ??= new ExpressionPossibilitySet();

            _postfixOperators.AddChoice(operatorTokens);
        }

        return this;
    }

    /// <summary>
    /// This method is used to add a new binary operator option to the expression parser.
    /// </summary>
    /// <param name="operatorTokens">The set of tokens that make up the operator.</param>
    /// <param name="precedence">The precedence of the operator.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionParser AddBinaryOperatorParser(Token[] operatorTokens, OperatorPrecedence precedence)
    {
        return AddBinaryOperatorParser(operatorTokens, (int) precedence);
    }

    /// <summary>
    /// This method is used to add a new binary operator option to the expression parser.
    /// </summary>
    /// <remarks>
    /// Typical values for operator precedence may be found in the
    /// <see cref="OperatorPrecedence"/> enum.
    /// </remarks>
    /// <param name="operatorTokens">The set of tokens that make up the operator.</param>
    /// <param name="precedence">The precedence of the operator.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionParser AddBinaryOperatorParser(Token[] operatorTokens, int precedence)
    {
        ExpressionPossibilitySet.ValidateTokens(operatorTokens, nameof(operatorTokens));

        _binaryOperators ??= [];
        
        _binaryOperators.Add(new MultiTermOperator
        {
            Operator1 = ExpressionPossibilitySet.CreateItemClauseParser(operatorTokens),
            Precedence = precedence
        });

        return this;
    }

    /// <summary>
    /// This method is used to add a new trinary operator option to the expression parser.
    /// </summary>
    /// <param name="leftOperatorTokens">The set of tokens that make up the left operator.</param>
    /// <param name="rightOperatorTokens">The set of tokens that make up the right operator.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionParser AddTrinaryOperatorParser(
        Token[] leftOperatorTokens, Token[] rightOperatorTokens)
    {
        ExpressionPossibilitySet.ValidateTokens(leftOperatorTokens, nameof(leftOperatorTokens));
        ExpressionPossibilitySet.ValidateTokens(rightOperatorTokens, nameof(rightOperatorTokens));

        _trinaryOperators ??= [];
        
        _trinaryOperators.Add(new MultiTermOperator
        {
            Operator1 = ExpressionPossibilitySet.CreateItemClauseParser(leftOperatorTokens),
            Operator2 = ExpressionPossibilitySet.CreateItemClauseParser(rightOperatorTokens)
        });

        return this;
    }

    /// <summary>
    /// This method is used to parse an expression.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <returns>The parsed expression as a term.</returns>
    public IExpressionTerm ParseExpression(LexicalParser parser)
    {
        if (_termChoiceParsers.Count == 0)
            throw new Exception("No term formats have been included in the expression parser.");

        return ParseExpressionTerm(parser);
    }

    /// <summary>
    /// This method is used to parse an expression.  We rely on the public one to verify
    /// the parser is in a proper state, which allows this to recurse as necessary.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="optional">A flag noting whether the term is optional or not.</param>
    /// <returns>The parsed expression as a term.</returns>
    internal IExpressionTerm ParseExpressionTerm(LexicalParser parser, bool optional = false)
    {
        IExpressionTerm term = ParseBinaryTerm(parser, optional);

        if (term == null)
            return null;

        if (_trinaryOperators == null)
            return term;

        Operator leftOperator = ParseOperator(parser, _trinaryOperators);

        if (leftOperator != null)
        {
            IExpressionTerm left = term;
            IExpressionTerm middle = ParseExpressionTerm(parser);
            Clause rightOperator = leftOperator.MultiTermOperator.Operator2.TryParse(parser);

            if (rightOperator == null)
            {
                string text = string.Join(' ', leftOperator.Clause.Tokens.Select(token => token.Text));

                throw new TokenException(ResolveMessage(ExpressionParseMessageTypes.MissingRightOperator, text))
                {
                    Token = parser.PeekNextToken()
                };
            }

            IExpressionTerm right = ParseExpressionTerm(parser);

            term = _treeBuilder.CreateTrinaryOperation(
                leftOperator.Clause.Tokens, rightOperator.Tokens, left, middle, right);
        }

        return term;
    }

    /// <summary>
    /// This method is used to parse an expression.  We rely on the public one to verify
    /// we have a tree builder, and other such state things, which allows this to recurse
    /// as necessary.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="optional">A flag noting whether the term is optional or not.</param>
    /// <returns>The parsed expression as a term.</returns>
    private IExpressionTerm ParseBinaryTerm(LexicalParser parser, bool optional)
    {
        IExpressionTerm term = ParseUnaryTerm(parser, optional);

        if (term == null)
            return null;

        if (_binaryOperators == null)
            return term;

        Operator op = ParseOperator(parser, _binaryOperators);

        if (op != null)
        {
            List<IExpressionTerm> terms = [term];
            List<Operator> operators = [op];

            terms.Add(ParseUnaryTerm(parser, false));

            op = ParseOperator(parser, _binaryOperators);

            while (op != null)
            {
                operators.Add(op);
                terms.Add(ParseUnaryTerm(parser, false));

                TryToReduce(terms, operators);

                op = ParseOperator(parser, _binaryOperators);
            }

            // Now, reduce the expression down to a single term.
            while (operators.Count > 0)
                DoBinaryReduction(terms, operators, operators.Count - 1);

            term = terms.First();
        }

        return term;
    }

    /// <summary>
    /// This method looks at the last two operators and, if the next to last one has a
    /// precedence that is the same or larger than the last one, the next to last is
    /// combined into a single term.
    /// </summary>
    /// <param name="terms">The current set of terms in the expression.</param>
    /// <param name="operators">The current set of operators in the expression.</param>
    private void TryToReduce(List<IExpressionTerm> terms, List<Operator> operators)
    {
        int index1 = operators.Count - 2;
        int index2 = index1 + 1;

        if (operators[index1] >= operators[index2])
            DoBinaryReduction(terms, operators, index1);
    }

    /// <summary>
    /// This method performs a binary operation reduction on the given lists of terms and
    /// operators.
    /// </summary>
    /// <param name="terms">The current set of terms in the expression.</param>
    /// <param name="operators">The current set of operators in the expression.</param>
    /// <param name="index">The index where the reduction should take pace.</param>
    private void DoBinaryReduction(List<IExpressionTerm> terms, List<Operator> operators, int index)
    {
        int next = index + 1;

        terms[next] = _treeBuilder.CreateBinaryOperation(
            operators[index].Clause.Tokens, terms[index], terms[next]);

        terms.RemoveAt(index);
        operators.RemoveAt(index);
    }

    /// <summary>
    /// This method is used to parse a term, resolving any unary operators that are present.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="optional">A flag noting whether the term is optional or not.</param>
    /// <returns>The parsed expression as a term.</returns>
    private IExpressionTerm ParseUnaryTerm(LexicalParser parser, bool optional)
    {
        // 1. Queue up any prefix operators.
        List<Clause> prefixClauses = GatherUnaryOperators(parser, _prefixOperators);

        // If we have any prefix operators, the term can no longer be optional.
        if (prefixClauses.Count > 0)
            optional = false;

        // 2. Now, parse the term itself.
        IExpressionTerm term = parser.IsNext(BounderToken.LeftParen)
            ? ParseParentheticalExpression(parser)
            : ParseFundamentalTerm(parser, optional);

        if (term == null)
            return null;

        // 3. Next, queue up any postfix operators.
        List<Clause> postfixClauses = GatherUnaryOperators(parser, _postfixOperators);

        // 4. Now, wrap the term with the appropriate operators.
        if (PrefixFirst)
        {
            term = WrapTermWithUnaryOperators(
                WrapTermWithUnaryOperators(term, prefixClauses, true),
                postfixClauses, false);
        }
        else
        {
            term = WrapTermWithUnaryOperators(
                WrapTermWithUnaryOperators(term, postfixClauses, false),
                prefixClauses, true);
        }

        return term;
    }

    /// <summary>
    /// This is a helper method for gathering a list of unary operators.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="set">The unary operator collection to use.</param>
    /// <returns>A list of any operator clauses we found.</returns>
    private static List<Clause> GatherUnaryOperators(LexicalParser parser, ExpressionPossibilitySet set)
    {
        List<Clause> result = [];
        Clause clause = set?.TryParse(parser);

        while (clause != null)
        {
            result.Add(clause);

            clause = set.TryParse(parser);
        }

        return result;
    }

    /// <summary>
    /// This is a helper method for applying a list of unary operators to a term.
    /// </summary>
    /// <param name="term">The term to start with.</param>
    /// <param name="operators">The list of operators to apply</param>
    /// <param name="isPrefix">A flag noting whether the operators are prefix or postfix.</param>
    /// <returns>The resulting term.</returns>
    private IExpressionTerm WrapTermWithUnaryOperators(IExpressionTerm term, List<Clause> operators, bool isPrefix)
    {
        // Short-circuit...
        if (operators.Count == 0)
            return term;

        if (isPrefix)
            operators.Reverse();

        term = operators.Aggregate(
            term,
            (current, clause) => _treeBuilder.CreateUnaryOperation(clause.Tokens, current, isPrefix));

        return term;
    }

    /// <summary>
    /// This method is used to parse a parenthetical expression.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <returns>The term that represents the contents of the parentheses.</returns>
    private IExpressionTerm ParseParentheticalExpression(LexicalParser parser)
    {
        _ = parser.GetNextToken(); // Eat the left parenthesis.

        IExpressionTerm term = ParseExpressionTerm(parser);

        _ = parser.MatchToken(
            true,
            () => ResolveMessage(ExpressionParseMessageTypes.MissingRightParenthesis),
            BounderToken.RightParen);

        return term;
    }

    /// <summary>
    /// This method is used to parse a "fundamental" term.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="optional">A flag noting whether the term is optional or not.</param>
    /// <returns>The Basic term we found.</returns>
    private IExpressionTerm ParseFundamentalTerm(LexicalParser parser, bool optional)
    {
        Token token = parser.PeekNextToken();

        foreach (var term in _termChoiceParsers
                     .Select(termChoiceParser => termChoiceParser.TryParse(this, parser))
                     .Where(term => term != null))
        {
            return term;
        }

        if (optional)
            return null;

        throw new TokenException(ResolveMessage(ExpressionParseMessageTypes.MissingRequiredTerm))
        {
            Token = token
        };
    }

    /// <summary>
    /// This is a helper method that will try to parse an operator.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="operators">The list of operators to check for.</param>
    /// <returns>The parsed operator or <c>null</c>.</returns>
    private static Operator ParseOperator(LexicalParser parser, List<MultiTermOperator> operators)
    {
        foreach (MultiTermOperator op in operators)
        {
            Clause clause = op.Operator1.TryParse(parser);

            if (clause != null)
                return new Operator(op, clause);
        }

        return null;
    }

    /// <summary>
    /// This method allows you to set the text to report in the various token exceptions
    /// that the expression parser may throw.
    /// </summary>
    /// <param name="messageType">The type of message to set the text for.</param>
    /// <param name="text">The new text to report for the given type of message.</param>
    /// <returns>This object, for fluency.</returns>
    public ExpressionParser SetMessageText(ExpressionParseMessageTypes messageType, string text)
    {
        if (messageType == ExpressionParseMessageTypes.None)
            throw new ArgumentException("Cannot set the text for the \"None\" message type.", nameof(messageType));

        if (text == null || text.Trim().Length == 0)
            throw new ArgumentException("You must set the text for a message type to a non-empty string.", nameof(text));

        _messages[messageType] = text.Trim();

        return this;
    }

    /// <summary>
    /// This method is used to look up, and potentially format, a message string.
    /// </summary>
    /// <param name="messageType">The message type to resolve.</param>
    /// <param name="arguments">Any arguments for the message to format.</param>
    /// <returns>The resolved message text.</returns>
    internal string ResolveMessage(ExpressionParseMessageTypes messageType, params string[] arguments)
    {
        object[] args = arguments.Cast<object>().ToArray();

        return string.Format(_messages[messageType], args);
    }
}
