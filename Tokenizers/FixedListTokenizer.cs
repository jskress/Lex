
using Lex.Parser;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides a base class for tokenizers that isolate tokens whose text comes
/// from a fixed list of possibilities.
/// </summary>
public abstract class FixedListTokenizer : Tokenizer
{
    /// <summary>
    /// This is a helper method for normalizing a list of strings to guarantee it does not
    /// contain <c>null</c> or empty values and that none of them contain whitespace.
    /// </summary>
    /// <param name="values">The list of values to normalize.</param>
    /// <param name="noun">The noun to use for errors.</param>
    /// <returns></returns>
    private static string[] Normalize(HashSet<string> values, string noun)
    {
        values = values.Select(value => Normalize(value, noun)).ToHashSet();

        if (values.Any(value => value == string.Empty))
            throw new ArgumentException($"Empty values are not allowed for {noun}.");

        return values
            .OrderByDescending(possibility => possibility.Length)
            .ToArray();
    }

    private string[] _possibilities;

    protected FixedListTokenizer(LexicalParser parser, HashSet<string> possibilities, string noun)
        : base(parser)
    {
        _possibilities = Normalize(possibilities, noun);
    }

    /// <summary>
    /// This method allows subclasses to add additional possibilities after construction.
    /// </summary>
    /// <param name="additionalValues">The additional values we want to allow.</param>
    /// <param name="noun">The noun to use for errors.</param>
    protected void Including(IEnumerable<string> additionalValues, string noun)
    {
        HashSet<string> values = [.._possibilities];

        values.UnionWith(additionalValues);

        _possibilities = Normalize(values, noun);
    }

    /// <summary>
    /// This method allows subclasses to remove unwanted possibilities after construction.
    /// </summary>
    /// <param name="unwantedValues">The values provided at construction that we don't want.</param>
    /// <param name="noun">The noun to use for errors.</param>
    protected void Excluding(IEnumerable<string> unwantedValues, string noun)
    {
        HashSet<string> values = [.._possibilities];

        values.ExceptWith(unwantedValues);

        _possibilities = Normalize(values, noun);
    }

    /// <summary>
    /// This method is used to inform the parser whether this tokenizer can start a token
    /// with the specified character.  Because of our nature, we actually consume all the
    /// appropriate characters here and just make it available to feed into creating the
    /// appropriate token.
    /// </summary>
    /// <param name="ch">The character to check (or begin checking with).</param>
    /// <returns><c>true</c> if this tokenizer accepts the job of parsing the next token
    /// or <c>false</c> if not.</returns>
    internal override bool CanStart(char ch)
    {
        return FindMatch(ch) != null;
    }

    /// <summary>
    /// This method reads all text that represents the token being parsed.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed text or <c>null</c>.</returns>
    private string FindMatch(char ch)
    {
        string[] matching = GetStartsWith(_possibilities, ch);

        if (matching.Length == 0)
            return null;

        Builder.Append(ch);

        foreach (string possibility in matching)
        {
            string text = ReadToBuffer(possibility.Length);

            if (possibility == text)
                return text;

            ReturnBuffer(1);
        }

        Builder.Length = 0;

        return null;
    }

    /// <summary>
    /// This method is used to acquire the (potentially modified) first character of the
    /// given string.
    /// </summary>
    /// <param name="possibilities">The possibilities to filter.</param>
    /// <param name="ch">The starting character to look for.</param>
    /// <returns>The possibilities, filtered to those that start with the given character.</returns>
    protected virtual string[] GetStartsWith(string[] possibilities, char ch)
    {
        return possibilities
            .Where(possibility => possibility[0] == ch)
            .ToArray();
    }

    /// <summary>
    /// This method is used to read in text up to a specific count to see what's next in
    /// the character stream.
    /// </summary>
    /// <param name="count">The length of the text to read.</param>
    /// <returns>The text of the appropriate length or <c>null</c>, if there are not enough
    /// characters available.</returns>
    protected virtual string ReadToBuffer(int count)
    {
        while (NeedMoreCharacters(count))
        {
            (int data, char ch) = Read();

            if (ShouldInclude(data, ch))
                Builder.Append(ch);
            else
            {
                ReturnChar(data);

                break;
            }
        }

        return Builder.ToString();
    }

    /// <summary>
    /// This is a hook that allows a subclass to decide whether number of characters should
    /// matter in what we read.  By default, we note that we need more characters until our
    /// buffer is the same length as what we're about to compare against.
    /// </summary>
    /// <param name="count">The number of characters of the possibility we are about to
    /// match.</param>
    /// <returns><c>true</c>, if more characters should be read, or <c>false</c>, if not.</returns>
    protected virtual bool NeedMoreCharacters(int count)
    {
        return Builder.Length < count;
    }

    /// <summary>
    /// This is a hook to allow a subclass to decide, based on the character just read,
    /// whether that character should be included in the text we are reading.  By default,
    /// we include any character.
    /// </summary>
    /// <param name="data">The character as an <c>int</c>.</param>
    /// <param name="ch">The character as a character.</param>
    /// <returns><c>true</c>, if the character should be included, or <c>false</c>, if not.</returns>
    protected virtual bool ShouldInclude(int data, char ch)
    {
        return data >= 0;
    }
}
