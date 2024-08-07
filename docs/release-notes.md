## Release Notes

### 1.1.4

- Fixed two issues with switch clause tagging.
- Fixed an issue with parsing expression terms that contain nested expressions.  Yes, it
  includes a new test.
- Fixed an issue with parsing expression terms that contain nested expressions.  Yes, it
  includes a new test.
- Fixed an issue with parsing expressions through clauses.  A new test was added to make
  sure things work right.
- Fixed an issue with expression clause parsers not being properly created.
- Fixed an issue with using multiple types of string tokenizers in the parser DSL.
- Fixed an issue where a simple number was not allowed in a repeat clause.

### 1.1.3

- Found a better way to support self- or cross-referencing clauses that doesn't require
  forward declarations.

### 1.1.2.1

- Fixed an issue with self- or cross-referencing clause definitions to the DSL DSl.

### 1.1.2

- Added support for self- or cross-referencing clause definitions to the DSL DSl.

### 1.1.1

- Added support for Greek letters to be part of identifiers as they can be common in some
  DSLs.

- The `CanStart()` method on tokenizers is meant to be protected, not internal.  Without
  that, custom tokenizers outside the library cannot be written.

### 1.1.0.1

- Minor bug fix relating to wrapping an expression clause with a repeating clause.

### 1.1.0

- Added support for an expression clause, allowing expressions to be expected in the
  midst of other clauses.

- Addition of the [Use Cases](use-cases.md) page to the documentation.

### 1.0.0

The initial version of the library, at least from the point of view of C#; see
[History of the Library](history.md) if you're curious about more details.
