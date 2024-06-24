namespace Lex.Expressions;

/// <summary>
/// This represents a tagging interface for all types of terms in an expression.  This is
/// required because generics just won't cut it, given how expression parsers need to be
/// created from a DSL specification.
/// </summary>
public interface IExpressionTerm;
