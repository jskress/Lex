using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating keywords.
/// </summary>
public class KeywordTokenizer : FixedListTokenizer
{
    /// <summary>
    /// This is a helper method for validating that the given collection of strings has
    /// no entries that cannot be keywords.
    /// </summary>
    /// <param name="keywords">The keywords to validate.</param>
    private static void ValidateKeywords(HashSet<string> keywords)
    {
        if (keywords == null || keywords.Count == 0)
            throw new ArgumentException("At least one keyword must be provided.");

        foreach (string keyword in keywords
                     .Where(keyword => !keyword.All(char.IsLetter)))
        {
            throw new ArgumentException($"Keyword '{keyword}' contains at least one non-letter character.", nameof(keywords));
        }
    }

    /// <summary>
    /// This holds the casing style to use when matching text and creating tokens.
    /// </summary>
    public LetterCaseStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            _styledPossibilities = null;
        }
    }

    private LetterCaseStyle _style;
    private string[] _styledPossibilities;

    public KeywordTokenizer(LexicalParser parser, HashSet<string> keywords)
        : base(parser, keywords, "a keyword")
    {
        ValidateKeywords(keywords);

        _style = LetterCaseStyle.AsIs;
        _styledPossibilities = null;
    }

    public KeywordTokenizer(LexicalParser parser, params string[] keywords)
        : this(parser, keywords.ToHashSet()) {}

    public KeywordTokenizer(LexicalParser parser, params KeywordToken[] keywords)
        : this(parser, keywords.Select(kw => kw.Text).ToHashSet()) {}

    /// <summary>
    /// This method allows for adding additional keyword possibilities after construction.
    /// </summary>
    /// <param name="additionalKeywords">The additional keywords we want to support.</param>
    public void Including(IEnumerable<string> additionalKeywords)
    {
        HashSet<string> strings = additionalKeywords.ToHashSet();

        ValidateKeywords(strings);
        Including(strings, "a keyword");

        _styledPossibilities = null;
    }

    /// <summary>
    /// This method allows for the removal of unwanted keyword possibilities after
    /// construction.
    /// </summary>
    /// <param name="unwantedKeywords">The keywords provided at construction that we don't want.</param>
    public void Excluding(IEnumerable<string> unwantedKeywords)
    {
        HashSet<string> strings = unwantedKeywords.ToHashSet();

        ValidateKeywords(strings);
        Excluding(strings, "a keyword");

        _styledPossibilities = null;
    }

    /// <summary>
    /// This method is used to inform the parser whether this tokenizer can start a token
    /// with the specified character.
    /// </summary>
    /// <param name="ch">The character to check (or begin checking with).</param>
    /// <returns><c>true</c> if this tokenizer accepts the job of parsing the next token
    /// or <c>false</c> if not.</returns>
    internal override bool CanStart(char ch)
    {
        return base.CanStart(_style.Apply(ch));
    }

    /// <summary>
    /// This method is used to acquire the (potentially modified) first character of the
    /// given string.
    /// </summary>
    /// <param name="possibilities">The possibilities to filter.</param>
    /// <param name="ch">The starting character to look for.</param>
    /// <returns>The possibilities, filtered to those that start with the given character.</returns>
    protected override string[] GetStartsWith(string[] possibilities, char ch)
    {
        _styledPossibilities ??= _style == LetterCaseStyle.AsIs
            ? possibilities
            : possibilities
                .Select(possibility => Style.Apply(possibility))
                .ToArray();

        return _styledPossibilities
            .Where(possibility => possibility[0] == ch)
            .ToArray();
    }

    /// <summary>
    /// This method is used to read in text up to a specific count to see what's next in
    /// the character stream.  Once read, the current style is applied to the text.
    /// </summary>
    /// <param name="count">The length of the text to read.</param>
    /// <returns>The text of the appropriate length or <c>null</c>, if there are not enough
    /// characters available.</returns>
    protected override string ReadToBuffer(int count)
    {
        return Style.Apply(base.ReadToBuffer(count));
    }

    /// <summary>
    /// We use this hook to indicate that we should keep reading characters, regardless of
    /// the buffer length.
    /// </summary>
    /// <param name="count">The number of characters of the possibility we are about to
    /// match.</param>
    /// <returns><c>true</c>, always.</returns>
    protected override bool NeedMoreCharacters(int count)
    {
        return true;
    }

    /// <summary>
    /// We use this hook to tell the parent class to keep reading so long as the character
    /// that was read is a letter.
    /// </summary>
    /// <param name="data">The character as an <c>int</c>.</param>
    /// <param name="ch">The character as a character.</param>
    /// <returns><c>true</c>, if the character should be included, or <c>false</c>, if not.</returns>
    protected override bool ShouldInclude(int data, char ch)
    {
        return char.IsLetter(ch);
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        return new KeywordToken(Style.Apply(Builder.ToString()));
    }
}
