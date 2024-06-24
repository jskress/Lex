using Lex.Clauses;
using Lex.Expressions;
using Lex.Parser;
using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This class represents a domain-specific language.  It is represented by a collection
/// of named clauses
/// </summary>
public class Dsl
{
    private const string TopLevelClauseParser = "$top-level-clause$";

    /// <summary>
    /// This property makes available the full set of keywords that were defined in the DSL's
    /// specification.
    /// </summary>
    public KeywordToken[] Keywords => _keywords.ToArray();

    /// <summary>
    /// This property makes available the full set of operators that were defined in the DSL's
    /// specification.
    /// </summary>
    public OperatorToken[] Operators => _operators.ToArray();

    /// <summary>
    /// This property holds the object that will be used to parse expressions for the
    /// language.
    /// </summary>
    public ExpressionParser ExpressionParser { get; internal set; }

    private readonly Dictionary<string, ClauseParser> _clauses = new ();
    private readonly List<KeywordToken> _keywords = [];
    private readonly List<OperatorToken> _operators = [];

    private string _lexicalParserSpec;

    internal Dsl(ClauseParser topLevelClauseParser = null)
    {
        if (topLevelClauseParser != null)
            SetTopLevelClause(topLevelClauseParser);
    }

    /// <summary>
    /// This method is used to set the lexical parser specification string for the DSL.
    /// </summary>
    /// <param name="parserSpec">The lexical parser specification to begin using.</param>
    internal void SetLexicalParserSpec(string parserSpec)
    {
        _lexicalParserSpec = parserSpec;
    }

    /// <summary>
    /// This method is used to add a keyword to the DSL.  These are added as the DSL specification
    /// is parsed.
    /// </summary>
    /// <param name="token">The keyword token to add (if we don't already know about it).</param>
    internal void AddKeyword(KeywordToken token)
    {
        if (!_keywords.Any(keyword => keyword.Matches(token)))
            _keywords.Add(token);
    }

    /// <summary>
    /// This method is used to add an operator to the DSL.  These are added as the DSL specification
    /// is parsed.
    /// </summary>
    /// <param name="token">The operator token to add (if we don't already know about it).</param>
    internal void AddOperator(OperatorToken token)
    {
        if (!_operators.Any(op => op.Matches(token)))
            _operators.Add(token);
    }

    /// <summary>
    /// This method is used to add a named clause to the DSL.
    /// </summary>
    /// <param name="name">The name for the new clause.</param>
    /// <param name="clauseParser">The clause parser to add.</param>
    internal void AddNamedClauseParser(string name, ClauseParser clauseParser)
    {
        _clauses[name] = clauseParser;
    }

    /// <summary>
    /// This method is used to set the top-level clause parser for the DSL.
    /// </summary>
    /// <param name="topLevelClauseParser">The top-level clause parser for the DSL.</param>
    internal void SetTopLevelClause(ClauseParser topLevelClauseParser)
    {
        if (_clauses.ContainsKey(TopLevelClauseParser))
            throw new ArgumentException("A top-level clause has already been set.");

        AddNamedClauseParser(TopLevelClauseParser, topLevelClauseParser);
    }

    /// <summary>
    /// This method is used to set the debugging state for the named clause parser.  Pass
    /// <c>null</c> for the name to affect the top-level clause parser.
    /// </summary>
    /// <param name="name">The name of the clause parser to change the debugging state for.</param>
    /// <param name="state">The new debugging state for that parser.</param>
    /// <param name="fullGraph">A flag noting whether just the named parser should be
    /// updated or it, along with all its descendents.</param>
    public void SetDebugging(string name, bool state, bool fullGraph)
    {
        if (_clauses.TryGetValue(name ?? TopLevelClauseParser, out ClauseParser parser))
        {
            if (fullGraph)
                parser.SetDebugging(state);
            else
                parser.IsDebugging = state;
        }
    }

    /// <summary>
    /// This method is used to set the debugging state for the named clause parser.  Pass
    /// <c>null</c> for the name to affect the top-level clause parser.
    /// </summary>
    /// <param name="name">The name of the clause parser to change the debugging state for.</param>
    /// <param name="consumer">The new debugging information consumer for that parser.</param>
    /// <param name="fullGraph">A flag noting whether just the named parser should be
    /// updated or it, along with all its descendents.</param>
    public void SetDebugConsumer(string name, Action<ClauseParserDebugInfo> consumer, bool fullGraph)
    {
        if (_clauses.TryGetValue(name ?? TopLevelClauseParser, out ClauseParser parser))
        {
            if (fullGraph)
                parser.SetDebugConsumer(consumer);
            else
                parser.DebugConsumer = consumer;
        }
    }

    /// <summary>
    /// This method is used to create an appropriate lexical parser for this DSL.  This
    /// requires that the specification for the DSL itself contain a <c>parserSpec:</c>
    /// clause set to the specifical expressed in the lexical parser DSL.
    /// </summary>
    /// <returns>The created and configured lexical parser.</returns>
    public LexicalParser CreateLexicalParser()
    {
        if (_lexicalParserSpec == null)
            throw new InvalidOperationException("The DSL does not contain a parser specification.");

        return LexicalParserFactory.CreateFrom(_lexicalParserSpec, this);
    }

    /// <summary>
    /// This method is used to parse the next clause from the given parser.  This method
    /// assumes the presence of a top-level clause.
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The next parsed claus or <c>null</c> if the top-level clause in the DSL
    /// was not matched.</returns>
    public Clause ParseNextClause(LexicalParser parser)
    {
        return ParseClause(parser, TopLevelClauseParser);
    }

    /// <summary>
    /// This method is used to parse a clause from the given parser.  The clause must have
    /// been previously registered with the specified tag.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    public Clause ParseClause(LexicalParser parser, string tag)
    {
        if (!_clauses.TryGetValue(tag, out ClauseParser clause))
        {
            throw tag == TopLevelClauseParser
                ? new ArgumentException("There is no top-level clause parser defined for this DSL.")
                : new ArgumentException($"There is no clause parser known by the tag, {tag}.", nameof(tag));
        }

        return clause.TryParse(parser);
    }

    /// <summary>
    /// This method is used to parse an expression from the given parser.  This method
    /// requires that the source DSL specification included an expression specification.
    /// </summary>
    /// <remarks>
    /// You must set a tree builder into the current expression parser before calling this
    /// method.
    /// </remarks>
    /// <param name="parser">The parser to use.</param>
    /// <returns>The parsed expression as an expression tree.</returns>
    public IExpressionTerm ParseExpression(LexicalParser parser)
    {
        return ExpressionParser.ParseExpression(parser);
    }
}
