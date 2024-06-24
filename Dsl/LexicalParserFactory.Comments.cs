using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Lex.Dsl;

/// <summary>
/// This class implements a DSL (go figure) for defining and configuring a lexical parser.
/// </summary>
public static partial class LexicalParserFactory
{
    /// <summary>
    /// This method is used to update the given parser with standard comments handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the standard comments statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleStandardCommentsClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(CommentTokenizer), usedTypes, tokens);

        tokens.RemoveRange(0, 2);

        CommentTokenizer tokenizer = new CommentTokenizer(parser).AddStandardMarkers();

        AddExtraCommentMarkers(tokens, tokenizer);

        return tokenizer;
    }

    /// <summary>
    /// This method is used to update the given parser with comments handling.
    /// </summary>
    /// <param name="tokens">The list of tokens that are the comments statement.</param>
    /// <param name="dsl">An optional DSL object to pull information from.</param>
    /// <param name="parser">The parser we are configuring.</param>
    /// <param name="usedTypes">The set of already used tokenizer types.</param>
    /// <returns>The created and configured tokenizer.</returns>
    private static Tokenizer HandleCommentsClause(
        List<Token> tokens, Dsl dsl, LexicalParser parser, ISet<string> usedTypes)
    {
        EnsureFirstTime(typeof(CommentTokenizer), usedTypes, tokens);

        tokens.RemoveRange(0, 3);

        CommentTokenizer tokenizer = new CommentTokenizer(parser);

        AddCommentMarkers(tokens, tokenizer);
        AddExtraCommentMarkers(tokens, tokenizer);

        return tokenizer;
    }

    /// <summary>
    /// This is a helper method for adding markers to a comment tokenizer.
    /// </summary>
    /// <param name="tokens">The list of tokens to pull from.</param>
    /// <param name="tokenizer">The tokenizer to add markers to.</param>
    private static void AddCommentMarkers(List<Token> tokens, CommentTokenizer tokenizer)
    {
        string starter = tokens.RemoveFirst().Text;

        tokens.RemoveFirst();

        Token enderToken = tokens.RemoveFirst();

        if (enderToken is StringToken stringToken)
            tokenizer.AddMarkers(starter, stringToken.Text);
        else
            tokenizer.AddLineCommentMarker(starter);
    }

    /// <summary>
    /// This is a helper method for adding extra markers to a comment tokenizer.
    /// </summary>
    /// <param name="tokens">The list of tokens to pull from.</param>
    /// <param name="tokenizer">The tokenizer to add markers to.</param>
    private static void AddExtraCommentMarkers(List<Token> tokens, CommentTokenizer tokenizer)
    {
        while (tokens.GetTokenText() == "or")
        {
            tokens.RemoveFirst();

            AddCommentMarkers(tokens, tokenizer);
        }
    }
}
