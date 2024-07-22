using Lex.Clauses;
using Lex.Parser;
using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This record carries the information we need while we are parsing source DSL.
/// </summary>
/// <param name="Stored">The clause parser that is stored; i.e., the "official" one.</param>
/// <param name="Parser">The claude parser that needs to be populated later.</param>
/// <param name="Tokens">The list of tokens that define the clause</param>
/// <param name="Debug">The debug flag to set once the clause has been populated.</param>
internal record ClauseParserSource<TParser>(
    ClauseParser Stored, TParser Parser, List<Token> Tokens, bool Debug) 
    where TParser : ClauseParser;

/// <summary>
/// This class is a carrier for all the things we care about while parsing a DSL
/// specification.
/// </summary>
internal class DslParsingContext
{
    /// <summary>
    /// This property holds the parser that is being used to parse the DSL specification.
    /// </summary>
    internal LexicalParser Parser { get; init; }

    /// <summary>
    /// This property holds the set of variables we are using.
    /// </summary>
    internal Dictionary<string, object> Variables { get; init; }

    /// <summary>
    /// This property holds the list of tokens that make up the clause to process.
    /// </summary>
    internal List<Token> Tokens { get; set; }

    /// <summary>
    /// The DSL we are building up.
    /// </summary>
    internal Dsl Dsl { get; init; }

    /// <summary>
    /// This property holds a list of deferred token lists to use in populating sequential
    /// clauses.
    /// </summary>
    internal List<ClauseParserSource<SequentialClauseParser>> SequentialClauseSources { get; } = [];

    /// <summary>
    /// This property holds a list of deferred token lists to use in populating switch
    /// clauses.
    /// </summary>
    internal List<ClauseParserSource<SwitchClauseParser>> SwitchClauseSources { get; } = [];
}
