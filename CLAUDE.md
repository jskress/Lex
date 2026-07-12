# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

**Lex** is a general-purpose, programmable lexical parser library for C# (a port of a Java
library the author has maintained for decades). It tokenizes character streams, groups tokens
into higher-level *clauses*, and parses *expressions* into expression trees. It is not tied to
any one language — consumers define their own DSLs against it (e.g. the author's `RayTracer`
project, in the sibling `../RayTracer` checkout, uses Lex for its `.igl` scene-description
language; see `docs/use-cases.md`). This repo ships as the `Lex` NuGet package (see
`Lex.csproj`).

## Build, run, test

```bash
dotnet build                                              # build Lex + Tests
dotnet test                                               # run the full MSTest suite
dotnet test --filter "FullyQualifiedName~ClauseDslTests"  # run one test class
dotnet test --filter "FullyQualifiedName~ClauseDslTests.SomeMethod"  # run one test method
```

Target framework is `net8.0`, `LangVersion` `preview`, nullable reference types disabled.
`Lex.csproj` excludes `Tests/**` from its own compile/resource items; `Tests/Tests.csproj`
references `Lex.csproj` directly and uses MSTest (`[TestClass]`/`[TestMethod]`).
`TestResults/` at the repo root accumulates many timestamped run directories from prior
`dotnet test` invocations — not source, ignore when exploring.

## Architecture

Lex has three layers, each usable independently, with each layer built on the one below:

```
Tokenizers/Tokens  -->  Clauses/Expressions  -->  Dsl (language specification)
(character stream)      (token grouping)          (declarative language definition)
```

### Layer 1: Tokenizing (`Tokenizers/`, `Tokens/`, `Parser/`)

`LexicalParser` (`Parser/LexicalParser.cs`) turns a `StreamReader` source into a stream of
`Token`s by delegating to a registered list of `Tokenizer`s (comments, identifiers, keywords,
numbers — including based/scientific forms — operators, bounders, single/double/triple-quoted
strings, whitespace). Each tokenizer type produces its own `Token` subclass (`Tokens/`) that
carries the parsed *value*, not just raw text (e.g. `NumberToken` exposes the converted
numeric value, `StringToken` exposes the value with escapes already resolved). Tokenizers are
registered directly onto a `LexicalParser` instance in priority order, or built declaratively —
see Layer 3.

### Layer 2: Clauses and expressions (`Clauses/`, `Expressions/`)

A *clause* is a named grouping of tokens representing one grammatical concept (`Clauses/`):
- `SingleTokenClauseParser` — matches one token (by instance or type).
- `SequentialClauseParser` — matches an ordered "and" sequence of terms (`.Matching(...).Then(...)`).
- `SwitchClauseParser` — matches an "or" choice of terms (`.Matching(...).Or(...)`), with each
  choice optionally tagged so a dispatcher can tell which alternative fired.
- `RepeatingClauseParser` — wraps another clause parser with min/max repetition counts.

All implement `ClauseParser.TryParse()` returning a `Clause` (tag + captured `Token`s), and
support debugging via `IsDebugging`/`DebugConsumer` (`ClauseParserDebugInfo`). Clause parsers
can reference each other, including recursively/self-referentially, which is why many are
constructed as `static readonly` fields first and wired together afterward (see the static
constructor pattern in `Dsl/LexicalDslFactory.cs`).

Expressions (`Expressions/`) are a separate, more configurable (not extensible-via-subclassing)
mechanism for parsing operator/term trees with precedence and associativity
(`OperatorPrecedence`, unary/binary/trinary operation term types). `ExpressionParser` drives the
parse; you plug in an `IExpressionTreeBuilder` (default: `DefaultTreeBuilder`) to control how
parsed terms/operators are assembled into your own expression-tree node types — this is the
main extension point when embedding Lex's expression support in a consuming DSL.

### Layer 3: Declarative DSL definition (`Dsl/`)

Two built-in DSLs, both self-hosted (i.e. defined and parsed using Lex itself), drive this
layer; their grammars live as string constants in the repo root (`parser-dsl.syntax`,
`dsl-dsl.syntax`) and are documented in `docs/parser-factory-dsl.md` and
`docs/dsl-specification-dsl.md`:

- **Lexical parser factory DSL** (`Dsl/LexicalParserFactory*.cs`, one file per tokenizer kind,
  spec in `Dsl/LexicalParserFactory.DSL.cs`): a compact textual spec (e.g. `"standard comments
  \n keywords 'true' \n identifiers \n numbers \n ..."`) that `LexicalParserFactory.CreateFrom`
  turns into a configured `LexicalParser` without hand-wiring tokenizer objects.
- **DSL specification DSL** (`Dsl/LexicalDslFactory*.cs`): a richer spec language for defining
  an entire consumer language — keywords, operators, expression term/operator forms, and named
  sequential (`{ ... }`) / switch (`[ ... ]`) clauses with repetition (`{n..m}`) and tagging
  (`=> "tag"`) — that `LexicalDslFactory.CreateFrom` compiles into a `Dsl` instance.

`Dsl` (`Dsl/Dsl.cs`) is the resulting handle consumers use at runtime: it holds the named clause
parsers plus the collected keyword/operator token lists, exposes `CreateLexicalParser()`,
`ParseNextClause()`/`ParseClause(tag)`, and `ParseExpression()`. `DslParsingContext` is the
scratch state (`Variables` pool mapping spec names to tokens/types/clause parsers, plus
collected token lists) used while compiling a DSL specification into these runtime objects.

### Typical consumer flow

1. Define the language (parser spec + grammar) as a DSL-spec-DSL string, or build a `Dsl`
   programmatically from `ClauseParser`/`ExpressionParser` pieces directly.
2. `dsl.CreateLexicalParser()` to get a `LexicalParser`, then `parser.SetSource(...)`.
3. Loop `dsl.ParseNextClause(parser)` (or `ParseClause(parser, tag)` for specific clauses),
   dispatching on `Clause.Tag` to interpret each clause into the consumer's own model — this is
   the pattern `RayTracer`'s `LanguageParser` follows for its `.igl` files.

## Documentation

`docs/README.md` is the documentation index; start there for anything not covered above
(`the-basics.md`, `the-parser.md`, `the-tokenizers.md`, `parser-factory-dsl.md`,
`language-specification.md`, `dsl-specification-dsl.md`, `using-clauses.md`,
`using-expressions.md`, `extending-lex.md`, `use-cases.md`).
