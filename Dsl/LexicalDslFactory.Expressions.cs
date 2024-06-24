using Lex.Clauses;
using Lex.Expressions;
using Lex.Parser;
using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This class implements a DSL (go figure) for defining and configuring a DSL.
/// </summary>
public static partial class LexicalDslFactory
{
    // Some tokens we will look for...
    private static readonly IdToken TermToken = new ("term");
    private static readonly IdToken UnaryToken = new ("unary");
    private static readonly IdToken BinaryToken = new ("binary");
    private static readonly IdToken TrinaryToken = new ("trinary");
    private static readonly IdToken MessagesToken = new ("messages");
    private static readonly IdToken PostfixFirst = new ("postfixFirst");
    
    // And some clauses to make things easier...
    private static readonly ClauseParser TermClause = new SequentialClauseParser()
        .Matching(TermToken)
        .Then("Expecting a colon after \"term\" here.", OperatorToken.Colon)
        .Then("Expecting an open bracket after \"term:\" here.", BounderToken.OpenBracket);
    private static readonly ClauseParser UnaryClause = new SequentialClauseParser()
        .Matching(UnaryToken)
        .Then("Expecting a colon after \"unary\" here.", OperatorToken.Colon)
        .Then("Expecting an open bracket after \"unary:\" here.", BounderToken.OpenBracket);
    private static readonly ClauseParser BinaryClause = new SequentialClauseParser()
        .Matching(BinaryToken)
        .Then("Expecting a colon after \"binary\" here.", OperatorToken.Colon)
        .Then("Expecting an open bracket after \"binary:\" here.", BounderToken.OpenBracket);
    private static readonly ClauseParser TrinaryClause = new SequentialClauseParser()
        .Matching(TrinaryToken)
        .Then("Expecting a colon after \"trinary\" here.", OperatorToken.Colon)
        .Then("Expecting an open bracket after \"trinary:\" here.", BounderToken.OpenBracket);
    private static readonly ClauseParser MessagesClause = new SequentialClauseParser()
        .Matching(MessagesToken)
        .Then("Expecting a colon after \"messages\" here.", OperatorToken.Colon)
        .Then("Expecting an open bracket after \"messages:\" here.", BounderToken.OpenBracket);
    private static readonly ClauseParser ExpressionSubClause = new SwitchClauseParser()
        .Matching(TermClause, nameof(ProcessExpressionTermSpec))
        .Or(UnaryClause, nameof(ProcessExpressionUnaryOperatorSpec))
        .Or(BinaryClause, nameof(ProcessExpressionBinaryOperatorSpec))
        .Or(TrinaryClause, nameof(ProcessExpressionTrinaryOperatorSpec))
        .Or(MessagesClause, nameof(ProcessExpressionMessagesSpec))
        .OnNoClausesMatched("Expecting \"term:\", \"unary:\", \"binary:\", \"trinary:\" or \"messages:\" followed by an open bracket here.");

    private static readonly Dictionary<
            string, Action<IDictionary<string, object>, ExpressionParser, List<Token>>>
        ExpressionSpecProcessors = new ()
        {
            { nameof(ProcessExpressionTermSpec), ProcessExpressionTermSpec },
            { nameof(ProcessExpressionUnaryOperatorSpec), ProcessExpressionUnaryOperatorSpec },
            { nameof(ProcessExpressionBinaryOperatorSpec), ProcessExpressionBinaryOperatorSpec },
            { nameof(ProcessExpressionTrinaryOperatorSpec), ProcessExpressionTrinaryOperatorSpec },
            { nameof(ProcessExpressionMessagesSpec), ProcessExpressionMessagesSpec }
        };
    private static readonly Dictionary<string, string> TermTagNouns = new ()
    {
        { nameof(ProcessExpressionTermSpec), "term" },
        { nameof(ProcessExpressionUnaryOperatorSpec), "unary operator" },
        { nameof(ProcessExpressionBinaryOperatorSpec), "binary operator" },
        { nameof(ProcessExpressionTrinaryOperatorSpec), "trinary operator" },
        { nameof(ProcessExpressionMessagesSpec), "messages" },
    };

    /// <summary>
    /// This method is used to process the expressions specification clause.
    /// </summary>
    /// <param name="parser">The parser that is being used to parse the DSL specification.</param>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="tokens">The list of tokens that make up the clause to process.</param>
    /// <param name="dsl">The DSL we are building up</param>
    private static void HandleExpressionsClause(
        LexicalParser parser, IDictionary<string, object> variables, List<Token> tokens, Dsl dsl)
    {
        if (dsl.ExpressionParser != null)
        {
            throw new TokenException("An expression specification has already been set.")
            {
                Token = tokens.First()
            };
        }

        ExpressionParser expressionParser = new ExpressionParser();
        bool termWasSpecified = false;

        while (!parser.IsAtEnd() && !parser.IsNext(BounderToken.CloseBrace))
        {
            Clause clause = ExpressionSubClause.TryParse(parser);

            ProcessExpressionSubClause(parser, variables, expressionParser, clause.Tag);

            termWasSpecified |= clause.Tag == nameof(ProcessExpressionTermSpec);
        }

        if (!termWasSpecified)
        {
            throw new TokenException("No term clause specified for expressions.")
            {
                Token = parser.GetNextToken()
            };
        }

        parser.GetNextToken(); // Eat the close brace.

        dsl.ExpressionParser = expressionParser;
    }

    /// <summary>
    /// This method handles parsing a term clause for an expression.
    /// </summary>
    /// <param name="parser">The parser to pull tokens from.</param>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="expressionParser">The expression parser to populate.</param>
    /// <param name="tag">The tag to use in handling list items</param>
    private static void ProcessExpressionSubClause(
        LexicalParser parser, IDictionary<string, object> variables, ExpressionParser expressionParser,
        string tag)
    {
        Token token = parser.GetRequiredToken(
            () => $"Expecting a {TermTagNouns[tag]} spec or close bracket here.");
        Action<IDictionary<string, object>, ExpressionParser, List<Token>> processor =
            ExpressionSpecProcessors[tag];

        parser.ReturnToken(token);

        while (!BounderToken.CloseBracket.Matches(token))
        {
            List<Token> tokens = ParseUntil(
                parser, OperatorToken.Comma, BounderToken.CloseBracket);

            if (tokens.Count == 0)
            {
                throw new TokenException($"Expecting a {TermTagNouns[tag]} spec here.")
                {
                    Token = parser.PeekNextToken()
                };
            }

            processor.Invoke(variables, expressionParser, tokens);

            if (tokens.Count > 0)
            {
                throw new TokenException("Unexpected token found here.")
                {
                    Token = tokens.First()
                };
            }

            token = parser.MatchToken(
                true, () => "Expecting a comma or close bracket here.",
                OperatorToken.Comma, BounderToken.CloseBracket);
        }
    }

    /// <summary>
    /// This is a helper method for queuing up a list of tokens up to a given one.  Note
    /// that we deliberately don't consume the token that stopped us.
    /// </summary>
    /// <param name="parser">The parser to pull tokens from.</param>
    /// <param name="endingTokens">The tokens that can stop the gathering.</param>
    /// <returns>The list of gathered tokens.</returns>
    private static List<Token> ParseUntil(LexicalParser parser, params Token[] endingTokens)
    {
        List<Token> tokens = [];
        bool inParentheses = false;

        while (!parser.IsAtEnd() && !(!inParentheses && parser.IsNext(endingTokens)))
        {
            Token token = parser.GetNextToken();

            inParentheses = inParentheses switch
            {
                false when BounderToken.LeftParen.Matches(token) => true,
                true when BounderToken.RightParen.Matches(token) => false,
                _ => inParentheses
            };

            tokens.Add(token);
        }

        return tokens;
    }

    /// <summary>
    /// This method handles parsing a term spec for an expression.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="expressionParser">The expression parser to populate.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    private static void ProcessExpressionTermSpec(
        IDictionary<string, object> variables, ExpressionParser expressionParser,  List<Token> tokens)
    {
        TermChoiceParser termChoiceParser = new TermChoiceParser();

        while (tokens.Count > 0)
        {
            Token suppressToken = null;
            bool suppress = OperatorToken.Divide.Matches(tokens.First());

            if (suppress)
            {
                suppressToken = tokens.RemoveFirst();

                if (tokens.Count == 0)
                {
                    throw new TokenException("Expecting a term spec here.")
                    {
                        Token = suppressToken
                    };
                }
            }

            Token token = tokens.RemoveFirst();

            if (token.Text == "_expression")
                AddTermChoiceExpressionEntry(variables, tokens, termChoiceParser, suppressToken);
            else
                AddTermChoiceEntry(variables, termChoiceParser, token, suppressToken, tokens);

            if (ProcessExpressionTermTagClause(tokens, termChoiceParser))
                break;
        }

        expressionParser.AddTermChoiceParser(termChoiceParser);
    }

    /// <summary>
    /// This is a helper method for adding an expression placeholder to a term choice.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    /// <param name="termChoiceParser">The term choice parser to update.</param>
    /// <param name="suppressToken">The suppression token, if any.</param>
    private static void AddTermChoiceExpressionEntry(
        IDictionary<string, object> variables, List<Token> tokens, TermChoiceParser termChoiceParser,
        Token suppressToken)
    {
        if (suppressToken != null)
        {
            throw new TokenException("Expressions cannot be suppressed.")
            {
                Token = suppressToken
            };
        }

        ExpressionPossibilitySet separators = null;
        int min = 1;
        int max = 1;

        if (BounderToken.LeftParen.Matches(tokens.FirstOrDefault()))
        {
            tokens.RemoveFirst(); // Eat the left parenthesis.

            (min, max) = ProcessExpressionCountNotation(tokens);

            if (max > 1)
                separators = ProcessExpressionSeparators(variables, tokens);

            if (!BounderToken.RightParen.Matches(tokens.FirstOrDefault()))
            {
                throw new TokenException("Expecting a right parenthesis here.")
                {
                    Token = tokens.FirstOrDefault()
                };
            }

            tokens.RemoveFirst(); // Eat the right parenthesis.
        }

        termChoiceParser.ExpectingExpression(new ExpressionTermChoiceItem(min, max, separators));
    }

    /// <summary>
    /// This method is used to process the token that represents the number of expressions
    /// that should be expected/allowed.
    /// </summary>
    /// <param name="tokens">The list of tokens to process.</param>
    /// <returns>A tuple containg<c>true</c>, if separators should be expected, or <c>false</c>, if not.</returns>
    private static (int, int) ProcessExpressionCountNotation(List<Token> tokens)
    {
        Token token = tokens.FirstOrDefault();
        int min = 1;
        int max = 1;

        if (!BounderToken.RightParen.Matches(token))
        {
            bool error = false;

            if (token is OperatorToken operatorToken)
            {
                if (OperatorToken.Multiply.Matches(operatorToken))
                {
                    min = 0;
                    max = int.MaxValue;
                }
                else if (OperatorToken.Plus.Matches(operatorToken))
                    max = int.MaxValue;
                else if (OperatorToken.If.Matches(operatorToken))
                    min = 0;
                else
                    error = true;

                if (!error)
                    tokens.RemoveFirst();
            }
            else
            {
                (min, max) = ParseMinMax(tokens, 1, 1);

                if (max == 0)
                    error = true;
            }

            if (error)
            {
                throw new TokenException("Expecting a '*', '+', '?' or a number greater than 1 or range here.")
                {
                    Token = token
                };
            }
        }

        return (min, max);
    }

    /// <summary>
    /// This method handles parsing the list of separators for an expression list into the
    /// given item.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    /// <returns>A possibility set that represents what separators to look for.</returns>
    private static ExpressionPossibilitySet ProcessExpressionSeparators(
        IDictionary<string, object> variables, List<Token> tokens)
    {
        ExpressionPossibilitySet set = new ExpressionPossibilitySet();
        List<Token> work = [];

        while (tokens.Count > 0)
        {
            if (!OperatorToken.Comma.Matches(tokens.First()))
                break;

            tokens.RemoveFirst();

            while (tokens.Count > 0 && tokens[0] is IdToken)
                work.Add(ResolveVariableAsToken(variables, tokens.RemoveFirst()));

            set.AddChoice(work.ToArray());
        }

        return set.IsEmpty ? null : set;
    }

    /// <summary>
    /// This is a helper method for adding a token or type to the given term choice parser.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="parser">The term choice parser to add to.</param>
    /// <param name="token">The token that refers to the thing to add.</param>
    /// <param name="suppressToken">The suppression token for the new entry.</param>
    /// <param name="tokens">The list of tokens we are working with.</param>
    private static void AddTermChoiceEntry(
        IDictionary<string, object> variables, TermChoiceParser parser, Token token,
        Token suppressToken, List<Token> tokens)
    {
        string text = token.Text;
        Token tokenValue;
        Type typeValue;

        if (text[0] == '_' && TokenTypesMap.TryGetValue(text, out var type))
            (tokenValue, typeValue) = ResolveTypeReference(tokens, text, type);
        else
            (tokenValue, typeValue, _) = ResolveVariableReference(variables, token, text);

        if (tokenValue != null)
        {
            if (parser.IsEmpty)
                parser.Matching(tokenValue, suppressToken != null);
            else
                parser.Then(tokenValue, suppressToken != null);
        }
        else if (typeValue != null)
        {
            if (suppressToken != null)
            {
                throw new TokenException("Term type reference cannot be suppressed.")
                {
                    Token = suppressToken
                };
            }

            if (parser.IsEmpty)
                parser.Matching(typeValue);
            else
                parser.Then(typeValue);
        }
        else
        {
            throw new TokenException(
                $"The identifier, {token.Text}, must refer to a variable that is a token " +
                $"or a type or a token definition.")
            {
                Token = token
            };
        }
    }

    /// <summary>
    /// This is a helper method that will process, if present, the tag clause for a term
    /// choice.
    /// </summary>
    /// <param name="tokens">The list of tokens to process.</param>
    /// <param name="termChoiceParser">The term choice parser to update.</param>
    /// <returns><c>true</c>, if a tag clause was found and processed.</returns>
    private static bool ProcessExpressionTermTagClause(
        List<Token> tokens, TermChoiceParser termChoiceParser)
    {
        if (OperatorToken.DoubleArrow.Matches(tokens.FirstOrDefault()))
        {
            Token arrowToken = tokens.RemoveFirst();
            Token next = tokens.RemoveFirst();

            if (next is StringToken token)
                termChoiceParser.Tag = token.Text;
            else
            {
                throw new TokenException("Expecting a string to follow '=>' here.")
                {
                    Token = next ?? arrowToken
                };
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// This method handles parsing a unary operator spec for an expression.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="expressionParser">The expression parser to populate.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    private static void ProcessExpressionUnaryOperatorSpec(
        IDictionary<string, object> variables, ExpressionParser expressionParser, List<Token> tokens)
    {
        if (tokens.Count > 1 &&
            OperatorToken.Colon.Matches(tokens[0]) &&
            PostfixFirst.Matches(tokens[1]))
        {
            expressionParser.PrefixFirst = false;

            tokens.RemoveRange(0, 2);

            return;
        }

        bool canBePostfix = IsMarkedWithExpression(tokens);
        Token[] operatorTokens = GatherOperatorTokens(variables, tokens, OperatorToken.Multiply);
        bool canBePrefix = IsMarkedWithExpression(tokens);

        expressionParser.AddUnaryOperatorParser(operatorTokens, canBePrefix, canBePostfix);
    }

    /// <summary>
    /// This is a helper method to test for the presence of the asterisk in the token
    /// stream.  If it is found, it is consumed.
    /// </summary>
    /// <param name="tokens">The list of tokens to examine.</param>
    /// <returns><c>true</c>, if an asterisk was found and removed, or <c>false</c>, if not.</returns>
    private static bool IsMarkedWithExpression(List<Token> tokens)
    {
        if (OperatorToken.Multiply.Matches(tokens.FirstOrDefault()))
        {
            tokens.RemoveFirst();

            return true;
        }

        return false;
    }

    /// <summary>
    /// This method handles parsing a binary operator spec for an expression.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="expressionParser">The expression parser to populate.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    private static void ProcessExpressionBinaryOperatorSpec(
        IDictionary<string, object> variables, ExpressionParser expressionParser, List<Token> tokens)
    {
        Token[] operatorTokens = GatherOperatorTokens(variables, tokens, OperatorToken.Colon);
        int precedence = ResolveOperatorPrecedence(tokens, operatorTokens);

        expressionParser.AddBinaryOperatorParser(operatorTokens, precedence);
    }

    /// <summary>
    /// This method handles parsing a trinary operator spec for an expression.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="expressionParser">The expression parser to populate.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    private static void ProcessExpressionTrinaryOperatorSpec(
        IDictionary<string, object> variables, ExpressionParser expressionParser, List<Token> tokens)
    {
        if (BounderToken.LeftParen.Matches(tokens.FirstOrDefault()))
            tokens.RemoveFirst();
        else
        {
            throw new TokenException("Expecting a left parenthesis here.")
            {
                Token = tokens.First()
            };
        }

        Token[] leftOperatorTokens = GatherOperatorTokens(variables, tokens, OperatorToken.Comma);

        if (tokens.Count > 0)
            tokens.RemoveFirst(); // Eat the comma.
        else
        {
            throw new TokenException("The trinary operator specification is not properly formed.")
            {
                Token = leftOperatorTokens[0]
            };
        }

        Token[] rightOperatorTokens =
            GatherOperatorTokens(variables, tokens, BounderToken.RightParen);

        if (BounderToken.RightParen.Matches(tokens.FirstOrDefault()))
            tokens.RemoveFirst();
        else
        {
            throw new TokenException("Expecting a right parenthesis here.")
            {
                Token = tokens.FirstOrDefault()
            };
        }

        expressionParser.AddTrinaryOperatorParser(leftOperatorTokens, rightOperatorTokens);
    }

    /// <summary>
    /// This is a helper method for creating a sequential clause parser from a series of
    /// tokens.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    /// <param name="marker">The marker token that that can stop the gathering.</param>
    /// <returns>The array of tokens that make up the operator.</returns>
    private static Token[] GatherOperatorTokens(
        IDictionary<string, object> variables, List<Token> tokens, Token marker)
    {
        List<Token> work = [];

        while (tokens.Count > 0 && !marker.Matches(tokens.First()))
            work.Add(ResolveVariableAsToken(variables, tokens.RemoveFirst()));

        if (work.Count == 0)
        {
            throw new TokenException("Expecting a variable reference here.")
            {
                Token = tokens.FirstOrDefault()
            };
        }

        return work.ToArray();
    }

    /// <summary>
    /// This is a helper method for determining the precedence of an operator.
    /// </summary>
    /// <param name="tokens">The list of tokens to process.</param>
    /// <param name="operatorTokens">The set of tokens that make up the relevant operator.</param>
    /// <returns>The precedence to use.</returns>
    private static int ResolveOperatorPrecedence(List<Token> tokens, Token[] operatorTokens)
    {
        OperatorPrecedence precedence;

        // If the precedence is explicitly given...
        if (OperatorToken.Colon.Matches(tokens.FirstOrDefault()))
        {
            Token colon = tokens.RemoveFirst();

            if (tokens.Count == 0)
            {
                throw new TokenException("A precedence indicator is required after the colon.")
                {
                    Token = colon
                };
            }

            Token token = tokens.RemoveFirst();

            return token switch
            {
                NumberToken numberToken => (int) numberToken.IntegralNumber,
                IdToken idToken when Enum.TryParse(idToken.Text, true, out precedence) =>
                    (int) precedence,
                _ => throw new TokenException("Expecting a precedence value or number here.")
                {
                    Token = token
                }
            };
        }
        
        // Not explicit, infer it if possible.
        if (operatorTokens.Length == 1 && operatorTokens[0] is OperatorToken &&
            OperatorInfo.Precedences.TryGetValue(operatorTokens[0].Text, out precedence))
            return (int) precedence;

        throw new TokenException("Cannot determine the precedence of the operator.")
        {
            Token = operatorTokens[0]
        };
    }

    /// <summary>
    /// This method is used to resolve the value of a variable as a token.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="token">The token that represents the variable to resolve.</param>
    /// <returns>The token that the given one resolves to.</returns>
    private static Token ResolveVariableAsToken(IDictionary<string, object> variables, Token token)
    {
        if (token is IdToken idToken)
        {
            if (variables.TryGetValue(idToken.Text, out object value) &&
                value is Token valueToken)
                return valueToken;

            throw new TokenException($"The identifier, '{token.Text}', does not refer to a token.")
            {
                Token = idToken
            };
        }

        throw new TokenException("Expecting an identifier here.")
        {
            Token = token
        };
    }

    /// <summary>
    /// This method handles parsing an error spec for an expression.
    /// </summary>
    /// <param name="variables">The set of variables we are using.</param>
    /// <param name="expressionParser">The expression parser to populate.</param>
    /// <param name="tokens">The list of tokens to process.</param>
    private static void ProcessExpressionMessagesSpec(
        IDictionary<string, object> variables, ExpressionParser expressionParser,  List<Token> tokens)
    {
        while (tokens.Count > 0)
        {
            Token typeToken = tokens.RemoveFirst();
            Token colonToken = tokens.RemoveFirst();
            Token textToken = tokens.RemoveFirst();

            if (typeToken is not IdToken ||
                !Enum.TryParse(typeToken.Text, true, out ExpressionParseMessageTypes messageType) ||
                messageType == ExpressionParseMessageTypes.None)
            {
                throw new TokenException($"\"{typeToken.Text}\" is an unsupported message type here.")
                {
                    Token = typeToken
                };
            }

            if (!OperatorToken.Colon.Matches(colonToken))
            {
                throw new TokenException("Expecting a colon after the message type here.")
                {
                    Token = colonToken ?? typeToken
                };
            }
            
            if (textToken is not StringToken || textToken.Text.Trim().Length == 0)
            {
                throw new TokenException("Expecting a non-empty string here.")
                {
                    Token = textToken ?? colonToken ?? typeToken
                };
            }

            expressionParser.SetMessageText(messageType, textToken.Text);

            if (OperatorToken.Comma.Matches(tokens.FirstOrDefault()) && tokens.Count > 1)
                tokens.RemoveFirst();
        }
    }
}
