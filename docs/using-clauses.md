## Using Clauses

This section describes how to construct clause graphs that may be used to easily parse
collections of related tokens.  In one sense, this is similar to implementing a BNF
grammar for your DSL.  Here, we build these graphs by hand.  The Lex library, of course,
provides a DSL for more easily specifying the grammar details of your DSL.  Refer to the
[Defining a Language](language-specification.md) section for details about the grammar DSL.

To help grasp how clause graphs can be used to capture a series of tokens, think about how
regular expressions work in text pattern matching.  The concepts, and some of the vocabulary,
are very similar.

### Overview

Ultimately, a _clause_ is nothing more than a series of tokens that represent a linguistic
concept.  A great deal of parsing involves what amounts to boilerplate code that isolates
and verifies these sequences.  Since this is such a common task, Lex provides direct support
for this.

The goal here is to "capture", or match, some number of tokens with some understanding of
what they represent.  This boils down to four types of clause matching.

- Match an individual token by matching against specific tokens or a type of token.
- Match a series of tokens; i.e., "token A must be followed by token B ...".
- Match one of a collection of child clauses; i.e., "this clause, that clause or the other
  clause".
- Repetitive clause matching; the clause may, or must, appear some number of times.

Once a series of tokens has been matched, or captured, you can then process that series to
do whatever they are meant to in your DSL.

When constructing a series style clause graph, you may associate a label, or tag, with the
series.  When constructing an "or" style clause graph, you also have the ability to
associate a tag with each child clause.  This allows you to know easily identify which
series or which child clause graph was matched without having to inspect the tokens
themselves.

You can mix and nest these types of clause parsers in any form that makes sense to support
your DSL's grammar.  You are free to structure your parsing as token first, with some
number of support clause parsers or start with clauses and then use more token first
parsing where necessary.

#### Clause Representation

The Lex library provides the [`Clause`](../Clauses/Clause.cs) class to represent a captured
series of tokens.  As you would expect, it contains the series of captured tokens along with
the tag, if the result was captured from an "or" clause parser.

All clause parsers produce an instance of this class as the result of capturing tokens.  If
the clause cannot capture any tokens, and no error message is defined for this condition,
then the clause parser will return `null`.

### A Clause of One Token

At the leaves of a clause parser graph will be the simplest clause matcher, one that
captures a single token.

To create a clause parser for capturing a single token, use the
[`SingleTokenClauseParser`](../Clauses/SingleTokenClauseParser.cs) class.  This class wraps
the [MatchToken()](the-parser.md#token-matchtokenbool-isrequired-funcstring-msgsource--null-params-token-tokens)
and [MatchTokenType()](the-parser.md#token-matchtokentypebool-isrequired-funcstring-msgsource--null-params-type-types)
and related methods on the lexical parser to allow them to act as a capturing clause.  This
means you can create a `SingleTokenClauseParser` to match against a set of specific tokens
or one or more token types.

You may also provide an error message.  If you do, then a token exception with that message
will be thrown when it cannot capture a token.

In this example, a clause parser is created that will capture any of the four basic
mathematical operators.

```csharp
ClassParser operatorParser = new SingleTokenClassParser(
    OperatorToken.Plus, OperatorToken.Minus, OperatorToken.Multiply, OperatorToken.Divide
);
```

Since no error message was provided, the `TryParse()` method will return `null` rather than
throw a token exception.

### A Sequence of Clauses

There will be many times when you will want to capture a series of clauses (remembering that
matching a single token is also done by a clause).  The support for this is provided by the
[`SequentialClauseParser`](../Clauses/SequentialClauseParser.cs) class.

This example creates a sequential clause for capturing a `WITH GRANT OPTION` clause.

```csharp
ClauseParser withGrantOption = new SequentialClauseParser()
    .Matching(WithKeywordToken)
    .Then("Expecting 'GRANT' to follow 'WITH' here.", GrantKeywordOption)
    .Then("Expecting 'OPTION' to follow 'GRANT' here.", OptionKeywordOption); 
```

A sequence is begun by passing the first clause to one of the `Matching()` methods.
Subsequent clauses should be passed to one of the `Then()` clauses.  Notice in the example
that once we are in a known context, we can provide error messages that will result in
token exceptions when som sub-clause of the series does not match.

Ultimately, the `SequenceClauseParser` deals only with a series of clause parsers.  The
`Matching()` and `Then()` overloads that accept tokens or token types are convenience
methods that wrap their arguments with instances of the `SingleTokenClauseParser` for you.
This helps to keep your code looking cleaner.

Where you can put error messages will depend a great deal on how your clause parser graph
is structured.  For example, if you have multiple `WITH` clauses, you probably don't want
an error message on the `GRANT` piece as it would prevent matching to any other `WITH`
clause.  There will be more on this in the next section.

### A Choice of Clauses

There will also be many times when you will want to capture one clause from a collection of
choices.  This is the "or" style alluded to earlier.  You could also liken it to C#'s
`switch` construct.  The support for this is provided by the
[`SwitchClauseParser`](../Clauses/SwitchClauseParser.cs) class.

This example creates a sequential clause for capturing a primitive type notation clause.

```csharp
ClauseParser primitiveTypes = new SwitchClauseParser()
    .Matching(BoolKeywordToken)
    .Or(IntKeywordToken)
    .Or(LongKeywordToken)
    .Or(FloatKeywordToken)
    .Or(DoubleKeywordToken)
    .OnNoClausesMatched("Unsupported type found.");
```

A switch parser is begun by passing the first clause to one of the `Matching()` methods.
Subsequent clauses should be passed to one of the `Or()` clauses.  There will be more times
than not, depending on your DSL, when the clause you pass will be a sequence clause and not
just a simple token.  The switch clause doesn't care.  When you add a child clause, you have
the option of including a tag to report when that child clause is matched.  You may also
provide an error message to use in throwing a token exception when none of the clauses match.

Ultimately, the `SwitchClauseParser` deals only with a collection of child clause parsers.
`Matching()` and `Or()` overloads that accept tokens or token types are convenience methods
that wrap their arguments with instances of the `SingleTokenClauseParser` for you.  This
helps to keep your code looking cleaner.

### Optional and Repeated Clauses

Sometimes you will want a clause to be optional or to be allowed to repeat.  Both cases
are supported by the [`RepeatingClauseParser`](../Clauses/RepeatingClauseParser.cs) class.
This class requires a clause to wrap and a minimum and maximum number of times the clause
may repeat.  The minimum defaults to 0, meaning the wrapped clause is not required to be
present at all.  The maximum defaults to no limit, allowing the clause to repeat any
number of times.

If you provide an error message when constructing a repeating clause parser, a token
exception will be thrown with that message if the minimum number of times the wrapped
clause should appear is not met.  Otherwise, the repeating clause parser will report that
was not able to capture any tokens.

This example wraps a clause to note that it is optional (since the `min` argument defaults
to 0).

```csharp
ClauseParser optionalClause = new RepeatingClauseParser(wrappedClause, max: 1);
```

This example wraps a clause to note that it must be specified at least once (since the
'max' argument defaults to no limit).

```csharp
ClauseParser optionalClause = new RepeatingClauseParser(wrappedClause, min: 1);
```

### Capturing Tokens

Once you've built any clause parsers you need, you'll use them to capture tokens.  You'll
do this by using the `TryParse()` method, passing it the Lex parser, properly configured
for the language involved.  One of three things will happen when you invoke this method.

1. The method will return a `Clause` object containing all the tokens that were captured
   along with any applicable tag.
2. The method will throw a token exception, if any clause parser under the graph rooted at
   the one being invoked has a configured error message and matching failed.
3. The method will return `null` if the clause parser, for any reason within the clause
   parser graph, could not capture tokens.  This will be true if no error messages were
   present to cause an exception to be thrown.

When a clause is returned, you can be guaranteed that it matches all the rules specified by
your clause parser graph.  All that's left is for you to interpret the tokens returned in
the clause.  That said, it's important to note that there will be times, because of how
your DSL needs to work, where you will need to implement further validation checks.  For
example, review the
[Based number tokenizer statement](parser-factory-dsl.md#the-based-number-tokenizer-clause)
in the Lex parser factory DSL.  There are 3 "no" clauses so the factory DSL language
specification declares that the "no" keyword must be followed by one of 3 other keywords
and that that clause may repeat up to 3 times.  The factory code itself, however, further
validates that each option that may follow the "no" keyword is specified only once.

### Debugging

As I'm sure you've realized, when clause parser graphs get complicated, it can become
onerous to figure out what went wrong when they don't work as you designed.  The clause
parsers give you some help with this.

To aid in debugging, you can give each clause parser its own name.  If you are careful to
make these unique across your set of clause parsers, you'll know exactly which one does
what when you turn debugging on.  Set the name of the clause parser by invoking the
`Named()` method.

You can enable or disable debugging for a clause parser by setting its `IsDebugging`
property to an appropriate value.  Note that setting this property will affect debugging
for _that clause parser only_.  If you want to alter the debugging setting for a clause
parser _and all its children_, use the `SetDebugging()` method.

Once you've enabled debugging, the clause parser will begin emitting instances of the
[`ClauseParserDebugInfo`](../Clauses/ClauseParserDebugInfo.cs) class.  By default, the
content of these are written to the console. If you want to deal with them yourself, set
the `DebugConsumer` property to an appropriate value.  Note that setting this property will
affect the debug information consumer for _that clause parser only_.  If you want to alter
the debug consumer for a clause parser _and all its children_, use the `SetDebugConsumer()`
method.
