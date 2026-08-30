## Release Notes

### 1.4.0

- Fixed the keyword tokenizer reporting a keyword that it had taken out of the middle of a
  longer word.  It matched its keywords against the character stream directly, so with `flaw`
  as a keyword, `flaw2` came out as the keyword `flaw` followed by the number `2` rather than
  as the one identifier that was written; `flaw_x` went the same way.  Often enough the result
  was not a wrong token but a hard error: with `in` and `int` as keywords, `int8` lexed as the
  keyword `int` followed by an `8` that nothing would then accept.  The tokenizer now reads the
  whole word first and only then looks it up, which is how keywords are meant to be lexed and
  how the original Java version of this library did it.
- Where a word ends is now the identifier tokenizer's business.  The keyword tokenizer asks the
  `IdTokenizer` registered with the same parser what an identifier may start with and contain,
  rather than going by its own notion of a letter, so custom identifier alphabets are honored:
  tell the identifier tokenizer that a `$` may appear in an identifier and `flaw$x` is one
  identifier here too.  It is looked up when needed, so either registration order works.
  - Letters still count as part of a word whatever the identifier alphabet says, so narrowing
    what an identifier may be does not cost you your keywords.  With no identifier tokenizer
    registered, a word is just a run of letters, which leaves such parsers lexing exactly as
    they did.
- Keyword casing and identifier casing remain entirely separate settings; nothing here changes
  how either is applied.
- Added `IdTokenizer.Starters` and `IdTokenizer.Members` so the characters an identifier was
  configured with can be read back.  `FixedListTokenizer` now offers its possibilities to
  subclasses through a protected `Possibilities` property.

### 1.3.0

- A clause that has already consumed an expression can now be backed out of.  Previously, if
  a term after the expression failed to match, the clause could not restore the token stream
  -- an expression hands back a term, not the tokens that went into it -- so it threw
  "Syntax error near here." instead of reporting no match.  That made a switch clause unable
  to try an alternative containing an expression and fall through to the next one, so
  `[ interval | _expression ]` could never tell `(0, 3]` from `(balls)`.  This was previously
  described as a limitation in [the DSL specification docs](dsl-specification-dsl.md#the-term-clause),
  along with the advice to put an error message on every term following an expression; that
  advice is no longer necessary, though it still works, since a term with an error message
  remains a hard error.
  - Two things to be aware of.  A term that carries an explicit error message still throws
    rather than backtracking, so grammars written to the old advice behave exactly as they
    did.  And giving tokens back does not undo work done while reading them: an
    `IExpressionTreeBuilder` will already have been asked to build the terms of an expression
    that is then abandoned, so keep such builders free of side effects beyond building the
    term.
- Added `LexicalParser.MarkPosition()`, `RollbackToMark()` and `ReleaseMark()` for marking a
  spot in the token stream and returning to it.  This is what the above is built on, and it's
  the tool to reach for in your own clause parsers when `ReturnTokens()` isn't enough --
  notably when a sub-parser consumed tokens it cannot hand back, whether into an expression
  or into a suppressed term choice.  Marks nest, and each must be resolved exactly once.  See
  [The Parser](the-parser.md).
- The rollback covers everything the attempt consumed, not merely what the clause itself got
  back, so tokens a term choice was told to suppress are restored along with the rest.

### 1.2.1

- Bug fixes:
  - Fixed expressions that mix operators of equal precedence with a higher-precedence
    operator between them being grouped right-to-left instead of left-to-right.  Reducing the
    pending operators stopped after a single reduction, which could leave two equal-precedence
    operators adjacent, and the final pass then grouped that pair from the right.  For
    example, `1 - 2 / 2 - 3` parsed as `1 - ((2 / 2) - 3)`, which evaluates to -1, instead of
    `(1 - (2 / 2)) - 3`, which evaluates to 3.

### 1.2.0

- Added the [`ClauseDispatcher`](../Clauses/ClauseDispatcher.cs) and
  `ClauseDispatcher<TResult>` classes for routing a captured `Clause` to one of several
  handlers based on its tag.  This is the same tag-dispatch pattern Lex already used
  internally to interpret its own DSL specifications; it's now available so you don't have
  to build it yourself for your own DSL.  See
  [Dispatching Clauses](using-clauses.md#dispatching-clauses).
- Added the [`ClauseReader`](../Clauses/ClauseReader.cs) class (get one via a clause's new
  `Reader()` method) for sequentially reading a captured clause's tokens and expressions —
  `NextToken()`, `NextText()`, a type-checked `NextToken<T>()`, `SkipIfNextTextIs()`, and
  the equivalents for expressions — instead of hand-indexing into `Tokens`/`Expressions`.
  See [Reading Captured Clauses](using-clauses.md#reading-captured-clauses).
- Added the [`FailureTracker`](../Parser/FailureTracker.cs) class and a matching
  `LexicalParser.FailureTracker` property.  Attach one before parsing and it will track the
  furthest point reached, and what was expected there, across every failed match attempt
  made by `SingleTokenClauseParser` — letting you build a good default error message for a
  failed top-level parse without hand-annotating every clause in your grammar with its own
  error message.  Also added `LexicalParser.Line`/`Column` properties (mainly useful for
  this, and other diagnostics, when there's no token to report a position from).  See
  [Diagnosing Match Failures](using-clauses.md#diagnosing-match-failures).
- `RepeatingClauseParser` now detects a wrapped clause parser that reports a match without
  consuming any input and throws a clear error instead of looping forever (or, for a
  bounded repeat, silently accepting a phantom match).  This guards against a whole class
  of bugs in custom clause parsers, not just the one described below.
- The null-on-no-match contract that every clause parser is expected to follow is now
  spelled out explicitly in the `ClauseParser.TryParse()`/`TryParseClause()` doc comments,
  and is covered by new tests across the built-in clause parser types.
- Bug fixes:
  - Fixed the default escape resolver turning `\'` into the two characters `\` and `'`
    instead of a single `'`, which broke single-character literals like `'\''`.
  - Fixed `ExpressionClauseParser` reporting a successful, non-null match even when nothing
    was actually parsed (i.e., when it was optional and no expression was present).  Besides
    polluting results with a stray `null` expression, this could hang an unbounded repeat of
    an optional expression clause (e.g., `_expression{*}`) in an infinite loop.
  - Fixed expression term specs with a minimum of zero (`_expression(*)`, `_expression(?)`)
    throwing "Expecting a term here." instead of succeeding with zero expressions when none
    were present.
  - Fixed `ClauseParserDebugInfo` capturing the offending token when a clause was
    successfully matched instead of when it was rejected, so debug output for a rejected
    clause never actually showed what was rejected.
- Nullable reference types are now enabled for the whole library.  This shouldn't affect
  compiled consumers (nullable annotations don't change the underlying types), but if you
  have nullable warnings enabled yourself, you may see a couple of new ones: most notably,
  if you have your own `IExpressionTreeBuilder` implementation, its `CreateTerm()` method's
  `tag` parameter is now correctly annotated as nullable.
- Minor internal cleanup with no observable effect: fixed a couple of typos (an internal
  field name, a duplicate character in the hex-escape character set), removed some dead
  code in the tokenizer's main loop, and moved off `LangVersion: preview` to the stable
  language version for `net8.0`.  Test/build dependencies (`Microsoft.NET.Test.Sdk`,
  `MSTest`, `coverlet.collector`) were also updated to their latest versions.

### 1.1.4

- Fixed two issues with switch clause tagging.
- Fixed an issue with parsing expression terms that contain nested expressions.  Yes, it
  includes a new test.
- Fixed an issue with parsing expression terms that contain nested expressions.  Yes, it
  includes a new test.
- Fixed an issue with parsing expressions through clauses.  A new test was added to make
  sure things work right.
- Fixed an issue with expression clause parsers not being properly created.
- Fixed an issue with using multiple types of string tokenizers in the parser DSL.
- Fixed an issue where a simple number was not allowed in a repeat clause.

### 1.1.3

- Found a better way to support self- or cross-referencing clauses that doesn't require
  forward declarations.

### 1.1.2.1

- Fixed an issue with self- or cross-referencing clause definitions to the DSL DSl.

### 1.1.2

- Added support for self- or cross-referencing clause definitions to the DSL DSl.

### 1.1.1

- Added support for Greek letters to be part of identifiers as they can be common in some
  DSLs.

- The `CanStart()` method on tokenizers is meant to be protected, not internal.  Without
  that, custom tokenizers outside the library cannot be written.

### 1.1.0.1

- Minor bug fix relating to wrapping an expression clause with a repeating clause.

### 1.1.0

- Added support for an expression clause, allowing expressions to be expected in the
  midst of other clauses.

- Addition of the [Use Cases](use-cases.md) page to the documentation.

### 1.0.0

The initial version of the library, at least from the point of view of C#; see
[History of the Library](history.md) if you're curious about more details.
