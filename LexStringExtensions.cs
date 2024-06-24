using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]

namespace Lex;

/// <summary>
/// This class provides some useful string extensions relating to the lexical parser.
/// </summary>
public static class LexStringExtensions
{
    /// <summary>
    /// This method returns the string as a stream reader.
    /// </summary>
    /// <param name="text">The string whose contents will become the content of the resulting
    /// stream.</param>
    /// <returns>The given text as a stream reader.</returns>
    public static StreamReader AsReader(this string text)
    {
        MemoryStream memoryStream = new ();
        StreamWriter writer = new (memoryStream);

        writer.Write(text);
        writer.Flush();

        memoryStream.Position = 0;

        return new StreamReader(memoryStream);
    }
}
