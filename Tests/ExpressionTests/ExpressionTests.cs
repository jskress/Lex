using Lex;
using Lex.Clauses;
using Lex.Dsl;
using Lex.Expressions;
using Lex.Parser;
using Lex.Tokens;

namespace Tests.ExpressionTests;

[TestClass]
public class ExpressionTests
{
    private const string DslSpec = """"
        _parserSpec: """
            dsl keywords
            identifiers
            integral numbers
            single quoted strings multiChar
            predefined operators
            bounders
            whitespace
            """
        _keywords: 'true', 'false', 'null'
        _operators: predefined
        _expressions:
        {
            term: [
                true, false, null, _string, _number,
                _identifier /leftParen _expression(*, comma) /rightParen => 'function',
                _identifier /openBracket _expression(1..2, comma) /closeBracket => 'indexed',
                _identifier /lessThan _expression(1..2, comma) _identifier => 'bogus',
                _identifier => 'id',
                /openBracket _expression(*, comma) /closeBracket => 'array'
            ]
            unary: [ plus*, minus*, not*, *doubleplus*, *doubleminus* ]
            binary: [ plus, minus, multiply, divide ]
            trinary: [ (if, colon) ]
        }
        """";

    private record GoodTestCase(string Scenario, string Source, string Expected)
    {
        internal void Run()
        {
            DefaultExpressionTerm term = ParseToTerm(Source);

            Assert.AreEqual(Expected, term.Text, Scenario);
        }
    }

    private static readonly GoodTestCase[] GoodTestCases =
    [
        new GoodTestCase("True keyword", "true", "(true)"),
        new GoodTestCase("False keyword", "false", "(false)"),
        new GoodTestCase("Null keyword", "null", "(null)"),
        new GoodTestCase("String", "'text'", "('text')"),
        new GoodTestCase("Number", "7", "(7)"),
        new GoodTestCase("Identifier", "bob", "(bob => id)"),
        new GoodTestCase("Function", "call(1, 2, 3)", "(call(1), (2), (3) => function)"),
        new GoodTestCase("Indexed", "array[12]", "(array(12) => indexed)"),
        new GoodTestCase("Array", "[7, 12]", "((7), (12) => array)"),
        new GoodTestCase("Prefixed unary 1", "!true", "(!(true))"),
        new GoodTestCase("Prefixed unary 2", "++2", "(++(2))"),
        new GoodTestCase("Post-fixed unary", "2--", "((2)--)"),
        new GoodTestCase("Pre and post-fixed unary", "++2--", "((++(2))--)"),
        new GoodTestCase("Default precedence", "1 + 2 * 3", "((1) + ((2) * (3)))"),
        new GoodTestCase("Parenthetical override", "(1 + 2) * 3", "(((1) + (2)) * (3))"),
        new GoodTestCase("Conditional", "1 ? 2 : 3", "((1) ? (2) : (3))"),
        new GoodTestCase("Combo", "1 + 2 ? 2 - 3 : 3 / 4", "(((1) + (2)) ? ((2) - (3)) : ((3) / (4)))"),
    ];

    private record DslErrorTestCase(string Scenario, string Source, string Expected)
    {
        internal void Run()
        {
            TestsBase.AssertTokenException(
                () => _ = LexicalDslFactory.CreateFrom(Source), Expected, Scenario);
        }
    }

    private static readonly DslErrorTestCase[] DslErrorTestCases =
    [
        new DslErrorTestCase(
            "Dupe expression error",
            """
                _expressions: { term: [ _identifier ] } 
                _expressions: { } 
                """,
            "An expression specification has already been set."),
        new DslErrorTestCase(
            "Missing term error",
            "_expressions: { } ",
            "No term clause specified for expressions."),
        new DslErrorTestCase(
            "Incomplete expression error",
            "_expressions: { term: [",
            "Expecting a term spec or close bracket here."),
        new DslErrorTestCase(
            "Empty clause error",
            "_expressions: { term: [ , ] }",
            "Expecting a term spec here."),
        new DslErrorTestCase(
            "Empty clause error",
            "_expressions: { term: [ / ] }",
            "Expecting a term spec here."),
        new DslErrorTestCase(
            "Extra token error",
            "_expressions: { term: [ _identifier => 'tag' _number ] }",
            "Unexpected token found here."),
        new DslErrorTestCase(
            "Cannot suppress expression error",
            "_expressions: { term: [ /_expression ] }",
            "Expressions cannot be suppressed."),
        new DslErrorTestCase(
            "Cannot suppress expression error",
            "_expressions: { term: [ /_identifier ] }",
            "Term type reference cannot be suppressed."),
        new DslErrorTestCase(
            "Cannot suppress expression error",
            "_expressions: { term: [ _expression(1, comma) ] }",
            "Expecting a right parenthesis here."),
        new DslErrorTestCase(
            "Expression count < 1 error",
            "_expressions: { term: [ _expression(0) ] }",
            "Expecting a '*', '+', '?' or a number greater than 1 or range here."),
        new DslErrorTestCase(
            "Expression count marker error",
            "_expressions: { term: [ _expression(??) ] }",
            "Expecting a '*', '+', '?' or a number greater than 1 or range here."),
        new DslErrorTestCase(
            "Expression count type error",
            "_expressions: { term: [ _expression('count') ] }",
            "Expecting the range operator, \"..\", here."),
        new DslErrorTestCase(
            "Term tag error 1",
            "_expressions: { term: [ _identifier => ] }",
            "Expecting a string to follow '=>' here."),
        new DslErrorTestCase(
            "Term tag error 2",
            "_expressions: { term: [ _identifier => 3 ] }",
            "Expecting a string to follow '=>' here."),
        new DslErrorTestCase(
            "Trinary error",
            "_expressions: { trinary: [ _identifier ] }",
            "Expecting a left parenthesis here."),
        new DslErrorTestCase(
            "Trinary form error 1",
            """
                _operators: predefined
                _expressions: { trinary: [ (,) ] }
                """,
            "Expecting a variable reference here."),
        new DslErrorTestCase(
            "Trinary form error 2",
            """
                _operators: predefined
                _expressions: { trinary: [ (if ] }
                """,
            "Expecting an identifier here."),
        new DslErrorTestCase(
            "Trinary form error 3",
            """
                _operators: predefined
                _expressions: { trinary: [ (if, ] }
                """,
            "Expecting an identifier here."),
        new DslErrorTestCase(
            "Trinary form error 4",
            """
                _operators: predefined
                _expressions: { trinary: [ (if,) ] }
                """,
            "Expecting a variable reference here."),
        new DslErrorTestCase(
            "Trinary form error 5",
            """
                _operators: predefined
                _expressions: { trinary: [ (if, colon ] }
                """,
            "Expecting an identifier here."),
        new DslErrorTestCase(
            "Precedence spec error 1",
            """
                _operators: predefined
                _expressions: { binary: [ plus : ] }
                """,
            "A precedence indicator is required after the colon."),
        new DslErrorTestCase(
            "Precedence spec error 2",
            """
                _operators: predefined
                _expressions: { binary: [ plus :bob ] }
                """,
            "Expecting a precedence value or number here."),
        new DslErrorTestCase(
            "Precedence inference error 1",
            """
                square: _operator("\u00B2")
                _expressions: { binary: [ square ] }
                """,
            "Cannot determine the precedence of the operator."),
        new DslErrorTestCase(
            "Precedence inference error 2",
            """
                _keywords: 'is', 'not'
                _expressions: { binary: [ is not ] }
                """,
            "Cannot determine the precedence of the operator."),
        new DslErrorTestCase(
            "Not a token error",
            "_expressions: { binary: [ _identifier ] }",
            "The identifier, '_identifier', does not refer to a token."),
        new DslErrorTestCase(
            "Not a token error",
            "_expressions: { binary: [ 3 ] }",
            "Expecting an identifier here."),
        new DslErrorTestCase(
            "Message type error 1",
            """
                _expressions: { messages: [ 2 ] }
                """,
            "\"2\" is an unsupported message type here."),
        new DslErrorTestCase(
            "Message type error 2",
            """
                _expressions: { messages: [ bob ] }
                """,
            "\"bob\" is an unsupported message type here."),
        new DslErrorTestCase(
            "Message type error 2",
            """
                _expressions: { messages: [ None ] }
                """,
            "\"None\" is an unsupported message type here."),
        new DslErrorTestCase(
            "Message colon error 1",
            """
                _expressions: { messages: [ MissingRequiredTerm ] }
                """,
            "Expecting a colon after the message type here."),
        new DslErrorTestCase(
            "Message colon error 2",
            """
                _expressions: { messages: [ MissingRequiredTerm id ] }
                """,
            "Expecting a colon after the message type here."),
        new DslErrorTestCase(
            "Message text error 1",
            """
                _expressions: { messages: [ MissingRequiredTerm: 2 ] }
                """,
            "Expecting a non-empty string here."),
        new DslErrorTestCase(
            "Message text error 2",
            """
                _expressions: { messages: [ MissingRequiredTerm: '' ] }
                """,
            "Expecting a non-empty string here."),
    ];
    
    private enum ExceptionType
    {
        Token,
        Argument,
        ArgumentNull,
        Exception
    }

    private record ExpressionParserErrorTestCase(
        string Scenario, ExceptionType Type, Action TheAction, string Expected)
    {
        internal void Run()
        {
            switch (Type)
            {
                case ExceptionType.Token:
                    TestsBase.AssertTokenException(TheAction, Expected, Scenario);
                    break;
                case ExceptionType.Argument:
                    TestsBase.AssertArgumentException(TheAction, Expected, Scenario);
                    break;
                case ExceptionType.ArgumentNull:
                    TestsBase.AssertArgumentNullException(TheAction, Expected, Scenario);
                    break;
                case ExceptionType.Exception:
                    TestsBase.AssertException(TheAction, Expected, Scenario);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static readonly ExpressionParserErrorTestCase[] ExpressionParserErrorTestCases =
    [
        new ExpressionParserErrorTestCase(
            "Null tree builder error", ExceptionType.ArgumentNull,
            () => new ExpressionParser().TreeBuilder = null,
            "The expression tree builder cannot be set to null. (Parameter 'TreeBuilder')"),
        new ExpressionParserErrorTestCase(
            "Null tree choice error", ExceptionType.ArgumentNull,
            () => new ExpressionParser().AddTermChoiceParser(null),
            "Value cannot be null. (Parameter 'termChoiceParser')"),
        new ExpressionParserErrorTestCase(
            "False unary flags error", ExceptionType.Argument,
            () => new ExpressionParser().AddUnaryOperatorParser(null, false, false),
            "At least one of canBePrefix or canBePostfix must be true."),
        new ExpressionParserErrorTestCase(
            "Null tokens (unary) error", ExceptionType.ArgumentNull,
            () => new ExpressionParser().AddUnaryOperatorParser(null, true, false),
            "Value cannot be null. (Parameter 'tokens')"),
        new ExpressionParserErrorTestCase(
            "Empty tokens (unary) error", ExceptionType.Argument,
            () => new ExpressionParser().AddUnaryOperatorParser([], true, false),
            "At least one token must be provided. (Parameter 'tokens')"),
        new ExpressionParserErrorTestCase(
            "Null tokens (binary) error", ExceptionType.ArgumentNull,
            () => new ExpressionParser().AddBinaryOperatorParser(null, 0),
            "Value cannot be null. (Parameter 'operatorTokens')"),
        new ExpressionParserErrorTestCase(
            "Empty tokens (binary) error", ExceptionType.Argument,
            () => new ExpressionParser().AddBinaryOperatorParser([], 0),
            "At least one token must be provided. (Parameter 'operatorTokens')"),
        new ExpressionParserErrorTestCase(
            "Null tokens (left trinary) error", ExceptionType.ArgumentNull,
            () => new ExpressionParser().AddTrinaryOperatorParser(null, null),
            "Value cannot be null. (Parameter 'leftOperatorTokens')"),
        new ExpressionParserErrorTestCase(
            "Empty tokens (left trinary) error", ExceptionType.Argument,
            () => new ExpressionParser().AddTrinaryOperatorParser([], null),
            "At least one token must be provided. (Parameter 'leftOperatorTokens')"),
        new ExpressionParserErrorTestCase(
            "Null tokens (right trinary) error", ExceptionType.ArgumentNull,
            () => new ExpressionParser().AddTrinaryOperatorParser([OperatorToken.Plus], null),
            "Value cannot be null. (Parameter 'rightOperatorTokens')"),
        new ExpressionParserErrorTestCase(
            "Empty tokens (right trinary) error", ExceptionType.Argument,
            () => new ExpressionParser().AddTrinaryOperatorParser([OperatorToken.Plus], []),
            "At least one token must be provided. (Parameter 'rightOperatorTokens')"),
        new ExpressionParserErrorTestCase(
            "Error text for None error", ExceptionType.Argument,
            () => new ExpressionParser().SetMessageText(ExpressionParseMessageTypes.None, ""),
            "Cannot set the text for the \"None\" message type. (Parameter 'messageType')"),
        new ExpressionParserErrorTestCase(
            "Null error text error", ExceptionType.Argument,
            () => new ExpressionParser().SetMessageText(ExpressionParseMessageTypes.MissingRequiredToken, null),
            "You must set the text for a message type to a non-empty string. (Parameter 'text')"),
        new ExpressionParserErrorTestCase(
            "Empty error text error 1", ExceptionType.Argument,
            () => new ExpressionParser().SetMessageText(ExpressionParseMessageTypes.MissingRequiredToken, ""),
            "You must set the text for a message type to a non-empty string. (Parameter 'text')"),
        new ExpressionParserErrorTestCase(
            "Empty error text error 2", ExceptionType.Argument,
            () => new ExpressionParser().SetMessageText(ExpressionParseMessageTypes.MissingRequiredToken, "  "),
            "You must set the text for a message type to a non-empty string. (Parameter 'text')"),
        new ExpressionParserErrorTestCase(
            "Min too low error", ExceptionType.Argument,
            () => _ = new ExpressionTermChoiceItem(-1, 1, null),
            "Minimum must be zero or greater. (Parameter 'minimum')"),
        new ExpressionParserErrorTestCase(
            "Max too low error 1", ExceptionType.Argument,
            () => _ = new ExpressionTermChoiceItem(0, 0, null),
            "Maximum must be 1 or greater. (Parameter 'maximum')"),
        new ExpressionParserErrorTestCase(
            "Max too low error 2", ExceptionType.Argument,
            () => _ = new ExpressionTermChoiceItem(4, 1, null),
            "Maximum cannot be smaller than minimum. (Parameter 'maximum')"),
        new ExpressionParserErrorTestCase(
            "Disallowed separators error", ExceptionType.Argument,
            () => _ = new ExpressionTermChoiceItem(0, 1, new ExpressionPossibilitySet()),
            "Cannot specify separators when maximum is 1. (Parameter 'separators')"),
        new ExpressionParserErrorTestCase(
            "Term choice started error 1", ExceptionType.Exception,
            () => new TermChoiceParser().Matching(OperatorToken.And).Matching(OperatorToken.And),
            "The term choice has already been started."),
        new ExpressionParserErrorTestCase(
            "Term choice started error 2", ExceptionType.Exception,
            () => new TermChoiceParser().Matching(OperatorToken.And).Matching(typeof(IdToken)),
            "The term choice has already been started."),
        new ExpressionParserErrorTestCase(
            "Term choice not started error 1", ExceptionType.Exception,
            () => new TermChoiceParser().Then(OperatorToken.And),
            "The term choice has not been started."),
        new ExpressionParserErrorTestCase(
            "Term choice not started error 2", ExceptionType.Exception,
            () => new TermChoiceParser().Then(typeof(IdToken)),
            "The term choice has not been started."),
        new ExpressionParserErrorTestCase(
            "Null expression item error", ExceptionType.ArgumentNull,
            () => new TermChoiceParser().ExpectingExpression(null),
            "Value cannot be null. (Parameter 'item')"),
        new ExpressionParserErrorTestCase(
            "No terms error", ExceptionType.Exception,
            () => new ExpressionParser().ParseExpression(null),
            "No term formats have been included in the expression parser."),
        new ExpressionParserErrorTestCase(
            "Missing trinary right error", ExceptionType.Token,
            () => ParseToTerm("1 ? 2 x"),
            "Expecting the right trinary operator that goes with \"?\" here."),
        new ExpressionParserErrorTestCase(
            "Missing binary right error", ExceptionType.Token,
            () => ParseToTerm("1 + ,"),
            "Expecting a term here."),
        new ExpressionParserErrorTestCase(
            "Missing unary term error", ExceptionType.Token,
            () => ParseToTerm("++ ,"),
            "Expecting a term here."),
        new ExpressionParserErrorTestCase(
            "Unbalanced paren error", ExceptionType.Token,
            () => ParseToTerm("(1 + 2"),
            "Expecting a right parenthesis here."),
        new ExpressionParserErrorTestCase(
            "Post expression token error", ExceptionType.Token,
            () => ParseToTerm("id[1, 2 3"),
            "Expecting a bounder of \"]\" here."),
        new ExpressionParserErrorTestCase(
            "Post expression type error", ExceptionType.Token,
            () => ParseToTerm("id < 1, 2 3"),
            "Expecting an identifier here."),
        new ExpressionParserErrorTestCase(
            "Not enough expressions error", ExceptionType.Token,
            () => ParseToTerm("id[]"),
            "Expecting a term here."),
    ];

    [TestMethod]
    public void TestGoodScenarios()
    {
        foreach (GoodTestCase testCase in GoodTestCases)
            testCase.Run();
    }

    [TestMethod]
    public void TestDslErrors()
    {
        foreach (DslErrorTestCase testCase in DslErrorTestCases)
            testCase.Run();
    }

    [TestMethod]
    public void TestExpressionParserErrors()
    {
        foreach (ExpressionParserErrorTestCase testCase in ExpressionParserErrorTestCases)
            testCase.Run();
    }

    private static DefaultExpressionTerm ParseToTerm(string source)
    {
        Dsl dsl = LexicalDslFactory.CreateFrom(DslSpec);
        using LexicalParser parser = dsl.CreateLexicalParser();

        parser.SetSource(source.AsReader());

        return (DefaultExpressionTerm) dsl.ParseExpression(parser);
    }

    [TestMethod]
    public void TestDslUsage()
    {
        Dsl dsl = LexicalDslFactory.CreateFrom(""""
            _parserSpec: """
                dsl keywords
                single quoted strings multiChar
                whitespace
                """
            _keywords: 'this'
            _expressions:{
                term: [ _string ]
            }
            clause: { this > _expression } 
            """");
        using LexicalParser parser = dsl.CreateLexicalParser();

        parser.SetSource("this 'is a test'".AsReader());

        Clause clause = dsl.ParseClause(parser, "clause");
        
        Assert.AreEqual(1, clause.Tokens.Count);
        Assert.IsTrue(new KeywordToken("this").Matches(clause.Tokens[0]));
        Assert.AreEqual(1, clause.Expressions.Count);

        DefaultExpressionTerm term = clause.Expressions[0] as DefaultExpressionTerm;
        
        Assert.IsNotNull(term);
        Assert.AreEqual(1, term.Tokens.Count);
        Assert.AreEqual("('is a test')", term.Text);
    }
}
