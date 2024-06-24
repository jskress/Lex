using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This class provides some useful extension methods for lists.
/// </summary>
internal static class ListExtensions
{
    /// <summary>
    /// This is a helper method for obtaining the text from the first token in a token list.
    /// If the requested token does not exist, then <c>null</c> will be returned.
    /// </summary>
    /// <param name="tokens">The list of tokens to pull from.</param>
    /// <param name="index">The index of the token to get the text for.</param>
    /// <returns>The text of the indicated token, or <c>null</c>.</returns>
    internal static string GetTokenText(this List<Token> tokens, int index = 0)
    {
        return index < tokens.Count ? tokens[index].Text : null;
    }

    /// <summary>
    /// This method is used to remove the first element in a list, making it available.
    /// If the list is <c>null</c> or empty, then <c>null</c> is returned.
    /// </summary>
    /// <param name="list">The list to remove the element from.</param>
    /// <returns>The element from the list.</returns>
    internal static T RemoveFirst<T>(this List<T> list)
        where T : class
    {
        if (list == null || list.Count == 0)
            return null;

        T result = list[0];

        list.RemoveAt(0);

        return result;
    }

    /// <summary>
    /// This is a helper method for debugging.
    /// </summary>
    /// <param name="list">The list to dump the contents of.</param>
    internal static void Dump<T>(this List<T> list)
    {
        Console.Write('[');
        Console.Write(string.Join(", ", list));
        Console.WriteLine(']');
    }
}
