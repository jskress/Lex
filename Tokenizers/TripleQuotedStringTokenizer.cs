using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating triple-quoted, multi-line string literals.
/// </summary>
public class TripleQuotedStringTokenizer : StringTokenizer
{
    private const char Quote = '\"';
    private const string TwoQuotes = "\"\"";
    private const string ThreeQuotes = "\"\"\"";

    /// <summary>
    /// This property indicates whether the triple-quote bounder can be longer, ala C# or
    /// just the three quotes, ala Java and Python.
    /// </summary>
    public bool MarkerCanExtend { get; set; }

    public TripleQuotedStringTokenizer(LexicalParser parser) : base(parser, ThreeQuotes)
    {
        IsMultiLine = true;
        MarkerCanExtend = true;
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        if (MarkerCanExtend)
        {
            int data;

            // Pull off the other two starting quotes.
            Read();
            Read();

            Builder.Append(TwoQuotes);

            while (((data, _) = Read()).data == Quote)
                Builder.Append(Quote);

            ReturnChar(data);

            Bounder = Builder.ToString() + Quote;

            ReturnBuffer();
        }
        else
            Bounder = ThreeQuotes;

        return base.ParseToken(ch);
    }

    /// <summary>
    /// This method is used to obtain the string literal the token represents.  By default,
    /// the current content of the text buffer is returned.  Some subclasses may want to
    /// post-process the text before it gets put into a token.
    /// </summary>
    /// <returns>The literal text to put in the token.</returns>
    protected override string GetText()
    {
        List<string> lines = base
            .GetText()
            .TrimStart()
            .Split('\n')
            .ToList();

        if (lines.Count > 1)
        {
            string last = lines.Last();
            string trimmed = last.TrimStart();
            int indent = last.Length - trimmed.Length;

            lines = lines
                .Select(line => TrimIndent(line, indent))
                .ToList();
        }

        return string.Join('\n', lines);
    }

    /// <summary>
    /// This method handles trimming the indent from the given line.
    /// </summary>
    /// <param name="line">The line to trim the indent from.</param>
    /// <param name="indent">The size of the indent to trim.</param>
    /// <returns>The trimmed line.</returns>
    private static string TrimIndent(string line, int indent)
    {
        if (line.Length < indent)
            return line.TrimStart();

        for (int index = 0; index < indent; index++)
        {
            if (!char.IsWhiteSpace(line[index]))
            {
                indent = index;
                break;
            }
        }

        return line[indent..];
    }
}
