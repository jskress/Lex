## Extending Lex

This section describes how you can extend what the Lex library can do.

### Tokenizers and Tokens

Although Lex comes with a fairly complete collection of tokenizers and tokens, you may
come across situations where you want, or need, to something different.  You've probably
noticed that there is not a 1:1 correspondence between tokenizers and types of tokens.  So,
if you have a need for a new type of tokenizer, you won't necessarily need to produce a
corresponding type of token.

#### Types of Tokens

If you do need to provide your own type of token, you need only create a class to represent
it that extends the [`Token`](../Tokens/Token.cs) class provided by Lex.  Upon construction,
your class will need to provide a string to the parent constructor, usually by accepting a
string parameter itself.

If your token type should represent a value other than text, consider reviewing the
[`NumberToken`](../Tokens/NumberToken.cs) class as a pattern to follow.

#### Tokenizers

If you need to provider your own tokenizer, you will need to create a class that extends
the [`Tokenizer`](../Tokenizers/Tokenizer.cs) class provided by Lex.  The tokenizer base
class provides you with a `StringBuilder` that is used as a buffer when parsing a token,
access to the controlling lexical parser and the `ReportTokens` property.

At the very least, you will need to provide implementations for the `CanStart()` and
`ParseToken()` abstract methods.  Both methods are passed the most recent character that
the lexical parser read from its source.  Tokenizers are expected to be _reluctant_
character consumers; that is, they should consume the least possible characters required
to create a single token.

The `CanStart()` method must decide whether it has the ability to parse a token from the
source character stream, including the character passed to it as the first one it should
look at.  You are free to look ahead as many characters as required to make this decision,
but it is vital that, when you return from `CanStart()`, the input character stream is in
the exact same state as when `CanStart()` was called.  You should use the `Read()` and/or
`Peek()` methods provided by the [`Tokenizer`](../Tokenizers/Tokenizer.cs) base class to
read more characters and use the `ReturnChar()` and/or `ReturnBuffer()` methods to put
characters back into the source character stream.

You are free to use the `Builder` property to buffer up characters as needed, though you're
more likely to use it while actually parsing the token.

The `ParseToken()` method should do exactly that.  It must consume exactly the characters
required to make up a single token and then package those characters in an appropriate
subclass of the [`Token`](../Tokens/Token.cs) class as the result of this method.  It is
not allowed for this method to return `null`.

### Clauses

It is less likely that you would need to create your own clause parsers.  However, if you
do need to, you will need to create a class that extends the
[`ClauseParser`](../Clauses/ClauseParser.cs) class provided by Lex.  This base class takes
care of supporting debugging.

You will need to provide an implementation of the `TryParseClause()` abstract method.  In
a manner similar to tokenizers, it's important that your clause parser properly manage the
underlying token stream.  They should be _reluctant_ token consumers.

As with the clause parsers provided by the Lex library, there are three possible results
from calling the `TryParseClause()` method.

- The clause parser is able to match a series of tokens.  In this case, the tokens should
  be removed from the lexical parser's token stream and returned in an instance of the
  [`Clause`](../Clauses/Clause.cs) class.
- the clause parser is unable to match a series of tokens **and** it is not configured to
  report an error.  In this case, the underlying token stream should be restored to its
  original state as of when `TryParseClause()` was called.  The lexical parser class
  provides you a `RetornTokens()` method to help with this.
- The clause parser is unable to match a series of tokens **and** it should report an error.
  In this case, the underlying token stream should be restored to its original state as of
  when `TryParseClause()` was called.  Then, a `TokenException` should be thrown with an
  appropriate error message and referring to the first token the `TryParseClause()` saw.

### Expression Tree Builders

If you make use of Lex's support for expression parsing, you'll notice that the expression
parser uses a default tree builder.  You are free to use this as-is, but depending on what
you want to do with parsed expression trees, you may find it more efficient to declare your
own set of classes to represent your parsed trees.

To support this, all you need to do is set an instance of a class that implements the
[`IExpressionTreeBuilder`](../Expressions/IExpressionTreeBuilder.cs) interface.  Lex does
require that all types of terms, whether they represent literals, binary operations,
function calls or whatever, all be tagged with the
[`IExpressionTerm`](../Expressions/IExpressionTerm.cs) interface.

The expression parser guarantees that child terms and operations are all ordered properly
based on order of operation rules.

The [`IExpressionTreeBuilder`](../Expressions/IExpressionTreeBuilder.cs) requires
implementations of the following four methods.

#### The `CreateTerm()` Method

This method is called whenever the expression parser has successfully parsed a term.  It
is given the list of tokens that were found to make up the term, along with any nested
expressions that were found.  If the term form includes a tag, this is given to the method
also.

#### The `CreateUnaryOperation()` Method

This method is called whenever the expression parser has successfully parsed a unary
operation.  It is given the list of tokens that make up the operation and the one term
the operator is to act on.

#### The `CreateBinaryOperation()` Method

This method is called whenever the expression parser has successfully parsed a binary
operation.  It is given the list of tokens that make up the operation and the left and
right terms the operator is to act on.

#### The `CreateTrinaryOperation()` Method

This method is called whenever the expression parser has successfully parsed a trinary
operation.  It is given the list of tokens that make up the left and right operations (as
separate lists) and the left, middle and right terms the operator is to act on.
