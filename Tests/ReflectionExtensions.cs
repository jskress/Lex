using System.Reflection;
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
}
