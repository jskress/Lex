## Defining a Language

This section describes how to go about using the
[`LexicalDslFactory`](../Dsl/LexicalDslFactory.cs) class to create instances of the
[`Dsl`](../Dsl/Dsl.cs) class to represent a parseable language and how to use them.

See the [DSL Specification DSL](dsl-specification-dsl.md) section for details on defining
a language in Lex's DSL DSL.

### Overview

One way to conceive of your DSL, or any language you want to parse with Lex, is that its
definition is encapsulated by any collection of clause parsers, and, optionally, an
expression form, you might have along with your own code for interpreting tokens parsed
from a source.

There are a few reasons why you might want to formally define your DSL in the DSL factory
DSL provided by Lex.  The first is that the quality and expressiveness of a DSL will be
greatly increased by thinking through all the possible grammatical structures.  In other
words, the more you have to specify in Lex's DSL for DSLs, the more you will have naturally
thought through the side effects of your grammar.

if you need to parse expressions, it's way more straightforward to describe them in the DSL
than to use code to create an expression parser.

Similar to expressions, it's easier to specify the grammar of your overall DSL, or, at
least, most of the clauses it needs, in the DSL factory DSL.  This makes your DSL easier
to extend and maintain.

### The `Dsl` Class

Lex uses the [`Dsl`](../Dsl/Dsl.cs) class to represent a collection of clauses that,
collectively, describe a language.  All clause parsers are named, except one top-level
clause.  Once created by the DSL factory, the [`Dsl`](../Dsl/Dsl.cs) instance can then be
used to easily parse clauses and expressions (if you described what one looks like in the
DSL) from a source.

The [`Dsl`](../Dsl/Dsl.cs) class also makes available the collection of keywords and
operators you specify in the source specification of the language.  This allows you to
keep a single source for these lists between the language definition and those used by
the keyword and operator tokenizers you configure on your Lex parser if you don't use the
[`Dsl`](../Dsl/Dsl.cs) instance to create them.

#### Creating a Lexical Parser

You will use the `CreateLexicalParser()` method to create a lexical parser that is capable
of parsing the DSL.  The Lex library itself does nothing special to guarantee this.  Rather,
as part of your language's definition, you include the specification of the tokenizers
needed to parse your language.  The specification must be in the form of the
[lexical parser DSL](parser-factory-dsl.md).  See the
[lexical parser specification clause](dsl-specification-dsl.md#lexical-parser-specification)
documentation for details.

#### Parsing Clauses

You will use the `ParseNextClause()` and `ParseClause()` methods to capture the tokens for
a clause.  Using the `ParseNextClause()` will invoke the top-level clause defined in the
DSL.  It is an error to invoke this method when there's no top-level clause defined.  The
`ParseClause()` method is used to ask the named clause parser to try to capture a set of
tokens.

Let's assume you have a top-level clause parser.  Let's also assume you've defined a tag
for each of the switch clause parser choices (since things would be harder if you didn't
do this).  This, then, is an example of how your top-level parsing loop might look.

```csharp
Dsl dsl = LexicalDslFactory.CreateFrom("/* language spec goes here. */");

using LexicalParser parser = LexicalParserFactory.CreateFrom("/* parser spec goes here. */", dsl);

parser.SetSurce(/* the source to parse. */);

while (!parser.IsAtEnd())
    ProcessNextClause(dsl.ParseNextClause(parser));
```

#### Parsing Expressions

You will use the `ParseExpression()` method to parse an expression tree from the source.
See the [Using Expressions](using-expressions.md) section for full details on parsing
expressions.

#### Debugging

The [`Dsl`](../Dsl/Dsl.cs) class also lets you easily manipulate the debugging settings
for the clauses it carries.  Use the `SetDebugging()` and `SetDebugConsumer()` methods to
do this.  Refer to the section on [clause parser debugging](using-clauses.md#debugging) for details on debugging
clause parsers.
