namespace Lex.Tokens;

/// <summary>
/// This class represents a string token.
/// </summary>
public class StringToken : Token
{
    /// <summary>
    /// This property holds the text of the bounder that marked out the string literal
    /// we represent.
    /// </summary>
    public string Bounder { get; }

    public StringToken(string bounder, string text) : base(text)
    {
        Bounder = bounder;
    }
}
