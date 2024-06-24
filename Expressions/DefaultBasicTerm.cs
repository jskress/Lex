using Lex.Tokens;

namespace Lex.Expressions;

/// <summary>
/// This class represents a basic term in an expression.
/// </summary>
public class DefaultBasicTerm : DefaultExpressionTerm
{
    /// <summary>
    /// This property holds the list of child terms that make up this term.
    /// </summary>
    public IReadOnlyList<DefaultExpressionTerm> Terms { get; internal init; }

    /// <summary>
    /// This property holds the tag that was configured with the term parser.
    /// </summary>
    public string Tag { get; internal init; }

    /// <summary>
    /// This method produces a text representation of this term.
    /// </summary>
    /// <returns>A text representation of this term.</returns>
    protected override string Format()
    {
        int bounder = FindBounderTokens();
        string leading = bounder < 0
            ? string.Join(' ', Tokens.Select(ToText))
            : string.Join(' ', Tokens.ToList()[..bounder].Select(ToText));
        string terms = string.Join(", ", Terms.Select(term => term.Text));
        string trailing = bounder < 0
            ? ""
            : string.Join(' ', Tokens.ToList()[bounder..].Select(ToText));
        string suffix = Tag == null ? "" : $" => {Tag}";

        return leading + terms + trailing + suffix;
    }

    /// <summary>
    /// This is a helper method for finding tokens that are pairs of parentheses or square
    /// brackets.
    /// </summary>
    /// <returns>The index of the closing bounder or <c>-1</c>.</returns>
    private int FindBounderTokens()
    {
        for (int index = 0; index < Tokens.Count - 1; index++)
        {
            if ((BounderToken.LeftParen.Matches(Tokens[index]) &&
                 BounderToken.RightParen.Matches(Tokens[index + 1])) ||
                (BounderToken.OpenBracket.Matches(Tokens[index]) &&
                 BounderToken.CloseBracket.Matches(Tokens[index + 1])))
                return index + 1;
        }

        return -1;
    }

    /// <summary>
    /// This is a helper method for converting a token to display text.  This is primarily
    /// to make sure string literals are properly noted.
    /// </summary>
    /// <param name="token">The token to convert to text.</param>
    /// <returns>The token as text.</returns>
    private static string ToText(Token token)
    {
        return token is StringToken stringToken
            ? stringToken.Bounder + stringToken.Text + stringToken.Bounder
            : token.Text;
    }
}
