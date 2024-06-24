using Lex.Parser;

namespace Lex.Tokenizers;

/// <summary>
/// This class provides the tokenizer for isolating double-quoted string literals.  Such
/// literals cannot span lines.
/// </summary>
public class DoubleQuotedStringTokenizer : StringTokenizer
{
    public DoubleQuotedStringTokenizer(LexicalParser parser) : base(parser, "\"") {}
}
