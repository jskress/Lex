## The Tokenizers

This section describes each of the tokenizers provided by the Lex library.

### The Based Number Tokenizer

The based number tokenizer is used to parse numeric literals noted in base 2, 8 or 16.
Base 2 (binary) literals are formed by the `0b` or `0B` character sequence, followed by 1
or more `0` or `1` digits.  Base 8 (octal) literals are formed by a leading `0`, followed
by 1 or more digits from `0` to `7`.  Base 16 (hexadecimal) literals are formed by the `0x`
or `0X` character sequence, followed by 1 or more characters that are either digits, `0` to
`9`, or the letters, `a` to `f` or `A` to `F`.

The tokenizer is provided by the [`BasedNumberTokenizer`](../Tokenizers/BasedNumberTokenizer.cs)
class.  By default, it reports tokens it parses.

By default, the tokenizer will recognize numeric literals in all three bases.  Any of the
`supportHex`, `supportOctal` and `supportBinary` named arguments to the constructor may be
set to `false` to disable the recognition of the indicated numeric base.

This tokenizer produces instances of the [`NumberToken`](../Tokens/NumberToken.cs) token
class.  The value of the token will always be integral.

### The Bounder Tokenizer

The bounder tokenizer is used to parse "bounders".  Bounders are most often in pairs and
serve to surround, or mark off, a collection of linguistic items.  The default bounders the
tokenizer recognizes are:

- Braces which are used to group a collection of items such as statements in C-like languages
  or a map/dictionary as is done in JSON
- Parentheses which are used to group things like arguments to a function or tuple
  specifications
- Square brackets which are used to denote sequence or dictionary indexing or array content
  specifications

Bounders are always single characters.

The tokenizer is provided by the [`BounderTokenizer`](../Tokenizers/BounderTokenizer.cs)
class.  By default, it reports tokens it parses.

By default, the tokenizer will recognize the default set of open and close braces (`{` and
`}`), open and close brackets (`[` and `]`) and left and right parentheses (`(` and `)`).

If you want the tokenizer to recognize a different set of bounders, provide **all** characters
that should be recognized as bounders in the `bounders` argument to the constructor.  Each
character of that string will be reported as a bounder token.

This tokenizer produces instances of the [`BounderToken`](../Tokens/BounderToken.cs) token
class.  For convenience, this class defines static fields, each of which representing one
of the default bounders.  This is useful for making comparisons.

### The Comment Tokenizer

The comment tokenizer is used to parse linguistic comments.  Comments are a series of
characters that the language compiler or interpreter ignores.  They allow developers to
put in information that will help inform future maintainers of the code.

Comments are either line-oriented or block-oriented.  Line-oriented comments have some sort
of marker, like the double-slash, `//` or the hash, `#`.  Any text on the line where the
starting marker occurs to the end of the line is taken as a comment.  Block-oriented comments
have both a starting marker and an ending one, like `/*` and `*/` in C-like languages.

The tokenizer is provided by the [`CommentTokenizer`](../Tokenizers/CommentTokenizer.cs)
class.  By default, it does not report tokens it parses.

Once instantiated, you will use any of 3 methods to register comment markers.  You can
register as many as you like.

- Use `AddStandardMarkers()` to add both the `//` line-oriented comment marker and the `/*`
  and `*/` block-oriented comment marker to the ones the tokenizer should look for.
- Use `AddLineCommentMarker(string starter)` to add a line-oriented comment marker to the
  ones the tokenizer should look for.
- Use `AddMarkers(string starter, string ender)` to add a block-oriented comment marker to
  the ones the tokenizer should look for.

This tokenizer produces instances of the [`CommentToken`](../Tokens/CommentToken.cs) token
class.  Note that the text carried by the token will include the comment start and end
markers.  For line-oriented comments, this includes the trailing new line that marks the
end of the comment.

### The Double Quoted String Tokenizer

The double-quoted string tokenizer is a convenience subclass of the
[`StringTokenizer`](../Tokenizers/StringTokenizer.cs) class.  It configures a string
tokenizer that uses the double quote, `"` as the bounding character.  The resulting
tokenizer follows the escaping rules used by most languages that support double-quoted
string literals.

See the [string tokenizer](#the-string-tokenizer) section for more details on how you can
customize this tokenizer.

The tokenizer is provided by the
[`DoubleQuotedTokenizer`](../Tokenizers/DoubleQuotedStringTokenizer.cs) class.  By default,
it reports tokens it parses.

This tokenizer produces instances of the [`StringToken`](../Tokens/StringToken.cs) token
class.

### The Identifier Tokenizer

The identifier tokenizer is used to parse linguistic identifiers.  This would include things
like variable names or words that are virtual keywords but that you don't want the parser
to treat as keywords.

To specify what constitutes an identifier, you provide a string containing the characters
that can be part of an identifier.  The tokenizer allows you to provide different character
sets for the first character of an identifier vs. those that may be a part of an identifier.
For example, no language allows digits to start an identifier since that will produce too
much ambiguity.  But, digits **are** allowed to be part of an identifier after the first
character.

If you don't provide a set of characters that can start an identifier, it will default to
the collection of Latin letters, both upper- and lower-case and the underscore.  If you don't
provide a set of characters that can make up the rest of an identifier, it will default to
the starter characters plus the digits, `0` to `9`.

You can configure the style of identifier the tokenizer will work with, represented by the
[`LetterCaseStyle`](../Tokenizers/LetterCaseStyle.cs) enum.

- `AsIs` -- Identifiers will be treated as case-sensitive.  The text in the resulting ID
  tokens will be in the exact case as parsed from the source.  This is the default.
- `LowerCase` -- Identifiers will be treated as case-insensitive.  The test in the resulting
  ID tokens will be forced to lower-case.
- `UpperCase` -- Identifiers will be treated as case-insensitive.  The test in the resulting
  ID tokens will be forced to upper-case.

The identifier tokenizer should be added to a lexical parser after any keyword tokenizer.
Otherwise, the identifier tokenizer will likely capture your keywords as identifiers before
the keyword tokenizer gets a chance to parse keywords.

The tokenizer is provided by the [`IdTokenizer`](../Tokenizers/IdTokenizer.cs) class.  It
provides a number of constants to help making strings of characters easier.  The `Style`
property controls the style that the tokenizer will use.  By default, the tokenizer reports
tokens it parses.

This tokenizer produces instances of the [`IdToken`](../Tokens/IdToken.cs) token class.

### The Keyword Tokenizer

The keyword tokenizer is used to parse linguistic keywords, or _reserved_ words.  This would
include things like the `if`, `for`, `switch`, etc. keywords from C-like languages or
`SELECT`, `INSERT`, `CREATE`, etc. keywords from SQL.  Whether you even want to support the
concept of keywords in your DSL will depend greatly on the specifics of the language but
this tokenizer is here if you do need it.

A keyword tokenizer requires only a list of words to treat as keywords.  Given their nature,
keywords may only be made up of letters of either case.  It is an error to provide any other
character to the tokenizer.

You can configure the style of keyword the tokenizer will work with, represented by the
[`LetterCaseStyle`](../Tokenizers/LetterCaseStyle.cs) enum.

- `AsIs` -- Keywords will be treated as case-sensitive.  The text in the resulting keyword
  tokens will be in the exact case as parsed from the source.  This is the default.
- `LowerCase` -- Keywords will be treated as case-insensitive.  The test in the resulting
  keyword tokens will be forced to lower-case.
- `UpperCase` -- Keywords will be treated as case-insensitive.  The test in the resulting
  keyword tokens will be forced to upper-case.

If you include the identifier tokenizer when configuring your parser, and that tokenizer
allows for letter-only identifiers, be sure to create and configure the keyword tokenizer
first.  Otherwise, the identifier tokenizer will capture your keywords; they will be
reported as identifiers instead of keywords.

The tokenizer is provided by the [`KeywordTokenizer`](../Tokenizers/KeywordTokenizer.cs)
class.  The `Style` property controls the style that the tokenizer will use.  By default,
the tokenizer reports tokens it parses.

You may use the `Including()` and `Excluding()` methods to add to, or remove from, the
original list of keywords.

This tokenizer produces instances of the [`KeywordToken`](../Tokens/KeywordToken.cs) token
class.

### The Number Tokenizer

The number tokenizer is used to parse numeric literals.  This includes integral literals,
decimal literals and scientific notation literals.  You can control which of these formats
are recognized.  You can also control whether the numeric literal is allowed to include a
leading sign (`+` or `-`); this is disabled by default since it's generally cleaner to
treat those as unary, prefix operators.

The tokenizer is provided by the [`NumberTokenizer`](../Tokenizers/NumberTokenizer.cs)
class.  By default, it reports tokens it parses.  You may use these properties to control
the behavior of the tokenizer:

- Use the `SupportLeadingSign` property to control whether the tokenizer will consume a
  leading sign, `+` or `-`, as part of the numeric literal.  This is `false`, by default.
- Use the `SupportFraction` property to control whether the parsed numeric literal may
  include a fractional part; i.e., a decimal point followed by digits.  This is `true`, by
  default.
- Use the `SupportScientificNotation` property to control whether the parsed numeric
  literal may include an exponential part; i.e., something like `e1.5`.  If this is enabled,
  the `e` can be either upper- or lower-case.  This property is ignored unless the
  `SupportFraction` property is set to `true`.  It is `true`, by default

If you set `SupportLeadingSign` to `true` and you want to include an operator tokenizer
that includes either `+` or `-`, be sure to include the number tokenizer ahead of the
operator tokenizer.  Otherwise, the `SupportLeadingSign` property will become meaningless.

This tokenizer produces instances of the [`NumberToken`](../Tokens/NumberToken.cs) token
class.  The token provides two value properties, `IntegralNumber`, which is a `long` and
`FloatingPointNumber`, which is a `double`.  Only one of these will be set; use the
`IsFloatingPoint` property to decide which value property to access.

### The Operator Tokenizer

The operator tokenizer is used to parse linguistic operators.  This would include things
like the `+`, `*`, `>=`, `++`, etc. operators from C-like languages.  Some punctuation
like `:`, `;` and `,` are also considered operators by default.  This is a side effect of
the wide variety of symbols and strings that represents operators across many languages.

An operator tokenizer requires only a list of strings to treat as operators.

If you include the number tokenizer when configuring your parser, and that tokenizer is
configured to accept leading signs, and the operator tokenizer includes the `+` and `-`
as operators, be sure to create and configure the operator tokenizer after the number
tokenizer.  Otherwise, the operator tokenizer will capture the `+` and `-` operators and
the signs will never be recognized as part of numeric literals.

The tokenizer is provided by the [`OperatorTokenizer`](../Tokenizers/OperatorTokenizer.cs)
class.  By default, it reports tokens it parses.

You may use the `Including()` and `Excluding()` methods to add to, or remove from, the
original list of operators.

This tokenizer produces instances of the [`OperatorToken`](../Tokens/OperatorToken.cs)
token class.  It provides a number of constants that define all the default operators as
tokens suitable for comparison.

### The Single Quoted String Tokenizer

The single-quoted string tokenizer is a convenience subclass of the
[`StringTokenizer`](../Tokenizers/StringTokenizer.cs) class.  It configures a string
tokenizer that uses the single quote, `'` as the bounding character.  The resulting
tokenizer follows the escaping rules used by most languages that support single-quoted
string literals.

See the [string tokenizer](#the-string-tokenizer) section for more details on how you can
customize this tokenizer.

The tokenizer is provided by the
[`SingleQuotedTokenizer`](../Tokenizers/SingleQuotedStringTokenizer.cs) class.  By default,
it reports tokens it parses.

The tokenizer adds one additional property, `RepresentsCharacter`, that controls whether
the tokenizer will expect a single-quoted string to represent a single character, as in
C-like languages.  It defaults to `true`.  If set to `false`, single-quoted strings may be
of any length.

This tokenizer produces instances of the [`StringToken`](../Tokens/StringToken.cs) token
class.

### The String Tokenizer

The string tokenizer is used to parse string literals.  What character or string that is
used to mark out the string literal is completely up to you.  There are three subclasses
of the string tokenizer that configures the base string tokenizer to conform to the most
common string literal forms and provide some minor functional enhancements.

- Use the [`SingleQuotedStringTokenizer`](#the-single-quoted-string-tokenizer) class for
  supporting single-quoted string literals.  It also allows you to specify whether that
  literal should be interpreted as a character literal.
- Use the [`DoubleQuotedStringTokenizer`](#the-double-quoted-string-tokenizer) class for
  supporting double-quoted string literals.
- Use the [`TripleQuotedStringTokenizer](#the-triple-quoted-string-tokenizer) class for
  supporting triple-quoted string literals.  It also allows you to specify whether the
  number of double-quote characters that make up the string marker can be extended the
  way the triple-quoted literals support in C#.

The tokenizer is provided by the [`StringTokenizer`](../Tokenizers/StringTokenizer.cs)
class.  By default, it reports tokens it parses.  You may use these properties to control
the behavior of the tokenizer:

- Use the `Raw` property to control whether escape processing is applied to the content of
  parsed string literals.  This is `false`, by default.
- Use the `RepeatBounderEscapes` property to control whether the bounder string may be
  escaped (so that it can be included in a literal) by repeating it.  This is the same
  escaping style used in SQL.
- Use the `IsMultiLine` property to control whether a string literal can cross more than one
  line.  The triple-quoted string tokenizer sets this to `true`.  It is `fals` by default
  in all other cases.
- Use the `EscapeCharacter` property to control what character should be used to note
  escape sequences in non-raw string literals.  This defaults to the backslash, `\`,
  character.
- Use the `EscapeResolver` property to control how escape sequences are converted to
  characters.  This defaults to a resolver that resolves the most common escape sequences.
  You may customize this by providing your own subclass of the `EscapeResolver` class that
  implements the behavior you want.

This tokenizer produces instances of the [`StringToken`](../Tokens/StringToken.cs) token
class.

### The Triple Quoted String Tokenizer

The triple-quoted string tokenizer is a convenience subclass of the
[`StringTokenizer`](../Tokenizers/StringTokenizer.cs) class.  It configures a string
tokenizer that uses three double quotes, `"""` as the bounding text for a string literal.
The resulting tokenizer follows the escaping and multi-line rules used by most languages
that support triple-quoted string literals.

See the [string tokenizer](#the-string-tokenizer) section for more details on how you can
customize this tokenizer.

The tokenizer is provided by the
[`TripleQuotedTokenizer`](../Tokenizers/TripleQuotedStringTokenizer.cs) class.  By default,
it reports tokens it parses.

It adds one additional property, `MarkerCanExtend`, that controls whether the tokenizer
will allow the bounding series of double-quote characters to be extended to more than
three.  If enabled, then more than three double-quote characters may be used to bound the
literal so that three consecutive double-quotes may be contained in the literal.  This is
the same rule allowed in C# triple-quoted strings.  This, and multi-line, behaviors are
both enabled by default.

This tokenizer produces instances of the [`StringToken`](../Tokens/StringToken.cs) token
class.

### The Whitespace Tokenizer

The whitespace tokenizer is used to parse whitespace.

The tokenizer is provided by the [`WhitespaceTokenizer`](../Tokenizers/WhitespaceTokenizer.cs)
class.  By default, it does not report tokens it parses.  You may use the
`ReportLineEndsSeparately` property to control whether line ends are reported separately
from other whitespace.  By default, this is `false`.

This tokenizer produces instances of the [`WhitespaceToken`](../Tokens/WhitespaceToken.cs)
token class.
