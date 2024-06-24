using Lex.Parser;
using Lex.Tokens;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating comments.
/// </summary>
public class CommentTokenizer : Tokenizer
{
    private readonly List<string> _startMarkers;
    private readonly List<string> _endMarkers;

    private string _terminator;

    public CommentTokenizer(LexicalParser parser) : base(parser)
    {
        _startMarkers = [];
        _endMarkers = [];
        _terminator = null;

        ReportTokens = false;
    }

    /// <summary>
    /// This method adds the (fairly) standard comment markers for the <c>/*</c> and
    /// <c>*/</c> block comment type and the <c>//</c> line comment.
    /// </summary>
    /// <returns>This object, for fluency.</returns>
    public CommentTokenizer AddStandardMarkers()
    {
        AddMarkers("/*", "*/");
        AddLineCommentMarker("//");

        return this;
    }

    /// <summary>
    /// This method is used to add comment markers for that have a starting mark and go
    /// to the end of the line.
    /// </summary>
    /// <param name="start">The starting marker.</param>
    /// <returns>This object, for fluency.</returns>
    public CommentTokenizer AddLineCommentMarker(string start)
    {
        return AddMarkers(start, "\n");
    }

    /// <summary>
    /// This method is used to add the bounders for a comment.
    /// </summary>
    /// <param name="start">The starting marker.</param>
    /// <param name="end">The ending marker.</param>
    /// <returns>This object, for fluency.</returns>
    public CommentTokenizer AddMarkers(string start, string end)
    {
        start = Normalize(start, "a starting comment marker");

        if (end != "\n")
            end = Normalize(end, "an ending comment marker");

        if (start == string.Empty)
            throw new ArgumentException("Start comment marker cannot be empty.");

        if (end == string.Empty)
            throw new ArgumentException("End comment marker cannot be empty.");

        _startMarkers.Add(start);
        _endMarkers.Add(end);

        return this;
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
        for (int index = 0; index < _startMarkers.Count; index++)
        {
            if (IsNext(ch, _startMarkers[index]))
            {
                Builder.Append(_startMarkers[index]);

                Skip(_startMarkers[index].Length - 1);

                _terminator = _endMarkers[index];

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// This method should read all text that represents the token being parsed and wrap
    /// it in a token.
    /// </summary>
    /// <param name="ch">The first character that belongs to the token.</param>
    /// <returns>The parsed token.</returns>
    protected override Token ParseToken(char ch)
    {
        (int data, ch) = Read();

        while (data >= 0 && !IsNext(ch, _terminator))
        {
            Builder.Append(ch);

            (data, ch) = Read();
        }

        Builder.Append(_terminator);
        Skip(_terminator.Length - 1);

        return new CommentToken(Builder.ToString());
    }
}
