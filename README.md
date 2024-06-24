## Lex - A Programmable Lexical Parser

This repo contains the C# port of my **Lex** library.  I've been using it for decades (yes,
that long) as the basis for almost all DSL or source code parsing work I've needed to do.  I
hope you will find it useful.  See the [History](docs/history.md) page if you're interested in
how it evolved.

### Quick Start

Here's an example of creating a parser and parsing a source.

#### Clause-Centric

```csharp
Dsl dsl = LexicalDslFactory.CreateFrom("/* Your DSL specification goes here as a string. */");
using LexicalParser parser = dsl.CreateLexicalParser();
string sourceText = "/* The source you want to parse goes here. */");

parser.SetSource(sourceText.AsReader());

while (!parser.IsAtEnd())
{
    Clause clause = dsl.ParseNextClause(parser);

    // Process the clause
}
```

#### Token-Centric

```csharp
using LexicalParser parser = LexicalParserFactory.CreateFrom("""
    standard comments
    keywords 'true', 'false', 'null'
    identifiers
    numbers
    single quoted strings
    double quoted strings
    bounders
    predefined operators
    whitespace
    """);
string sourceText = "/* The source you want to parse goes here. */");

parser.SetSource(sourceText.AsReader());

while (!parser.IsAtEnd())
{
    Token token = parser.GetNextToken();

    // Process the tokens...
}
```

### Overview

Fundamentally, the Lex library serves the purpose of breaking up a stream of characters
into a stream of tokens.  There is also support for breaking those up into _clauses_,
which are just a series of tokens, if the grammar of a DSL permits.  Sometimes, though,
the grammar is more context-sensitive and statements require coding to isolate properly.
Either way, Lex will give you an easy-to-use way of converting characters to tokens from
which you can infer the meaning of something written in your DSL, or even existing
mainstream languages.  The parser is completely programmable, so you can support any type
of token your DSL needs, in whatever priority makes sense.

Since it is very common, and fairly onerous to do yourself, Lex also provides the means
for parsing expressions into expression trees.

### What's It Good For?

Use Lex to parse almost any computing language that is coded in text:

- Parse existing mainstream languages like C#, Java, Rust, etc. to glean information such
  as data definitions, to transpile to other languages, and so forth.
- Implement your own DSL.
- Implement your own interactive command line front end (think tools like `psql` or
  command and control tools that provide a command line interface to a remote business
  server).

### Extensible

Lex deals with _typed_ tokens.  Each type of token has its own, dedicated parser or
_tokenizer_.  This doesn't have to be a one-to-one thing; in the library you'll see a couple
tokenizers that produce number tokens and several string tokenizers.

You'll find that the library contains all the standard tokenizers you'd expect from any
generalized programming language.  However, tokenizers are very easy to write, so if the
one you need doesn't exist, just write it yourself and include it in the list of tokenizers
the lexical parser should use.

This same pattern, as much as can be applied, is also in place for clauses.  Given its
nature, the expression support is more _configurable_ than extensible.  However, if the
expression parser doesn't suit you, there's nothing stopping you from writing your own
using the other aspects of the Lex library.

### Documentation

See the [full documentation](docs/README.md) for all the information you need to use Lex
effectively.
