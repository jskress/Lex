## The Basics

Here, we'll describe the things you need to know for Lex to make sense.

### Semantic vs. Lexical Parsing

On the surface, parsing source code sounds easy.  Ultimately, the goal is to derive some
sort of semantic meaning from a stream of characters.

In written languages, meaning, or its _semantics_, is expressed in the form of paragraphs,
which are made up of sentences, which, in turn, are made up of words which, finally, are
made up of individual symbols.  In general, we can say that the act of inferring meaning
from a stream of symbols in a written document is the work of identifying sensible items
which have some core meaning and grouping those items in a way that conveys an intended
meaning.  (I use the word, "sensible" here to emphasize that things like misspellings can
detract from meaning.  And yes, I understand that glyph-based languages don't completely
operate this way.)

In written languages, these fundamental items are words.  Each language has a dictionary,
or _lexicon_ (hence, _lexical parsing_), that specifies the core meanings of those words.
This leads us to the idea that to understand a written sentence requires both identifying
the words that make up that sentence and then understanding what meaning is conveyed by
the collection of words, based on their individual meanings.

In computer languages, we have the same process.  Some string or file contains a series of
characters that are supposed to mean something; declaring variables or executing actions.
To work out what that meaning is requires identifying the computer language's "words",
typically called _tokens_, and then understanding what those tokens collectively represent.

The Lex library is built on the idea that separating the lexical parsing (inferring tokens
from characters) from the semantic parsing (discovering the intent of source expressed in
the language) is a worthwhile thing to do.  (If you don't agree with that, you might as well
quit Lex now.)  Every time I've tried to implement semantic parsing without separating it
from lexical parsing, things have gotten horribly complicated.  This complexity usually
stems from things like the read-ahead ability most parsers need to account for the
context-sensitive nature of most languages and/or string literal parsing and the escaping
and bounding rules that can come with that.  Allowing for comments can also complicate
matters.  This is especially true if what you want to parse isn't a DSL of your own
invention but some other established language like C#, Java or SQL.

### Types of Tokens

The process of tokenizing a stream of characters isn't much more than identifying the series
of subsets of those characters that represent each relevant token.  That said, Lex provides
some additional pieces of information about each token it discovers.

- The token's location, expressed as one-based line and column numbers.

  This is primarily useful for reporting errors to the end user so that they know where to
  look in the source for any problems.

- The type of token that was found.

  Semantic parsing often relies on knowing what the token represents. such as a string
  literal, a number, an operator, comments, whitespace, etc.

- The value of the token.

  By this, I mean the value of the token as understood by the represented language.    As
  an example, the token type that Lex uses for numeric tokens provides the integral or
  floating point value the characters in the token represent, already converted for you.
  Similarly, the string token type contains the value for the token after the bounders are
  removed and any escaping rules have been applied.

Parsing some of those types of tokens can have interesting challenges.  Consider the
variety of forms string literals can take across all the languages you know.  There are
often different forms of escaping rules as well so that the character used to bound a
string literal may appear inside the string or to include characters that cannot be easily
typed.  The same can be said of numeric and even comment tokens.  Lex parsing is easily
configured to account for many of these situations.

### Clauses

Even though Lex, at its core, creates tokens from character streams, much of what it provides
are some things common across most parsing situations.  These things will help you on the
semantic side of parsing.  Many of these are based on the concept of a _clause_.

There is a spectrum along which any given computing language or DSL will fall, which might
be called _verbosity_, or wordiness.  C-like languages, which are primarily symbol-centric
and functional in declaration style, would be at one end and SQL-like languages, which are
more word-, or even sentence-centric, would be at the other.  The closer you get to the
latter end, the more probable that you'll have multiple tokens to represent a single concept.
For example, the phrase, `WITH GRANT OPTION` in SQL is a single concept represented by 3 tokens.

Since a clause is really nothing more than a series of tokens that collectively represent
some higher-level concept, and that this is a common concept across almost all languages,
Lex provides you the tools to isolate and validate such clauses easily.

Most DSLs will fall somewhere in the middle on the above spectrum.  As such, you'll probably
find that you'll use a mix of low-level token handling from the lexical parser directly **and**
higher-level clauses from a DSL specification at the same time.  The library is built to
make this as easy and straightforward as possible.

### Expressions

If the language you want to support performs any sort of math or math-like operations such
as string concatenation using operators, you'll end up needing to parse _expressions_.

Parsing expressions goes beyond the isolation of tokens and inferring meaning.  Even once
you've done that for an expression, there's still the matter of correctly understanding
how that series of tokens that make up te expression should be processed.  To help with
this, the Lex library provides support for not only isolating and validating an expression's
tokens but also converting them into an _expression tree_.

To make this (hopefully) clearer, let's take a very simple example of a mathematical
expression:

```csharp
1 + 2 * 3
```

The result of this, is `7`, because we understand about operator precedence and how it
affects evaluation order.  Now, compare that to this one:

```csharp
(1 + 2) * 3
```

We know this one will evaluate to `9` because we've overridden the evaluation order.

Most compilers, when they process expressions like these, create a tree of operations that
take into account things like parentheticals and precedence.  As you've probably guessed
(or even experienced), parsing this in an intelligent way for any arbitrary expression of
operators and the terms they act on can get really hard to get right.  This is especially
true when terms can be multi-token things like function/method calls, which involve parsing
out sub-expressions, arrays and other such constructs.

Since the ideas of unary, binary and even trinary operations and the order and precedence
rules that govern their execution, are pretty universal across languages, Lex provides you
the means for going from the character stream of your source, processing and validating
the proper tokens in the right way for an expression and then working with your code, as
needed, to build a representative expression tree.  This will keep your DSL code much more
to the point of what the DSL is supposed to do.
