## Use Cases

This section covers some real-world examples of how the Lex library has been used.

### Parsing JSON

This is more of an academic exercise since most runtimes have a highly efficient JSON
parser available already, but it does serve as an example of some of what Lex can do.

Because it is so simple, you'd want to parse JSON at a token level only; using clauses
would needlessly complicate things.

Here's how you could declare JSON as a DSL specification:

```csharp
Dsl dsl = LexicalDslFactory.CreateFrom(""""
    _keywords: 'true', 'false', 'null'
    _operators: comma
    _parserSpec: """
        dsl keywords
        double quoted strings
        numbers with signs
        dsl operators
        bounders
        whitespace
        """
    """");
LexicalParser parser = dsl.CreateLexicalParser();
```

From there, you can use the parser to pull apart any JSON document.  Note that, at this
level, you'd have to manage object/array nesting yourself.

### Data Transfer Tool

There once was a need for a single tool that could read from a source, map and potentially
modify the data and then, write to a target.  This was back when it was still fairly
expensive to use intermediate formats such as XML and translation tools based, say, on XSD.

The tool needed to be able to infer data structures from a variety of sources, be they a
C header file, an SQL `CREATE TABLE` or `CREATE VIEW` DDL, Java classes, etc.

Lex allowed the tool to read data structure definitions from any of these source languagses
while also providing a DSL to declare the source-to-target mapping logic.

### C# to TypeScript Transpiler

Lex made it easy for this tool to "understand" the subset of the C# class declaration
syntax, including attribute interpretation, so that a TypeScript equivalent of the class
could be generated.  This allowed the development project to single-source its DTOs.

### Ray Tracer

A ray tracer has an obvious need for a DSL for specifying the structure of a scene to
render.  This includes declaring higher-level data constructs, such as vector and matrix
notation, than general languages support.  It also took advantage of being able to declare
that the superscript 2, (`²`), could be recognized as a postfix unary square operator.
Other Unicode math symbols were used as expression operators in a similar way.
