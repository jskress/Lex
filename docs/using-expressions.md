## Using Expressions

This section describes how to construct and use an expression parser.  Here, we build the
parser by hand but, of course, by describing your DSL's notion of what an expression is
in your DSL's specification, you can have the DSL factory create and configure the expression
parser for you which is made available to you via the `ExpressionParser` property of the
[`Dsl`](../Dsl/Dsl.cs) class.

See the
[Defining the Form of Expressions](dsl-specification-dsl.md#defining-the-form-of-expressions)
section of the DSL Specification page for details on defining what an expression looks like
using Lex's DSL DSL.

### Overview

Understanding an expression takes more than just isolating the tokens that make it up.
Ultimately, the only thing to understand about an expression is the terms in the expression
and any operators that act on them.  When an expression involves operators, you must also
understand the order in which they should be evaluated.

#### The Expression Tree

So, the goal of the expression parser that Lex provides is to take on the work of taking
the tokens discovered to be part of an expression and convert them into an expression tree.
A tree form is used because it can represent the order of operator evaluation correctly.

Given an expression involving different operator precedences like, `1 + 2 * 3`, the root
term of the tree will represent the addition binary operation with a left term of the number
1 and a right term that is the result of a multiplication binary operation of the 2 and 3.

The expression parser uses an implementation of the `IExpressionTreeBuilder` interface to
actually build the tree.  Whether you actually store the terms in a tree when providing
your own implementation of this interface is completely up to you.  It is important,
however, to understand that this is the assumption the parser makes and the pattern of calls
the parser makes to the tree builder will reflect this.

It is also assumed that the result of an operation will itself be a term.  This properly
allows for nested things such as a function call (a term) that carries a list of terms which
are the arguments to the function.  As such, you need to tag all you terms and operations
with the `ITerm` interface.

You can just as easily use the default tree builder provided by Lex.

#### Order of Operations

There are several nuances regarding the order of operations in an expression.  The parser
Lex follows these rules:

- Unary operators take precedence over binary operators which themselves take precedence
  over trinary operators.  All known computing languages behave this way.

- Within unary operators, operators are applied nearest to the term outward.  For prefix
  operators, this means right-to-left and for postfix, left-to-right.  The parser applies
  prefix operators first and then postfix, but you can change this.

- Within binary operators, all operators are registered with the parser with a precedence.
  The parser will guarantee that operators of higher precedence are evaluated first.  When
  precedences are the same, evaluation is left to right.

- Trinary operators are a special beast and do not support the notion of a priority.  This
  is a side effect of how they need to be parsed.  The first term is parsed as a potential
  binary term.  The middle and right terms are parsed as potential nested trinary terms.

### Expression Parser Classes

Here, we'll cover the various classes and interfaces that provide the mechanics of parsing
an expression.

#### The `ExpressionParser` Class

The expression parser is provided by the [`ExpressionParser`](../Expressions/ExpressionParser.cs)
class (there's a shock!).  It carries a reference to the tree builder it will use when
parsing source.  The tree builder is initially set to an instance of the default tree
builder provided by Lex.  You can change this to anything that implements the
[`IExpressionTreeBuilder`](../Expressions/IExpressionTreeBuilder.cs) interface, but you
cannot, for obvious reasons, set it to `null`.  If you want to be able to just "parse
through" an expression (i.e., consume the tokens from the source properly but never bother
with the parsed expression itself), use an instance of the
[`NoOpExpressionTreeBuilder`](../Expressions/NoOpExpressionTreeBuilder.cs) class that Lex
also provides.

Once you've created and configured, you'll use the `ParseExpression()` method to, well,
parse an expression into a root term.

For the parser to be able to anything, it must have at least one option for what a term
can look like.  This is represented by the [`TermChoiceParser`](../Expressions/TermChoiceParser.cs)
class ([see below for details on this](#the-termchoiceparser-class)).  You add instances
of these to the expression parser using the `AddTermChoiceParser()` method.  You can add
as many term options as makes sense for your language.

If you do nothing else, the expression parser will be able to parse whatever sorts of
terms you've configured, but that's all.  You will (likely) also want to add unary, binary
and/or trinary operators to the parser.

##### Adding Unary Operators

If you want to support any unary operators, use the `AddUnaryOperatorParser()` method to
add their specifications to the expression parser.  A unary operator is made up of one or
more sequential tokens and flags that indicate whether the operator may appear before or
after the term it affects, or both.  Usually, the token set will have only one token in
it, like `++`.  But multiple tokens are supported; think something like `inverse` then
`transform`.

You may use the `PrefixFirst` property to control whether prefix operators are applied
before postfix operators (the default) or postfix, then prefix.

##### Adding Binary Operators

If you want to support any binary operators, use one of the `AddBinaryOperatorParser()`
methods to add their specifications to the expression parser.  A binary operator is made up
of one or more sequential tokens and a precedence.  Ultimately, the precedence of a binary
operator is an integer which allows the parser to work out the order of applying binary
operators.

A default set of precedence definitions is provided in the
[`OperatorPrecedence`](../Expressions/OperatorPrecedence.cs) enum.  The entries in the enum
are valued in such a way as to follow standard mathematical precedence rules.  There are
two overloads for adding a binary operator, one that accepts an entry from the
[`OperatorPrecedence`](../Expressions/OperatorPrecedence.cs) enum and one that accepts a
raw integer.

Usually, the token set will have only one token in it, like `+`.  But multiple tokens are
supported; think something like `is` then `not`.

##### Adding Trinary Operators

If you want to support any trinary operators, use the `AddTrinaryOperatorParser()` method
to add their specifications to the expression parser.  A trinary operator is made up of
two sets of one or more sequential tokens.  Usually, the token sets will each have only one
token in it, like `?` and `:`.

##### Error Messages

There are a few places where the expression parser will throw parsing exceptions.  These
situations are represented by the entries in the
[`ExpressionParseMessageTypes`](../Expressions/ExpressionParseMessageTypes.cs) enum.  There
are default values for the text of each of the possible errors, but you may use the
`SetMessageText()` method to set the text to whatever makes more sense to you.  Note that
you cannot set the text for the `None` entry as that is not a message type that's ever used
in throwing an exception.

#### The `TermChoiceParser` Class

This class represents one form a term may take in an expression.  It must be configured to
match a series of one or more tokens or types.  You may also configure where in the token
stream to expect child expressions, such as entries in an array or arguments to a function
call.

When you add an explicit token to match (as opposed to a type), you can tell the choice
parser whether the token should be suppressed.  For example, when isolating a series of
tokens as a function call, you don't really need the parentheses.  So, by noting that
they must be present but not needed afterward, the parser will correctly require them but
will not pass them along to the `CreateTerm()` method of the current tree builder.  This
is called _suppressing_ a token.

Once you've configured a term choice parser, just add it to your expression parser and the
expression parser will handle everything from there.  Consider this example:

```csharp
ExpressionPossibilitySet commas = new ExpressionPossibilitySet()
    .AddChoice(OperatorToken.Comma);
ExpressionTermChoiceItem expressionsTerm = new ExpressionTermChoiceItem(0, int.MaxValue, commas)
TermChoiceParser trueTerm = new TermChoiceParser().Matching(new Keyword("true"));
TermChoiceParser falseTerm = new TermChoiceParser().Matching(new Keyword("false"));
TermChoiceParser nullTerm = new TermChoiceParser().Matching(new Keyword("null"));
TermChoiceParser stringTerm = new TermChoiceParser().Matching(typeOf(StringToken));
TermChoiceParser numberTerm = new TermChoiceParser().Matching(typeOf(NumberToken));
TermChoiceParser functionTerm = new TermChoiceParser()
    .Matching(typeOf(IdToken))
    .Then(BounderToken.LeftParen, true)
    .ExpectingExpression(expressionsTerm)
    .Then(BounderToken.RightParen, true)
    .WithTag("function");
TermChoiceParser idTerm = new TermChoiceParser().Matching(typeOf(IdToken));
ExpressionParser expressionParser = new ExpressionParsesr()
    .AddTermChoiceParser(trueTerm)
    .AddTermChoiceParser(falseTerm)
    .AddTermChoiceParser(nullTerm)
    .AddTermChoiceParser(stringTerm)
    .AddTermChoiceParser(numberTerm)
    .AddTermChoiceParser(functionTerm)
    .AddTermChoiceParser(idTerm);
```
(The [`ExpressionTermChoiceItem`](../Expressions/TermChoiceItem.cs) and
[`ExpressionPossibilitySet`](../Expressions/ExpressionPossibilitySet.cs) classes are
described below.)  Yes, configuring an expression parser can get involved; it's easier to
do this by using the DSL factory.  The equivalent expression DSL looks like this:

```
_keywords: 'true', 'false', 'null'
_operators: predefined
_expressions:
{
  term: [
      true, false, null, _string, _number,
      _identifier /leftParen _expression(*, comma) /rightParen => 'function',
      _identifier
  ]
}
```

Order is important here.  Notice that the more specific term form for a function is added
before the general identifier term.  If you added the identifier term first, the parser
would never report the function form.

#### The `ExpressionTermChoiceItem` Class

This class represents an item within a term choice parser that notes where some number of
child expressions should be expected.  The item must be configured with the minimum number
of required expressions, which may be `0`, the maximum number of expressions to allow and
optional set of items that may separate the expressions (like commas in an argument list).

If the maximum number of expressions to expect is 1, then you cannot specify any separators.
The set of separators is represented by an instance of the `ExpressionPossibilitySet`,
described next.

#### The `ExpressionPossibilitySet` Class

This class behaves the same way as the [`SwitchClauseParser`](../Clauses/SwitchClauseParser.cs)
that is restricted to matching against one or more tokens or token types.  It must have at least one choice added to it.
