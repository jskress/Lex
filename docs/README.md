## Lex Documentation

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

The library sports two DSLs of its own.  One is used by Lex's parser factory to create and
configure a lexical parser.  The other is used to provide a specification of a DSL, which
mostly means defining higher-level clause and expression grammar and giving you the means
for parsing source written in that language.

This documentation tells you everything you need to know to use Lex effectively.

- [The Basics](the-basics.md)
  - [Semantic vs. Lexical Parsing](the-basics.md#semantic-vs-lexical-parsing)
  - [Types of Tokens](the-basics.md#types-of-tokens)
  - [Clauses](the-basics.md#clauses)
  - [Expressions](the-basics.md#expressions)
- [Use Cases](use-cases.md)
  - [Parsing JSON](use-cases.md#parsing-json)
  - [Data Transfer Tool](use-cases.md#data-transfer-tool)
  - [C# to TypeScript Transpiler](use-cases.md#c-to-typescript-transpiler)
  - [Ray Tracer](use-cases.md#ray-tracer)
- [The Lexical Parser](the-parser.md)
  - [Parser Creation and Configuration](the-parser.md#parser-creation-and-configuration)
  - [Using the Lexical Parser](the-parser.md#using-the-lexical-parser)
- [The Tokenizers](the-tokenizers.md)
  - [The Based Number Tokenizer](the-tokenizers.md#the-based-number-tokenizer)
  - [The Bounder Tokenizer](the-tokenizers.md#the-bounder-tokenizer)
  - [The Comment Tokenizer](the-tokenizers.md#the-comment-tokenizer)
  - [The Double Quoted Sting Tokenizer](the-tokenizers.md#the-double-quoted-string-tokenizer)
  - [The Identifier Tokenizer](the-tokenizers.md#the-identifier-tokenizer)
  - [The Keyword Tokenizer](the-tokenizers.md#the-keyword-tokenizer)
  - [The Number Tokenizer](the-tokenizers.md#the-number-tokenizer)
  - [The Operator Tokenizer](the-tokenizers.md#the-operator-tokenizer)
  - [The Single Quoted String Tokenizer](the-tokenizers.md#the-single-quoted-string-tokenizer)
  - [The String Tokenizer](the-tokenizers.md#the-string-tokenizer)
  - [The Tripe Quoted String Tokenizer](the-tokenizers.md#the-triple-quoted-string-tokenizer)
  - [The Whitespace Tokenizer](the-tokenizers.md#the-whitespace-tokenizer)
- [The Parser Factory DSL](parser-factory-dsl.md)
  - [The Top Level](parser-factory-dsl.md#the-top-level)
  - [The Based Number Tokenizer Clause](parser-factory-dsl.md#the-based-number-tokenizer-clause)
  - [The Bounder Tokenizer Clause](parser-factory-dsl.md#the-bounder-tokenizer-clause)
  - [The Standard Comments Tokenizer Clause](parser-factory-dsl.md#the-standard-comments-tokenizer-clause)
  - [The Comments Tokenizer Clause](parser-factory-dsl.md#the-comments-tokenizer-clause)
  - [The Identifiers Tokenizer Clause](parser-factory-dsl.md#the-identifier-tokenizer-clause)
  - [The Sourced Keywords Tokenizer Clause](parser-factory-dsl.md#the-sourced-keywords-tokenizer-clause)
  - [The Keywords Tokenizer Clause](parser-factory-dsl.md#the-keywords-tokenizer-clause)
  - [The Numbers Tokenizer Clause](parser-factory-dsl.md#the-numbers-tokenizer-clause)
  - [The Sourced Operators Tokenizer Clause](parser-factory-dsl.md#the-sourced-operators-tokenizer-clause)
  - [The Operators Tokenizer Clause](parser-factory-dsl.md#the-operators-tokenizer-clause)
  - [The Strings Tokenizer Clause](parser-factory-dsl.md#the-strings-tokenizer-clause)
  - [The Whitespace Tokenizer Clause](parser-factory-dsl.md#the-whitespace-tokenizer-clause)
- [Using Clauses](using-clauses.md)
  - [Overview](using-clauses.md#overview)
  - [A Clause of One Token](using-clauses.md#a-clause-of-one-token)
  - [A Sequence of Clauses](using-clauses.md#a-sequence-of-clauses)
  - [A Choice of Clauses](using-clauses.md#a-choice-of-clauses)
  - [Optional and Repeated Clauses](using-clauses.md#optional-and-repeated-clauses)
  - [Capturing Tokens](using-clauses.md#capturing-tokens)
  - [Debugging](using-clauses.md#debugging)
- [Using Expressions](using-expressions.md)
  - [Overview](using-expressions.md#overview)
  - [Expression Parser Classes](using-expressions.md#expression-parser-classes)
- [Defining a Language](language-specification.md)
  - [Overview](language-specification.md#overview)
  - [The `Dsl` Class](language-specification.md#the-dsl-class)
- [DSL Specification DSL](dsl-specification-dsl.md)
  - [The Top Level](dsl-specification-dsl.md#the-top-level)
  - [Lexical Parser Specification](dsl-specification-dsl.md#lexical-parser-specification)
  - [Defining Keywords](dsl-specification-dsl.md#defining-keywords)
  - [Defining Operators](dsl-specification-dsl.md#defining-operators)
  - [Defining Arbitrary Tokens](dsl-specification-dsl.md#defining-arbitrary-tokens)
  - [Defining the Form of Expressions](dsl-specification-dsl.md#defining-the-form-of-expressions)
  - [Defining Expression Term Forms](dsl-specification-dsl.md#defining-expression-term-forms)
  - [Defining Unary Operators](dsl-specification-dsl.md#defining-unary-operators)
  - [Defining Binary Operators](dsl-specification-dsl.md#defining-binary-operators)
  - [Defining Trinary Operators](dsl-specification-dsl.md#defining-trinary-operators)
  - [Customizing Expression Parsing Exception Text](dsl-specification-dsl.md#customizing-expression-parsing-exception-text)
  - [Defining a Sequential Clause](dsl-specification-dsl.md#defining-a-sequential-clause)
  - [The Sequential Clause](dsl-specification-dsl.md#the-sequential-clause)
  - [Defining a Switch Clause](dsl-specification-dsl.md#defining-a-switch-clause)
  - [The Switch Clause](dsl-specification-dsl.md#the-switch-clause)
  - [The Term Clause](dsl-specification-dsl.md#the-term-clause)
  - [The Repetition Clause](dsl-specification-dsl.md#the-repetition-clause)
- [Extending Lex](extending-lex.md)
  - [Tokenizers and Tokens](extending-lex.md#tokenizers-and-tokens)
  - [Clauses](extending-lex.md#clauses)
  - [Expression Tree Builders](extending-lex.md#expression-tree-builders)
- [History of the Library](history.md)
- [Release Notes](release-notes.md)
