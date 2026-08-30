using Lex;
using Lex.Dsl;
using Lex.Parser;
using Lex.Tokenizers;
using Lex.Tokens;

namespace Tests.TokenizerTests;

/// <summary>
/// These tests cover a keyword being recognized only when it is a whole word.
/// </summary>
/// <remarks>
/// The tokenizer used to match its keywords against the character stream directly, which took
/// the <c>flaw</c> out of <c>flaw2</c> and called it a keyword, leaving a stray <c>2</c>
/// behind, when what is written there is one perfectly ordinary identifier.  It reads the
/// whole word now and only then looks it up.
/// </remarks>
[TestClass]
public class KeywordBoundaryTests : TokenizerTestsBase
{
    /// <summary>
    /// This is a helper that lexes the given source and describes what came out, so that a
    /// case can be stated as one readable line.
    /// </summary>
    private static string Lex(string source, Action<LexicalParser> setup)
    {
        using LexicalParser parser = new ();

        setup(parser);
        parser.SetSource(source.AsReader());

        List<string> tokens = [];

        while (parser.GetNextToken() is { } token)
            tokens.Add($"{token.GetType().Name.Replace("Token", string.Empty)}({token.Text})");

        return string.Join(' ', tokens);
    }

    private static void Standard(LexicalParser parser)
    {
        _ = new KeywordTokenizer(parser, "flaw", "in", "int");
        _ = new IdTokenizer(parser);
        _ = new NumberTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);
    }

    [TestMethod]
    public void TestAKeywordIsOnlyAKeywordAsAWholeWord()
    {
        foreach ((string source, string expected) in new[]
                 {
                     // The reported case, and the same fault reached through an underscore.
                     ("flaw2", "Id(flaw2)"),
                     ("flaw_x", "Id(flaw_x)"),
                     ("flaw2x", "Id(flaw2x)"),
                     ("_flaw", "Id(_flaw)"),

                     // A keyword that really is a whole word still is one.
                     ("flaw", "Keyword(flaw)"),
                     ("flaw 2", "Keyword(flaw) Number(2)"),
                     ("flaw,", "Keyword(flaw) Operator(,)"),
                     // A number runs up against the next word, which is lexed on its own terms.
                     ("2flaw", "Number(2) Keyword(flaw)"),

                     // These were already right and must stay so.
                     ("flawed", "Id(flawed)"),
                     ("xflaw", "Id(xflaw)")
                 })
        {
            Assert.AreEqual(expected, Lex(source, parser =>
            {
                _ = new KeywordTokenizer(parser, "flaw");
                _ = new IdTokenizer(parser);
                _ = new NumberTokenizer(parser);
                _ = new OperatorTokenizer(parser);
                _ = new WhitespaceTokenizer(parser);
            }), $"lexing \"{source}\"");
        }
    }

    /// <summary>
    /// One keyword being a prefix of another is the case most likely to go wrong, since the
    /// old matching worked through the list longest-first rather than reading the word.
    /// </summary>
    [TestMethod]
    public void TestKeywordsThatArePrefixesOfOtherKeywords()
    {
        foreach ((string source, string expected) in new[]
                 {
                     ("in", "Keyword(in)"),
                     ("int", "Keyword(int)"),
                     ("into", "Id(into)"),
                     ("in2", "Id(in2)"),
                     ("int8", "Id(int8)"),
                     ("in int into", "Keyword(in) Keyword(int) Id(into)")
                 })
        {
            Assert.AreEqual(expected, Lex(source, Standard), $"lexing \"{source}\"");
        }
    }

    /// <summary>
    /// Where a word ends is the identifier tokenizer's business, so a language that says an
    /// identifier may contain a dollar sign must have that respected here too.
    /// </summary>
    [TestMethod]
    public void TestTheIdentifierTokenizerDecidesWhereAWordEnds()
    {
        void Setup(LexicalParser parser)
        {
            _ = new KeywordTokenizer(parser, "flaw");
            _ = new IdTokenizer(parser, IdTokenizer.DefaultStarters, IdTokenizer.DefaultMembers + "$");
            _ = new WhitespaceTokenizer(parser);
        }

        Assert.AreEqual("Id(flaw$x)", Lex("flaw$x", Setup));
        Assert.AreEqual("Id(flaw$)", Lex("flaw$", Setup));
        Assert.AreEqual("Keyword(flaw)", Lex("flaw", Setup));
    }

    /// <summary>
    /// The identifier tokenizer may be registered either side of us, so it is looked up when
    /// it is needed rather than when we are built.
    /// </summary>
    [TestMethod]
    public void TestTheIdentifierTokenizerIsFoundWhenRegisteredAfterUs()
    {
        // Keywords first, which is the usual order and the only one where keywords win.
        Assert.AreEqual("Id(flaw$x)", Lex("flaw$x", parser =>
        {
            _ = new KeywordTokenizer(parser, "flaw");
            _ = new IdTokenizer(parser, IdTokenizer.DefaultStarters, IdTokenizer.DefaultMembers + "$");
            _ = new WhitespaceTokenizer(parser);
        }));
    }

    /// <summary>
    /// Keyword casing and identifier casing are separate settings, and must stay separate.
    /// </summary>
    [TestMethod]
    public void TestKeywordCasingIsIndependentOfIdentifierCasing()
    {
        void Setup(LexicalParser parser)
        {
            _ = new KeywordTokenizer(parser, "select", "from") { Style = LetterCaseStyle.LowerCase };
            _ = new IdTokenizer(parser) { Style = LetterCaseStyle.AsIs };
            _ = new WhitespaceTokenizer(parser);
        }

        Assert.AreEqual(
            "Keyword(select) Id(Foo) Keyword(from) Id(Bar)", Lex("SELECT Foo FROM Bar", Setup));

        // ...and the boundary rule still applies through the styling.
        Assert.AreEqual("Id(SELECT2)", Lex("SELECT2", Setup));
        Assert.AreEqual("Id(Selected)", Lex("Selected", Setup));
    }

    /// <summary>
    /// With no identifier tokenizer, a word can only be a run of letters, so a keyword may
    /// still be followed directly by something that is not one.
    /// </summary>
    [TestMethod]
    public void TestWithNoIdentifierTokenizerAWordIsJustLetters()
    {
        void Setup(LexicalParser parser)
        {
            _ = new KeywordTokenizer(parser, "flaw");
            _ = new NumberTokenizer(parser);
            _ = new WhitespaceTokenizer(parser);
        }

        Assert.AreEqual("Keyword(flaw)", Lex("flaw", Setup));
        Assert.AreEqual("Keyword(flaw) Number(2)", Lex("flaw2", Setup));
    }

    /// <summary>
    /// A keyword built from letters the identifier tokenizer will not accept is still a
    /// keyword; narrowing what an identifier may be should not cost you your keywords.
    /// </summary>
    [TestMethod]
    public void TestKeywordsSurviveANarrowedIdentifierAlphabet()
    {
        void Setup(LexicalParser parser)
        {
            _ = new KeywordTokenizer(parser, "Select");
            _ = new IdTokenizer(parser, IdTokenizer.Lowers, IdTokenizer.Lowers);
            _ = new WhitespaceTokenizer(parser);
        }

        Assert.AreEqual("Keyword(Select)", Lex("Select", Setup));
        Assert.AreEqual("Id(abc)", Lex("abc", Setup));
    }

    /// <summary>
    /// Keywords added or removed after construction have to take effect, which means the
    /// styled lookup set cannot be cached past them.
    /// </summary>
    [TestMethod]
    public void TestKeywordsChangedAfterConstructionTakeEffect()
    {
        using LexicalParser parser = new ();

        KeywordTokenizer keywords = new (parser, "flaw");

        _ = new IdTokenizer(parser);
        _ = new WhitespaceTokenizer(parser);

        keywords.Including(["extra"]);

        parser.SetSource("extra flaw".AsReader());

        Assert.AreEqual("Keyword(extra)", Describe(parser.GetNextToken()));
        Assert.AreEqual("Keyword(flaw)", Describe(parser.GetNextToken()));

        keywords.Excluding(["flaw"]);

        parser.SetSource("extra flaw".AsReader());

        Assert.AreEqual("Keyword(extra)", Describe(parser.GetNextToken()));
        Assert.AreEqual("Id(flaw)", Describe(parser.GetNextToken()));

        // Changing the style must invalidate the lookup set as well.
        keywords.Style = LetterCaseStyle.UpperCase;

        parser.SetSource("EXTRA".AsReader());

        Assert.AreEqual("Keyword(EXTRA)", Describe(parser.GetNextToken()));
    }

    private static string Describe(Token? token)
    {
        return token == null
            ? "null"
            : $"{token.GetType().Name.Replace("Token", string.Empty)}({token.Text})";
    }

    /// <summary>
    /// The same thing, reached the way a consumer actually configures a parser.
    /// </summary>
    [TestMethod]
    public void TestTheBoundaryRuleHoldsThroughTheParserDsl()
    {
        using LexicalParser parser = LexicalParserFactory.CreateFrom(
            """
            keywords 'flaw', 'in'
            identifiers
            integral numbers
            whitespace
            """);

        parser.SetSource("flaw flaw2 in into".AsReader());

        Assert.AreEqual("Keyword(flaw)", Describe(parser.GetNextToken()));
        Assert.AreEqual("Id(flaw2)", Describe(parser.GetNextToken()));
        Assert.AreEqual("Keyword(in)", Describe(parser.GetNextToken()));
        Assert.AreEqual("Id(into)", Describe(parser.GetNextToken()));
        Assert.IsNull(parser.GetNextToken());
    }
}
