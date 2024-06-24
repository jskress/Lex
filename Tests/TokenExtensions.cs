using Lex.Tokens;

namespace Tests;

/// <summary>
/// This class provides some extra functionality for tokens to support testing.
/// </summary>
public static class TokenExtensions
{
    /// <summary>
    /// This method tests to see whether the token we were invoked on exactly matches
    /// another token.  This includes the extra properties provided on various token
    /// subclasses.  Note that the line and column properties are not matched.
    /// </summary>
    /// <param name="token">The first token to compare.</param>
    /// <param name="other">The second token to compare.</param>
    public static void AssertTokenMatches(this Token token, Token other)
    {
        if (token == null && other == null)
            return;

        // Basic matching
        Assert.IsFalse(token == null || other == null, $"Token {token.Str()} does not exactly match {other.Str()}.");
        Assert.IsTrue(token.Matches(other), $"Token {token} does not exactly match {other}.");

        // Type-specific fields
        switch (token)
        {
            case NumberToken numberToken:
                numberToken.VerifyExactlyMatches(other);
                break;

            case StringToken stringToken:
                stringToken.VerifyExactlyMatches(other);
                break;
        }
    }

    /// <summary>
    /// This method test to see whether two number tokens exactly match.  This will be true
    /// if both have the same numeric properties.
    /// </summary>
    /// <param name="token">The first token to compare.</param>
    /// <param name="other">The second token to compare.</param>
    private static void VerifyExactlyMatches(this NumberToken token, Token other)
    {
        NumberToken numberToken = (NumberToken) other;

        Assert.IsTrue(token.IsFloatingPoint == numberToken.IsFloatingPoint &&
                      (token.IsFloatingPoint
                          ? Math.Abs(token.FloatingPointNumber - numberToken.FloatingPointNumber) < 0.00001
                          : token.IntegralNumber == numberToken.IntegralNumber),
            $"Number data in token {token} does not exactly match that in {other}.");
    }

    /// <summary>
    /// This method test to see whether two number tokens exactly match.  This will be true
    /// if both have the same numeric properties.
    /// </summary>
    /// <param name="token">The first token to compare.</param>
    /// <param name="other">The second token to compare.</param>
    private static void VerifyExactlyMatches(this StringToken token, Token other)
    {
        StringToken stringToken = (StringToken) other;

        Assert.IsTrue(token.Bounder == stringToken.Bounder,
            $"String data in token {token} does not exactly match that in {other}.");
    }

    /// <summary>
    /// A null-appropriate converter of tokens to strings.
    /// </summary>
    /// <param name="token">The token (or <c>null</c>) to convert to a string.</param>
    /// <returns>The token as a string.</returns>
    private static string Str(this Token token)
    {
        return token == null ? "<null>" : token.ToString();
    }
}
