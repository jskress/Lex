## The Lexical Parser

This section describes how to create, configure and use a lexical parser.

### Parser Creation and Configuration

To support all the concepts described in [The Basics](the-basics.md),  Lex is necessarily very
configurable and extensible.  As you might expect, there are two ways to create and
configure a lexical parser for a parsing task.

The first way is to do so in code by creating instances of appropriate parsers and tokenizers,
with various properties configured to make tokenizing work the way you need.

The second way, and you knew this was coming, is to declare how a parser should work in the
Lex parser configuration DSL and use the parser factory to parse that specification and
produce the properly configured Lex parser for your task.

When creating a parser, it's important to remember that it is a disposable object, so you
will want to declare and use it in a `using` clause or declaration.  That said, it is
possible to reuse a parser to parse multiple sources.  A process that uses a cache of
parsers to do recurring parsing tasks might be an example.  To support this, you would not
involve a `using` clause or declaration.  However, you **will* need to properly dispose of
the parser once it is ready to go out of scope (i.e, not be returned to the cache, in our
example).  To free up resources after a parsing task but without disposing of the object,
you will want to call the `Close()` method.

#### Parser Creation and Configuration By Hand

Creating an instance of the Lex parser is simple instantiation.

```csharp
using LexicalParser parser = new LexicalParser();
```

Then, to configure it, create the tokenizers you require, giving each a reference to the
parser.  A pretty common configuration might look like ths:

```csharp
_ = new CommentTokenizer(parser).AddStandardComments();
_ = new IdTokenizer(parser);
+ = new SingleQuotedStringTokenizer(parser)
    {
        RepresentsCharacter = false
    };
_ = new DoubleQuotedStringTokenizer(parser);
_ = new BounderTokenizer(parser);
_ = new OperatorTokenizer(parser);
_ = new WhitespaceTokenizer(parser);
```

Now you have a parser that's ready to parser some source.

Refer to [The Tokenizers](the-tokenizers.md) page for documentation about each of the
tokenizers provided by the Lex library.  Refer to the [Extending Lex](extending-lex.md)
page for documentation about writing your own tokenizers.

#### The Lexical Parser Factory

An alternate way to create and configure a parser is to use Lex's parser factory, giving
it the specification for how you would like the parser configured.  To use the factory to
create and configure the same parser as above would look like this:

```csharp
using LexicalParser parser = LexicalParserFactory.CreteFrom("""
    standard comments
    identifiers
    single quoted strings multiChar
    double quoted strings
    bounders
    predefined operators
    whitespace
    """);
```

Now you have a parser configured the same way as the one above that's ready to parse some
source.

Refer to [The Parser Factory DSL](parser-factory-dsl.md) page for documentation about the
DSL that the `CreateFrom()` method interprets.

### Using the Lexical Parser

Once you've constructed your Lex parser, you'll need to set the reader from which the parser
will read characters.  The Lex library provides a convenience extension method, `AsReader()`,
for `string`s that will produce a `StreamReader` with the string as its source that may then
be passed to the `SetSource()` method.

Once the source has been set, you'll be ready to start pulling tokens from the parser.
Because of the nature of DSL parsing, the parser does not present itself as enumerable.
Your top-level loop will generally look something like this:

```csharp
using LexicalParser parser = LexicalParserFactory.CreateFrom("...");

// This line is very important.
parser.SetSource(/* The source text your parser is to parse. */);

Token next = parser.GetNextToken();

while (next != null) // null indicates the end of the stream.
{
    // Process the token.
    
    next = parser.GetNextToken();
}
```

If you want to use clauses to parse at a somewhat higher level, see the
[Using Clauses](using-clauses.md) section below for how that will affect your top-level
loop.

#### `LexicalParser` Methods

##### `Token GetNextToken()`

Use the `GetNextToken()` method to parse the next token from the underlying stream of
characters.  If the character source has hit the end of its stream, `null` will be returned.

##### `Token GetRequiredToken(Func<string> msgSource = null)`

Use the `GetRequiredToken()` to parse the next token from the underlying stream of
characters, with the assumption that it is required.  If there are no more tokens, then
`msgSource` is invoked to produce the text of a `TokenException` which is then thrown.  If
`msgSource` is not provided or returns `null`, then a default message is used.

##### `Token PeekNextToken()`

Use the `PeekNextToken()` method to parse the next token from the underlying stream of
characters without actually consuming it from the stream of tokens.  If there are no more
tokens, `null` will be returned.

##### `bool IsAtEnd()`

Use the `IsAtEnd()` method to test whether more tokens exist to be parsed.  This has the
same effect as, `PeekNextToken() == null`.

##### `Token MatchToken(bool isRequired, Func<string> msgSource = null, params Token[] tokens)`

Use the `MatchToken()` method to get the next token and match it against one or more
expected tokens.  If no more tokens are available and `isRequired` is `true`, an exception
is thrown.  If the parsed token does not match one of the given ones, then `msgSource` is
invoked to produce the text of a `TokenException` which is then thrown.  If `msgSource` is
not provided or returns `null`, then a default message is used.

This example parses the next token and guarantees that it exists, is an operator token
and is either the `OperatorToken.Plus` or `OperatorToken.Minus` token.

```csharp
Token operator = parser.MatchToken(
    true, () => "Expecting + or - here.", OperatorToken.Plus, Operator.Minus);
```

##### `bool IsNext(params Token[] tokens)`

Use the `IsNext()` method to check whether the next token matches one or more expected
tokens.  The state of the token stream remains unchanged.

This example compares the next token to see if it is either the `OperatorToken.Plus` or
`OperatorToken.Minus` token.

```csharp
if (parser.IsNext(OperatorToken.Plus, Operator.Minus))
{
    // We have a sign!  Don't forget that the token has NOT been consumed.
}
```

##### `Token MatchTokenType(bool isRequired, Func<string> msgSource = null, params Type[] types)`

Use the `MatchTokenType()` method to get the next token and match it against one or more
expected types of tokens.  If no more tokens are available and `isRequired` is `true`, an
exception is thrown.  If the parsed token does not match one of the given types, then
`msgSource` is invoked to produce the text of a `TokenException` which is then thrown.  If
`msgSource` is not provided or returns `null`, then a default message is used.

This example parses the next token and guarantees that it exists and is an operator token.

```csharp
Token operator = parser.MatchTokenType(
    true, () => "Expecting + or - here.", typeof(OperatorToken));
```

##### `bool IsNextOfType(params Type[] types)`

Use the `IsNextOfType()` method to check whether the next token's type matches one or more
expected types.  The state of the token stream remains unchanged.

This example compares the next token to see if it is a bounder.

```csharp
if (parser.IsNextOfType(typeof(BounderToken)))
{
    // We have a bounder coming next!  Don't forget that the token has NOT been consumed.
}
```

##### `void ReturnToken(Token token)`

Use the `ReturnToken()` to "unread" a token or put a token back into the parser.  The
token provided to this method will become the next token to read from the parser.  If you
need to return multiple tokens, be sure to return them in the reverse order that they were
read to ensure the integrity of the token stream.

If `null` is passed as the token, then the method quietly does nothing.

##### `void ReturnTokens(IEnumerable<Token> tokens)`

Use the `ReturnTokens()` to "unread" a series of tokens or put a series of tokens back into
the parser.  The tokens provided to this method will become the next ones to read from the
parser.  Note that this method will reverse the order of the tokens before putting them
back in the parser.  As such, the enumeration should contain the tokens in the order they
were read from the parser.

If `null` is passed as the token enumeration, then the method quietly does nothing.

##### `void Close()`

Use the `Close()` to clean up at the end of a parsing task.  It is automatically called
by both `SetSource()` and `Dispose()`.  You should use it when you're done with a particular
parsing task but want to keep the parser around for another task.
