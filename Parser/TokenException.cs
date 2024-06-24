using Lex.Tokens;

namespace Lex.Parser;

/// <summary>
/// This class represents an exception that occurred during parsing.
/// </summary>
public class TokenException : Exception
{
    /// <summary>
    /// This property holds the token relating to the exception.
    /// </summary>
    public Token Token
    {
        get => _token;
        set
        {
            _token = value;

            if (value != null)
            {
                if (Line < 0)
                    Line = value.Line;

                if (Column < 0)
                    Column = value.Column;
            }
        }
    }

    /// <summary>
    /// This property holds the line in the source that the first character of the token
    /// was found on.
    /// </summary>
    public int Line { get; set; } = -1;

    /// <summary>
    /// This property holds the column in the source that the first character of the token
    /// was found on.
    /// </summary>
    public int Column { get; set; } = -1;

    private Token _token;

    public TokenException() {}
    public TokenException(string message) : base(message) {}
    public TokenException(string message, Exception innerException) : base(message, innerException) {}
}
