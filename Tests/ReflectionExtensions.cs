using System.Reflection;
using Lex.Clauses;
using Lex.Dsl;
using Lex.Parser;
using Lex.Tokenizers;

namespace Tests;

/// <summary>
/// This class provides some extra functionality for various lex objects relating to
/// reflection to support testing.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// This is a helper method for returning a reference to a parser's underlying list
    /// of tokenizers.
    /// </summary>
    /// <param name="parser">The lexical parser to get the list of tokenizers from.</param>
    /// <returns>The parser's list of tokenizers.</returns>
    public static List<Tokenizer> GetTokenizers(this LexicalParser parser)
    {
        FieldInfo fieldInfo = parser.GetType().GetField(
            "_tokenizers", BindingFlags.NonPublic | BindingFlags.Instance);

        return (List<Tokenizer>) fieldInfo?.GetValue(parser);
    }

    /// <summary>
    /// This is a helper method for returning a reference to a DSL's clause dictionary.
    /// </summary>
    /// <param name="dsl">The DSL to get the clause dictionary from.</param>
    /// <returns>The DSL's clause dictionary.</returns>
    public static Dictionary<string, ClauseParser> GetClauses(this Dsl dsl)
    {
        FieldInfo fieldInfo = dsl.GetType().GetField(
            "_clauses", BindingFlags.NonPublic | BindingFlags.Instance);

        return (Dictionary<string, ClauseParser>) fieldInfo?.GetValue(dsl);
    }

    /// <summary>
    /// This is a helper method for returning the nim and max values for a repeating clause
    /// parser.
    /// </summary>
    /// <param name="clauseParser">The clause parser to get the min/max values from.</param>
    /// <returns>A tuple containing the min and max values we found.</returns>
    public static (int, int?) GetMinMax(this RepeatingClauseParser clauseParser)
    {
        FieldInfo minFieldInfo = clauseParser.GetType().GetField(
            "_min", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo maxFieldInfo = clauseParser.GetType().GetField(
            "_max", BindingFlags.NonPublic | BindingFlags.Instance);

        return ((int) minFieldInfo!.GetValue(clauseParser)!,
                (int?) maxFieldInfo?.GetValue(clauseParser));
    }
}
