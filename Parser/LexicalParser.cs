using System.Diagnostics.CodeAnalysis;
using System.Text;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Lex.Parser;

/// <summary>
/// This class provides the means by which a stream of characters may be parsed into a
/// stream of language tokens.
/// </summary>
public sealed class LexicalParser : IDisposable
{
    /// <summary>
    /// This property holds the line of the last position this parser looked at in its
    /// source.  This is primarily useful for diagnostics when there's no token available
    /// to report a position from (e.g., at the end of input).
    /// </summary>
    public int Line => _line;

    /// <summary>
    /// This property holds the column of the last position this parser looked at in its
    /// source.  This is primarily useful for diagnostics when there's no token available
    /// to report a position from (e.g., at the end of input).
    /// </summary>
    public int Column => _column;

    /// <summary>
    /// This property, if set, notes the failure tracker that clause parsers should report
    /// their unannotated match failures to.  See <see cref="FailureTracker"/> for details.
    /// By default, this is <c>null</c>, meaning no tracking is done.
    /// </summary>
    public FailureTracker? FailureTracker { get; set; }

    private readonly List<Tokenizer> _tokenizers = [];
    private readonly LinkedList<Token> _returnedTokens = [];
    private readonly List<Token> _consumedSinceMark = [];
    private readonly Stack<int> _marks = new ();

    private RewindingStreamReader? _source;
    private int _line;
    private int _column;
    private bool _disposed;

    /// <summary>
    /// This method is used to register the next tokenizer for the parser.
    /// </summary>
    /// <param name="tokenizer">The next tokenizer to register.</param>
    internal void Register(Tokenizer tokenizer)
    {
        EnsureNotDisposed();

        _tokenizers.Add(tokenizer);
    }

    /// <summary>
    /// This method is used to retrieve a reference to the tokenizer of the given type.
    /// If there is no tokenizer of that type, then <c>null</c> will be returned.
    /// </summary>
    /// <typeparam name="T">The type of the desired tokenizer.</typeparam>
    /// <returns>The tokenizer of the requested type or <c>null</c>.</returns>
    public T? GetTokenizer<T>()
        where T : Tokenizer
    {
        return (T?) _tokenizers.FirstOrDefault(tokenizer => tokenizer is T);
    }

    /// <summary>
    /// This method is used to set the source reader for parsing.
    /// </summary>
    /// <param name="reader">The source reader to read characters from.</param>
    public void SetSource(StreamReader reader)
    {
        EnsureNotDisposed();
        Close();

        _source = new RewindingStreamReader(reader);
    }

    /// <summary>
    /// This method is used to read the next token from the parser.  If there are no more
    /// tokens that result from parsing, then <c>null</c> will be returned.
    /// </summary>
    /// <returns>The next token that was parsed, or <c>null</c>, if there are no more
    /// tokens.</returns>
    public Token? GetNextToken()
    {
        Token? token = ReadNextToken();

        // Anything handed out while a mark is pending has to be remembered so that a
        // rollback to that mark can put it back.  See MarkPosition().
        if (token != null && _marks.Count > 0)
            _consumedSinceMark.Add(token);

        return token;
    }

    /// <summary>
    /// This is a helper method that does the actual work of reading the next token, without
    /// the position bookkeeping that <see cref="GetNextToken"/> layers on top of it.
    /// </summary>
    /// <returns>The next token that was parsed, or <c>null</c>, if there are no more
    /// tokens.</returns>
    private Token? ReadNextToken()
    {
        EnsureNotDisposed();

        if (_returnedTokens.Count > 0)
        {
            Token buffered = _returnedTokens.Last();

            _returnedTokens.RemoveLast();

            return buffered;
        }

        if (_source == null)
            return null;

        char ch;

        // Note that Tokenizer.ParseToken() never returns null; it throws instead.  So, a
        // tokenizer that doesn't report its tokens (e.g., whitespace, comments) always
        // causes us to loop around and look for the next reportable token.
        while (true)
        {
            int data = GetNextChar();

            if (data < 0)
                return null;

            _line = _source.Line;
            _column = _source.Column;

            ch = (char) data;

            Tokenizer? tokenizer = _tokenizers
                .FirstOrDefault(t => t.StartsAToken(ch));

            if (tokenizer == null)
                break;

            Token token = tokenizer.ParseToken(ch, _line, _column);

            if (tokenizer.ReportTokens)
                return token;
        }

        throw new TokenException($"The character, '{ch}', is not recognized by any tokenizer.")
        {
            Line = _line,
            Column = _column
        };
    }

    /// <summary>
    /// This is a helper method for reading the next character from the underlying source.
    /// </summary>
    /// <returns>The next character from our underlying source.</returns>
    internal int GetNextChar()
    {
        EnsureNotDisposed();

        return _source?.Read() ?? -1;
    }

    /// <summary>
    /// This method is used to return a character back to the underlying reader.
    /// </summary>
    /// <param name="ch">The character to return.</param>
    internal void ReturnChar(int ch)
    {
        EnsureNotDisposed();

        if (ch >= 0)
            _source?.ReturnChar((char) ch);
    }

    /// <summary>
    /// this is a helper method for returning the content of the given builder back to our
    /// character reader (in the proper order).  The buffer's length is set to <c>keep</c>
    /// as a side effect.
    /// </summary>
    /// <param name="builder">The builder that contains the characters to return.</param>
    /// <param name="keep">The number of characters in the builder to keep.</param>
    internal void ReturnBuffer(StringBuilder builder, int keep = 0)
    {
        EnsureNotDisposed();

        for (int index = builder.Length - 1; index >= keep; index--)
            ReturnChar(builder[index]);

        builder.Length = keep;
    }

    /// <summary>
    /// This method is used to get the next token and throws an exception if one was not
    /// found.
    /// </summary>
    /// <param name="msgSource">The source for the message to use for the token missing
    /// exception.</param>
    /// <returns>The next token.</returns>
    public Token GetRequiredToken(Func<string>? msgSource = null)
    {
        EnsureNotDisposed();

        Token? result = GetNextToken();

        if (result == null)
        {
            throw new TokenException(msgSource?.Invoke() ?? "Unexpected end of input.")
            {
                Line = _line,
                Column = _column
            };
        }

        return result;
    }

    /// <summary>
    /// This is a helper method for taking a peek at the next token from the parser.
    /// </summary>
    /// <returns>The next token.</returns>
    public Token? PeekNextToken()
    {
        EnsureNotDisposed();

        Token? token = GetNextToken();

        ReturnToken(token);

        return token;
    }

    /// <summary>
    /// This is a convenience method that notes whether the parser has reached the end of
    /// its input.  This will be valid whether the parser has been disposed of or not.
    /// </summary>
    /// <returns><c>true</c>, if the parser has exhausted its input or <c>false</c>, if
    /// there are more tokens to read.</returns>
    public bool IsAtEnd()
    {
        EnsureNotDisposed();

        return PeekNextToken() == null;
    }

    /// <summary>
    /// This is a helper method for making sure the next token matches one of the ones
    /// specified.
    /// </summary>
    /// <param name="isRequired">Whether the next token is required.</param>
    /// <param name="msgSource">An optional source for the "no tokens match" message.</param>
    /// <param name="tokens">The tokens to compare against.</param>
    /// <returns>The token just verified.</returns>
    public Token MatchToken(bool isRequired, Func<string>? msgSource = null, params Token[] tokens)
    {
        EnsureNotDisposed();

        Token? token = isRequired ? GetRequiredToken(msgSource) : GetNextToken();

        if (IsNext(token, tokens))
            return token;

        ReturnToken(token);

        throw new TokenException(msgSource?.Invoke() ?? "The token is not allowed here.")
        {
            Token = token,
            Line = token?.Line ?? _line,
            Column = token?.Column ?? _column
        };
    }

    /// <summary>
    /// This method checks to see if the next token in the stream matches any of the ones
    /// specified.  The state of the token stream remains unchanged.
    /// </summary>
    /// <param name="tokens">The tokens to try to match.</param>
    /// <returns><c>true</c>, if the next token from the parser matches one of the specified
    /// tokens.</returns>
    public bool IsNext(params Token[] tokens)
    {
        EnsureNotDisposed();

        return IsNext(PeekNextToken(), tokens);
    }

    /// <summary>
    /// This method checks to see if the given token matches any of the ones specified.
    /// </summary>
    /// <param name="token">The token to look for.</param>
    /// <param name="tokens">The tokens to try to match.</param>
    /// <returns><c>true</c>, if the given token matches one of the specified tokens.</returns>
    private static bool IsNext([NotNullWhen(true)] Token? token, params Token[] tokens)
    {
        if (tokens == null || tokens.Length == 0)
            throw new ArgumentException($"No tokens provided to check.");

        return token != null && tokens.Any(next => next.Matches(token));
    }

    /// <summary>
    /// This is a helper method for making sure the type of the next token matches one of
    /// the types specified.
    /// </summary>
    /// <param name="isRequired">Whether the next token is required.</param>
    /// <param name="msgSource">An optional source for the "no token types match" message.</param>
    /// <param name="types">The token types to compare against.</param>
    /// <returns>The token just verified.</returns>
    public Token MatchTokenType(bool isRequired, Func<string>? msgSource, params Type[] types)
    {
        EnsureNotDisposed();

        if (!types.All(type => typeof(Token).IsAssignableFrom(type)))
            throw new ArgumentException($"Not all specified types are Token subclasses.");

        Token? token = isRequired ? GetRequiredToken(msgSource) : GetNextToken();

        if (IsNextOfType(token, types))
            return token;

        ReturnToken(token);

        throw new TokenException(msgSource?.Invoke() ?? "This type of token is not expected here.")
        {
            Token = token,
            Line = token?.Line ?? _line,
            Column = token?.Column ?? _column
        };
    }

    /// <summary>
    /// This method checks to see if the type of the next token in the stream matches any
    /// of the ones specified.  The state of the token stream remains unchanged.
    /// </summary>
    /// <param name="types">The types to try to match.</param>
    /// <returns><c>true</c>, if the next token from the parser is of one of the specified
    /// types.</returns>
    public bool IsNextOfType(params Type[] types)
    {
        EnsureNotDisposed();

        return IsNextOfType(PeekNextToken(), types);
    }

    /// <summary>
    /// This method checks to see if the type of the given token matches any of the ones
    /// specified.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <param name="types">The types to try to match.</param>
    /// <returns><c>true</c>, if the given token type matches one of the specified types.</returns>
    private static bool IsNextOfType([NotNullWhen(true)] Token? token, params Type[] types)
    {
        if (types == null || types.Length == 0)
            throw new ArgumentException($"No token types provided to check.");

        return token != null && types.Any(next => next.IsInstanceOfType(token));
    }

    /// <summary>
    /// This method is used to return a token to the parser.  If multiple tokens need to be
    /// returned, do so in the reverse order that they were read.  It is the caller's
    /// responsibility to ensure proper ordering when the tokens are re-read.
    /// </summary>
    /// <param name="token">The token to return to the parser.</param>
    public void ReturnToken(Token? token)
    {
        EnsureNotDisposed();

        if (token == null)
            return;

        _returnedTokens.AddLast(token);

        // Returning a token un-consumes it, so it must come back off the record of what
        // we've handed out since the pending mark; otherwise a rollback would hand it back a
        // second time.  This leans on tokens being returned in the reverse order they were
        // read, which is exactly what this method's contract asks of callers.
        if (_marks.Count > 0 && _consumedSinceMark.Count > 0 &&
            ReferenceEquals(_consumedSinceMark[^1], token))
            _consumedSinceMark.RemoveAt(_consumedSinceMark.Count - 1);
    }

    /// <summary>
    /// This method is used to return am enumeration of tokens to the parser.  It is assumed
    /// that the enumeration emits the tokens in the order they were fetched from the parser.
    /// To return them properly, this method reverses that order.
    /// </summary>
    /// <param name="tokens">The tokens to return.</param>
    public void ReturnTokens(IEnumerable<Token>? tokens)
    {
        EnsureNotDisposed();

        if (tokens != null)
        {
            foreach (Token token in tokens.Reverse()
                         .Where(t => t != null))
                ReturnToken(token);
        }
    }

    /// <summary>
    /// This method is used to mark the parser's current position so that it can be returned
    /// to later, by way of <see cref="RollbackToMark"/>.  Use this when you need to try
    /// something that may not pan out, and have the parser end up where it started when it
    /// doesn't.
    /// </summary>
    /// <remarks>
    /// This exists because returning tokens by hand can only undo what the caller actually
    /// holds tokens for.  A clause that consumes an expression, for one, gets back a term
    /// rather than the tokens that went into it, and so has nothing to hand back; the same
    /// goes for tokens a term choice was told to suppress.  Marking records what the parser
    /// hands out, so a rollback can undo the lot regardless of who kept what.
    /// <para>
    /// Every mark must be resolved exactly once, by either <see cref="RollbackToMark"/> or
    /// <see cref="ReleaseMark"/>.  Marks nest, so an inner one must be resolved before the
    /// one that encloses it.
    /// </para>
    /// <para>
    /// Note that rolling the tokens back does not roll back any work a consumer did while
    /// reading them.  An <see cref="Lex.Expressions.IExpressionTreeBuilder"/>, in particular,
    /// will already have been asked to build the terms of an expression that is about to be
    /// abandoned, so keep such builders free of side effects beyond building the term.
    /// </para>
    /// </remarks>
    public void MarkPosition()
    {
        EnsureNotDisposed();

        _marks.Push(_consumedSinceMark.Count);
    }

    /// <summary>
    /// This method is used to return the parser to the position of the most recent mark,
    /// resolving that mark.  Every token handed out since it was set is returned to the
    /// parser, so that the next read picks up where the mark was set.
    /// </summary>
    public void RollbackToMark()
    {
        EnsureNotDisposed();

        if (_marks.Count == 0)
            throw new InvalidOperationException("There is no marked position to roll back to.");

        int mark = _marks.Pop();

        // These go straight onto the returned-token stack rather than through ReturnToken(),
        // since we truncate the record ourselves right after.
        for (int index = _consumedSinceMark.Count - 1; index >= mark; index--)
            _returnedTokens.AddLast(_consumedSinceMark[index]);

        _consumedSinceMark.RemoveRange(mark, _consumedSinceMark.Count - mark);
    }

    /// <summary>
    /// This method is used to resolve the most recent mark without moving the parser,
    /// keeping everything consumed since it was set.  Call this once whatever the mark was
    /// guarding has worked out.
    /// </summary>
    public void ReleaseMark()
    {
        EnsureNotDisposed();

        if (_marks.Count == 0)
            throw new InvalidOperationException("There is no marked position to release.");

        _marks.Pop();

        // With no mark left pending, nothing can roll back any more, so the record can go
        // rather than grow for the rest of the parse.
        if (_marks.Count == 0)
            _consumedSinceMark.Clear();
    }

    /// <summary>
    /// This method is used to close the current character source and clean up after
    /// ourselves.
    /// </summary>
    public void Close()
    {
        EnsureNotDisposed();

        _source?.Dispose();
        _returnedTokens.Clear();
        _consumedSinceMark.Clear();
        _marks.Clear();

        _source = null;
    }

    /// <summary>
    /// This method is used to dispose of the parser.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Close();

            _disposed = true;
        }
    }

    /// <summary>
    /// This method is used to ensure that this instance of the parser has not been
    /// disposed.
    /// </summary>
    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LexicalParser), "The parser has been disposed.");
    }
}
