namespace Lex.Tokenizers;

/// <summary>
/// This enum lists the available styles for an identifier.
/// </summary>
public enum LetterCaseStyle
{
    /// <summary>
    /// This entry indicates that identifiers should be forced to lower case.
    /// </summary>
    LowerCase,

    /// <summary>
    /// This entry indicates that identifiers should be forced to upper case.
    /// </summary>
    UpperCase,

    /// <summary>
    /// This entry indicates that identifiers should be left in the case discovered.
    /// </summary>
    AsIs
}

/// <summary>
/// This class provides needed extensions on our <c>IdStyle</c> enum.
/// </summary>
internal static class IdStyleExtensions
{
    /// <summary>
    /// This method is used to apply the given style to the specified text.
    /// </summary>
    /// <param name="style">The style to apply.</param>
    /// <param name="text">The text to apply it to.</param>
    /// <returns>The text with the style applied.</returns>
    internal static string Apply(this LetterCaseStyle style, string text)
    {
        if (text == null)
            return null;

        return style switch
        {
            LetterCaseStyle.LowerCase => text.ToLowerInvariant(),
            LetterCaseStyle.UpperCase => text.ToUpperInvariant(),
            LetterCaseStyle.AsIs => text,
            _ => throw new ArgumentOutOfRangeException(nameof(style))
        };
    }

    /// <summary>
    /// This method is used to apply the given style to the specified character.
    /// </summary>
    /// <param name="style">The style to apply.</param>
    /// <param name="ch">The character to apply it to.</param>
    /// <returns>The character with the style applied.</returns>
    internal static char Apply(this LetterCaseStyle style, char ch)
    {
        return style switch
        {
            LetterCaseStyle.LowerCase => char.ToLowerInvariant(ch),
            LetterCaseStyle.UpperCase => char.ToUpperInvariant(ch),
            LetterCaseStyle.AsIs => ch,
            _ => throw new ArgumentOutOfRangeException(nameof(style))
        };
    }
}
