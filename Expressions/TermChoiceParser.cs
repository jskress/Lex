using Lex.Parser;
using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class handles parsing one particular style of expression term.
/// </summary>
public class TermChoiceParser
{
    /// <summary>
    /// This property holds the tag to report when this term choice is selected.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// This property reports whether the term choice parser is empty.
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    private readonly List<TermChoiceItem> _items = [];

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="token">The token to check for.</param>
    /// <param name="suppress">A flag that controls whether the token will be included in
    /// the list given to the term creator.</param>
    /// <returns>This object, for fluency.</returns>
    public TermChoiceParser Matching(Token token, bool suppress = false)
    {
        return AddItem(
            count => count > 0, "The term choice has already been started.",
            () => new TokenTermChoiceItem
            {
                Token = token,
                Suppress = suppress
            });
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="type">The token type to check for.</param>
    /// <returns>This object, for fluency.</returns>
    public TermChoiceParser Matching(Type type)
    {
        return AddItem(
            count => count > 0, "The term choice has already been started.",
            () => new TypeTermChoiceItem
            {
                Type = type
            });
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="token">The token to check for.</param>
    /// <param name="suppress">A flag that controls whether the token will be included in
    /// the list given to the term creator.</param>
    /// <returns>This object, for fluency.</returns>
    public TermChoiceParser Then(Token token, bool suppress = false)
    {
        return AddItem(
            count => count == 0, "The term choice has not been started.",
            () => new TokenTermChoiceItem
            {
                Token = token,
                Suppress = suppress
            });
    }

    /// <summary>
    /// This method is used to add a single token clause for matching against a set of
    /// tokens to this sequential one.  Call this to add the 1st sub-clause.
    /// </summary>
    /// <param name="type">The token type to check for.</param>
    /// <returns>This object, for fluency.</returns>
    public TermChoiceParser Then(Type type)
    {
        return AddItem(
            count => count == 0, "The term choice has not been started.",
            () => new TypeTermChoiceItem
            {
                Type = type
            });
    }

    /// <summary>
    /// This method is used to add the expectation of one or more expressions at the
    /// current point in the term.
    /// </summary>
    /// <param name="item">The expression item to add.</param>
    /// <returns>This object, for fluency.</returns>
    public TermChoiceParser ExpectingExpression(ExpressionTermChoiceItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        _items.Add(item);

        return this;
    }

    /// <summary>
    /// This is a helper method for adding a new entry to our list.
    /// </summary>
    /// <param name="gate">A test to see if adding is ok.</param>
    /// <param name="gateMessage">The message to use for the exception if adding is not good.</param>
    /// <param name="creator">A lambda for creating the entry.</param>
    /// <returns>This object, for fluency.</returns>
    private TermChoiceParser AddItem(Func<int, bool> gate, string gateMessage, Func<TermChoiceItem> creator)
    {
        if (gate.Invoke(_items.Count))
            throw new Exception(gateMessage);

        _items.Add(creator.Invoke());

        return this;
    }

    /// <summary>
    /// This method is used to set the tag for the choice parser.  This is a simple wrapper
    /// around setting the <see cref="Tag"/> property so that it can be part of a method
    /// chain.
    /// </summary>
    /// <param name="tag">The tag to report for this choice.</param>
    /// <returns>This object, for fluency.</returns>
    public TermChoiceParser WithTag(string tag)
    {
        if (tag == null || tag.Trim().Length == 0)
            throw new ArgumentException("The tag must be a non-empty string.");

        Tag = tag;

        return this;
    }

    /// <summary>
    /// This method is used to see if this term choice parser can accept what follows in
    /// the given lexical parser's token stream.  If so, an appropriate term is created
    /// for it.  Otherwise, <c>null</c>, is returned.
    /// </summary>
    /// <param name="expressionParser">The controlling expression parser.</param>
    /// <param name="parser">The parser to read from.</param>
    /// <returns>An appropriate term or <c>null</c>.</returns>
    internal IExpressionTerm TryParse(ExpressionParser expressionParser, LexicalParser parser)
    {
        if (_items.Count == 0)
            throw new Exception("Term choice parser has no items.");

        List<Token> tokens = [];
        List<IExpressionTerm> terms = [];
        ExpressionParseMessageTypes messageType = ExpressionParseMessageTypes.None;
        string errorArgument = null;
        bool expressionSeen = false;

        foreach (TermChoiceItem item in _items)
        {
            Token token = parser.PeekNextToken();

            switch (item)
            {
                case TokenTermChoiceItem tokenItem:
                    if (parser.IsNext(tokenItem.Token))
                    {
                        token = parser.GetNextToken();

                        if (!tokenItem.Suppress)
                            tokens.Add(token);
                    }
                    else
                    {
                        messageType = ExpressionParseMessageTypes.MissingRequiredToken;
                        errorArgument = Token.Describe(tokenItem.Token);
                    }
                    break;
                case TypeTermChoiceItem typeItem:
                    if (parser.IsNextOfType(typeItem.Type))
                        tokens.Add(parser.GetNextToken());
                    else
                    {
                        messageType = ExpressionParseMessageTypes.TokenTypeMismatch;
                        errorArgument = Token.Describe(typeItem.Type);
                    }
                    break;
                case ExpressionTermChoiceItem expressionItem:
                    ParseChildExpressions(expressionParser, parser, expressionItem, terms);

                    expressionSeen = true;
                    break;
                default:
                    throw new Exception($"Programming error; missing a case for item of type {item.GetType().FullName}");
            }

            if (messageType != ExpressionParseMessageTypes.None)
            {
                if (expressionSeen)
                {
                    throw new TokenException(expressionParser.ResolveMessage(messageType, errorArgument))
                    {
                        Token = token
                    };
                }

                parser.ReturnTokens(tokens);

                return null;
            }
        }

        return expressionParser.TreeBuilder.CreateTerm(tokens, terms, Tag);
    }

    /// <summary>
    /// This is a helper method for parsing some number of expressions as controlled by the
    /// given expression term choice item.
    /// </summary>
    /// <param name="expressionParser">The controlling expression parser.</param>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="item">The controlling item.</param>
    /// <param name="terms">The terms list to populate.</param>
    private static void ParseChildExpressions(
        ExpressionParser expressionParser, LexicalParser parser,
        ExpressionTermChoiceItem item, List<IExpressionTerm> terms)
    {
        IExpressionTerm term = expressionParser.ParseExpressionTerm(parser);

        if (term != null)
        {
            terms.Add(term);

            if (item.Maximum > 1)
            {
                while (terms.Count < item.Maximum)
                {
                    if (item.Separators != null && item.Separators.TryParse(parser) == null)
                        break;

                    term = expressionParser.ParseExpressionTerm(parser);

                    if (term == null)
                        break;

                    terms.Add(term);
                }
            }
        }

        if (terms.Count < item.Minimum)
        {
            throw new TokenException(expressionParser.ResolveMessage(ExpressionParseMessageTypes.NotEnoughExpressions))
            {
                Token = parser.PeekNextToken()
            };
        }
    }
}
