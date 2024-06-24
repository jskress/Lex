using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Lex.Parser;

/// <summary>
/// This class is used as a character reader.  It handles tracking of the current line and
/// column and allows for characters to be "un-read"; i.e., returned to the stream.  When
/// you put characters back into the stream, be sure to only return characters you got from
/// the stream and return them in reverse order.  Not doing this will corrupt the character
/// stream.
/// </summary>
/// <remarks>Note that this reader automatically converts any line end sequence to just a
/// new-line.  As such, only new-lines should be returned to the stream; returning a
/// carriage return will have no effect.  This is done to make line/column number management
/// when returning characters simpler, especially since the occasions where the type of
/// line end matters are so rare as to be virtually non-existent.
/// </remarks>
internal class RewindingStreamReader : IDisposable
{
    /// <summary>
    /// This property makes available the number of the line being read.  This is zero until
    /// the first character is read.  It is 1-based thereafter.  It is incremented at the
    /// point the first character of a stream is read and when each line end is read.
    /// </summary>
    internal int Line { get; private set; }

    /// <summary>
    /// This property makes available the number of the column just read.  This number is
    /// 1-based but will report a zero after a line end has been read but before the first
    /// character of the nex line is read.
    /// </summary>
    internal int Column { get; private set; }

    private StreamReader _source;
    private LinkedList<char> _returnedCharacters;
    private LinkedList<int> _lineEndColumns;

    private bool _disposed;

    internal RewindingStreamReader(StreamReader source)
    {
        _source = source;
        _returnedCharacters = new LinkedList<char>();
        _lineEndColumns = new LinkedList<int>();
        _disposed = false;

        Line = 0;
        Column = 0;
    }

    /// <summary>
    /// This method returns the next character from the stream.  If there are no more
    /// characters, then <c>-1</c> is returned.
    /// </summary>
    /// <returns>The next character from the stream or <c>-1</c>, if the stream is at end.</returns>
    internal int Read()
    {
        int ch;

        if (_returnedCharacters.Count > 0)
        {
            ch = _returnedCharacters.Last();
            _returnedCharacters.RemoveLast();
        }
        else
        {
            ch = _source.Read();

            if (ch == '\r')
            {
                int next = _source.Read();

                if (next != '\n' && next >= 0)
                    _returnedCharacters.AddLast((char) next);

                ch = '\n';
            }
        }

        if (ch >= 0)
            UpdatePosition((char) ch);

        return ch;
    }

    /// <summary>
    /// This method is used to update line and column based on the given character having
    /// just been read.
    /// </summary>
    /// <param name="ch">The character that was just read.</param>
    private void UpdatePosition(char ch)
    {
        if (ch == '\n')
        {
            _lineEndColumns.AddLast(Column);

            Line++;
            Column = 0;
        }
        else
        {
            Column++;

            // Special case: the beginning of the stream.
            if (Line == 0 && Column == 1)
                Line++;
        }
    }

    /// <summary>
    /// This method is used to return a character to the stream.  Keep in mind that characters
    /// should be returned in reverse order from which they were read from the stream to
    /// maintain the original content.
    /// </summary>
    /// <remarks>
    /// Note that the carriage return character cannot be returned.  If it is, it will be
    /// quietly ignored.
    /// </remarks>
    /// <param name="ch">The character to return.</param>
    internal void ReturnChar(char ch)
    {
        if (ch >= 0 && ch != '\r')
        {
            _returnedCharacters.AddLast(ch);

            if (ch == '\n')
            {
                Line--;
                Column = _lineEndColumns.Last();

                _lineEndColumns.RemoveLast();
            }
            else
                Column--;
        }
    }

    /// <summary>
    /// This method handles disposing of the reader.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            _source.Dispose();
            _returnedCharacters.Clear();
            _lineEndColumns.Clear();

            _source = null;
            _returnedCharacters = null;
            _lineEndColumns = null;
        }
    }
}
