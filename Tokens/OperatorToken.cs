using System.Reflection;

namespace Lex.Tokens;

/// <summary>
/// This class represents an operator token.
/// </summary>
public class OperatorToken : Token
{
    /// <summary>
    /// This constant defines the semicolon operator.
    /// </summary>
    public static readonly OperatorToken SemiColon = new (";");

    /// <summary>
    /// This constant defines the colon operator.
    /// </summary>
    public static readonly OperatorToken Colon = new (":");

    /// <summary>
    /// This constant defines the comma operator.
    /// </summary>
    public static readonly OperatorToken Comma = new (",");

    /// <summary>
    /// This constant defines the dot operator.
    /// </summary>
    public static readonly OperatorToken Dot = new (".");

    /// <summary>
    /// This constant defines the at operator.
    /// </summary>
    public static readonly OperatorToken At = new ("@");

    /// <summary>
    /// This constant defines the pipe (vertical bar) operator.
    /// </summary>
    public static readonly OperatorToken Pipe = new ("|");

    /// <summary>
    /// This constant defines the "or" (double vertical bar) operator.
    /// </summary>
    public static readonly OperatorToken Or = new ("||");

    /// <summary>
    /// This constant defines the ampersand operator.
    /// </summary>
    public static readonly OperatorToken Ampersand = new ("&");

    /// <summary>
    /// This constant defines the "and" (double ampersand bar) operator.
    /// </summary>
    public static readonly OperatorToken And = new ("&&");

    /// <summary>
    /// This constant defines the tilde operator.
    /// </summary>
    public static readonly OperatorToken Tilde = new ("~");

    /// <summary>
    /// This constant defines the not (exclamation point) operator.
    /// </summary>
    public static readonly OperatorToken Not = new ("!");

    /// <summary>
    /// This constant defines the hash operator.
    /// </summary>
    public static readonly OperatorToken Hash = new ("#");

    /// <summary>
    /// This constant defines the dollar operator.
    /// </summary>
    public static readonly OperatorToken Dollar = new ("$");

    /// <summary>
    /// This constant defines the modulo (percent) operator.
    /// </summary>
    public static readonly OperatorToken Modulo = new ("%");

    /// <summary>
    /// This constant defines the power (caret) operator.
    /// </summary>
    public static readonly OperatorToken Power = new ("^");

    /// <summary>
    /// This constant defines the underscore operator.
    /// </summary>
    public static readonly OperatorToken Underscore = new ("_");

    /// <summary>
    /// This constant defines the backslash operator.
    /// </summary>
    public static readonly OperatorToken Backslash = new (@"\");

    /// <summary>
    /// This constant defines the plus operator.
    /// </summary>
    public static readonly OperatorToken Plus = new ("+");

    /// <summary>
    /// This constant defines the double plus operator.
    /// </summary>
    public static readonly OperatorToken DoublePlus = new ("++");

    /// <summary>
    /// This constant defines the minus operator.
    /// </summary>
    public static readonly OperatorToken Minus = new ("-");

    /// <summary>
    /// This constant defines the double minus operator.
    /// </summary>
    public static readonly OperatorToken DoubleMinus = new ("--");

    /// <summary>
    /// This constant defines the multiply operator.
    /// </summary>
    public static readonly OperatorToken Multiply = new ("*");

    /// <summary>
    /// This constant defines the divide operator.
    /// </summary>
    public static readonly OperatorToken Divide = new ("/");

    /// <summary>
    /// This constant defines the assignment operator.
    /// </summary>
    public static readonly OperatorToken Assignment = new ("=");

    /// <summary>
    /// This constant defines the plus equal operator.
    /// </summary>
    public static readonly OperatorToken PlusEqual = new ("+=");

    /// <summary>
    /// This constant defines the minus equal operator.
    /// </summary>
    public static readonly OperatorToken MinusEqual = new ("-=");

    /// <summary>
    /// This constant defines the multiply equal operator.
    /// </summary>
    public static readonly OperatorToken MultiplyEqual = new ("*=");

    /// <summary>
    /// This constant defines the divide equal operator.
    /// </summary>
    public static readonly OperatorToken DivideEqual = new ("/=");

    /// <summary>
    /// This constant defines the or equal operator.
    /// </summary>
    public static readonly OperatorToken OrEqual = new ("|=");

    /// <summary>
    /// This constant defines the and equal operator.
    /// </summary>
    public static readonly OperatorToken AndEqual = new ("&=");

    /// <summary>
    /// This constant defines the equal operator.
    /// </summary>
    public static readonly OperatorToken Equal = new ("==");

    /// <summary>
    /// This constant defines the not equal operator.
    /// </summary>
    public static readonly OperatorToken NotEqual = new ("!=");

    /// <summary>
    /// This constant defines the less than operator.
    /// </summary>
    public static readonly OperatorToken LessThan = new ("<");

    /// <summary>
    /// This constant defines the less than or equal operator.
    /// </summary>
    public static readonly OperatorToken LessThanOrEqual = new ("<=");

    /// <summary>
    /// This constant defines the greater than operator.
    /// </summary>
    public static readonly OperatorToken GreaterThan = new (">");

    /// <summary>
    /// This constant defines the greater than or equal operator.
    /// </summary>
    public static readonly OperatorToken GreaterThanOrEqual = new (">=");

    /// <summary>
    /// This constant defines the shift left operator.
    /// </summary>
    public static readonly OperatorToken ShiftLeft = new ("<<");

    /// <summary>
    /// This constant defines the shift right operator.
    /// </summary>
    public static readonly OperatorToken ShiftRight = new (">>");

    /// <summary>
    /// This constant defines the unsigned shift right operator.
    /// </summary>
    public static readonly OperatorToken UnsignedShiftRight = new (">>>");

    /// <summary>
    /// This constant defines the arrow operator.
    /// </summary>
    public static readonly OperatorToken Arrow = new ("->");

    /// <summary>
    /// This constant defines the double arrow operator.
    /// </summary>
    public static readonly OperatorToken DoubleArrow = new ("=>");

    /// <summary>
    /// This constant defines the range operator.
    /// </summary>
    public static readonly OperatorToken Range = new ("..");

    /// <summary>
    /// This constant defines the if operator.
    /// </summary>
    public static readonly OperatorToken If = new ("?");

    /// <summary>
    /// This constant defines the coalesce operator.
    /// </summary>
    public static readonly OperatorToken Coalesce = new ("??");

    /// <summary>
    /// This field holds our predefined operator tokens as a dictionary, keyed by their
    /// names, folded to lower case.
    /// </summary>
    internal static Dictionary<string, OperatorToken> NamedOperators => LazyNamedOperators.Value;

    private static readonly Lazy<Dictionary<string, OperatorToken>> LazyNamedOperators = new (GetNamedOperators);

    /// <summary>
    /// This method is used to create a dictionary of our predefined operator tokens.  The
    /// dictionary is keyed by each token's field name, folded to lower-case.
    /// </summary>
    /// <remarks>
    /// Do not use this method directly; use the <see cref="NamedOperators"/> property.  This
    /// will ensure we take the reflection hit only once.
    /// </remarks>
    /// <returns>The dictionary of our predefined operator tokens.</returns>
    private static Dictionary<string, OperatorToken> GetNamedOperators()
    {
        Dictionary<string, OperatorToken> result = new ();

        foreach (FieldInfo info in typeof(OperatorToken).GetFields(
                     BindingFlags.Public | BindingFlags.Static)
                     .Where(fieldInfo => fieldInfo.FieldType == typeof(OperatorToken)))
        {
            string name = info.Name.ToLowerInvariant();

            result[name] = (OperatorToken) info.GetValue(null);
        }

        return result;
    }

    public OperatorToken(string text) : base(text) {}
}
