## Release Notes

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
