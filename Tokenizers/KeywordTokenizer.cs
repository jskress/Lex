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
            _styledKeywords = null;
        }
    }

    private LetterCaseStyle _style;
    private HashSet<string>? _styledKeywords;
    private IdTokenizer? _idTokenizer;
    private bool _lookedForIdTokenizer;

    public KeywordTokenizer(LexicalParser parser, HashSet<string> keywords)
        : base(parser, keywords, "a keyword")
    {
        ValidateKeywords(keywords);

        _style = LetterCaseStyle.AsIs;
        _styledKeywords = null;
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

        _styledKeywords = null;
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

        _styledKeywords = null;
    }

    /// <summary>
    /// This method is used to inform the parser whether this tokenizer can start a token
    /// with the specified character.
    /// </summary>
    /// <remarks>
    /// A keyword is a word that happens to be reserved, so the whole word is read first and
    /// only then looked up in our list.  Matching our text against the stream directly would
    /// take the <c>flaw</c> out of <c>flaw2</c> and call it a keyword, leaving a stray
    /// <c>2</c> behind, when what is written there is one perfectly ordinary identifier.
    /// <para>
    /// Reading the whole word means reading past what we may end up wanting, so when the word
    /// turns out not to be a keyword, everything but the first character goes back and we
    /// decline the job.  The identifier tokenizer, which is what should have it, then sees
    /// the stream exactly as we found it.
    /// </para>
    /// </remarks>
    /// <param name="ch">The character to check (or begin checking with).</param>
    /// <returns><c>true</c> if this tokenizer accepts the job of parsing the next token
    /// or <c>false</c> if not.</returns>
    protected override bool CanStart(char ch)
    {
        if (!IsWordStarter(ch))
            return false;

        Builder.Append(ch);

        (int data, char next) = Read();

        while (data >= 0 && IsWordMember(next))
        {
            Builder.Append(next);

            (data, next) = Read();
        }

        ReturnChar(data);

        if (StyledKeywords.Contains(Style.Apply(Builder.ToString())))
            return true;

        // Not a keyword after all, so put the word back.  The first character stays consumed
        // because the parser read it before offering it to us, and hands that same character
        // to whichever tokenizer takes the job next.
        ReturnBuffer(1);

        Builder.Length = 0;

        return false;
    }

    /// <summary>
    /// This property provides our keywords with the current style applied, for looking up
    /// the words we read.
    /// </summary>
    private HashSet<string> StyledKeywords =>
        _styledKeywords ??= Possibilities
            .Select(possibility => Style.Apply(possibility))
            .ToHashSet();

    /// <summary>
    /// This property provides the identifier tokenizer registered with our parser, if there
    /// is one.  It is looked up lazily since it may well be registered after we are.
    /// </summary>
    private IdTokenizer? IdTokenizer
    {
        get
        {
            if (!_lookedForIdTokenizer)
            {
                _idTokenizer = Parser.GetTokenizer<IdTokenizer>();
                _lookedForIdTokenizer = true;
            }

            return _idTokenizer;
        }
    }

    /// <summary>
    /// This method reports whether the given character may begin a word.
    /// </summary>
    /// <remarks>
    /// We go by what the identifier tokenizer accepts, since it is the one that decides where
    /// a word ends.  Letters count regardless: keywords are all letters, so a language that
    /// narrows its identifiers to some smaller set of characters should still get its
    /// keywords.  With no identifier tokenizer registered, letters are all there is to go on,
    /// and rightly so -- nothing else in that language could make a longer word for a keyword
    /// to be mistaken for part of.
    /// </remarks>
    /// <param name="ch">The character to check.</param>
    /// <returns><c>true</c>, if the character may begin a word.</returns>
    private bool IsWordStarter(char ch)
    {
        return char.IsLetter(ch) || (IdTokenizer?.Starters.Contains(ch) ?? false);
    }

    /// <summary>
    /// This method reports whether the given character may continue a word.
    /// </summary>
    /// <param name="ch">The character to check.</param>
    /// <returns><c>true</c>, if the character may continue a word.</returns>
    private bool IsWordMember(char ch)
    {
        return char.IsLetter(ch) || (IdTokenizer?.Members.Contains(ch) ?? false);
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
