namespace Lex.Tokens;

public class Token
{
    /// <summary>
    /// This field holds a mapping of token types to a "noun" string to use for each.
    /// </summary>
    private static readonly Dictionary<RuntimeTypeHandle, string> TokenTypeDescriptions = new ()
    {
        { typeof(BounderToken).TypeHandle, "a bounder" },
        { typeof(CommentToken).TypeHandle, "a comment" },
        { typeof(IdToken).TypeHandle, "an identifier" },
        { typeof(KeywordToken).TypeHandle, "a keyword" },
        { typeof(NumberToken).TypeHandle, "a number" },
        { typeof(OperatorToken).TypeHandle, "an operator" },
        { typeof(StringToken).TypeHandle, "a string" },
        { typeof(WhitespaceToken).TypeHandle, "whitespace" }
    };

    /// <summary>
    /// This method is used to describe the given type, which must be a token type.  The
    /// description is, effectively, a "noun" for the type, suitable for including in error
    /// messages.
    /// </summary>
    /// <param name="type">The type to describe.</param>
    /// <returns>A string describing the type.</returns>
    public static string Describe(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (!typeof(Token).IsAssignableFrom(type))
            throw new ArgumentException("Cannot describe types that are not token types.");

        if (TokenTypeDescriptions.TryGetValue(type.TypeHandle, out string noun))
            return noun;

        string name = type.Name.ToLowerInvariant();
        // ReSharper disable once StringLiteralTypo
        string prefix = "aeiou".Contains(name[0]) ? "an" : "a";

        if (name.EndsWith("token"))
            name = name[..^5];

        return $"{prefix} {name}";
    }

    /// <summary>
    /// This method is used to describe the given token.  The description is meant to be
    /// suitable for inclusion in error messages.
    /// </summary>
    /// <param name="token">The token to describe.</param>
    /// <returns>The token's description.</returns>
    public static string Describe(Token token)
    {
        ArgumentNullException.ThrowIfNull(token, nameof(token));

        return $"{Describe(token.GetType())} of \"{token.Text}\"";
    }

    /// <summary>
    /// This property holds the text parsed from the source that this token encapsulates.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// This property holds the line in the source that the first character of the token
    /// was found on.
    /// </summary>
    public int Line { get; internal set; }

    /// <summary>
    /// This property holds the column in the source that the first character of the token
    /// was found on.
    /// </summary>
    public int Column { get; internal set; }

    protected Token(string text)
    {
        Text = text;
    }

    /// <summary>
    /// This method returns whether this token is the same as the one specified.  This will
    /// be true only if the other object is of the same token type as this one and their
    /// text is the same.
    /// </summary>
    /// <param name="theOther">The other token to test for equality with.</param>
    /// <returns><c>true</c> if the given token matches this one or <c>false</c> if not.</returns>
    public bool Matches(Token theOther)
    {
        if (theOther == null)
            return false;

        return GetType() == theOther.GetType() &&
               Text.Equals(theOther.Text);
    }

    /// <summary>
    /// This is our string representation.
    /// </summary>
    /// <returns>Our string representation.</returns>
    public override string ToString()
    {
        string typeName = GetType().Name;
        string suffix = Line == 0 && Column == 0
            ? ""
            : $" at ({Line}, {Column})";

        if (typeName.EndsWith("Token"))
            typeName = typeName[..^5];

        return $"{typeName}('{Text}'){suffix}";
    }
}
