namespace Lex.Dsl;

/// <summary>
/// This class implements a DSL (go figure) for defining and configuring a lexical parser.
/// </summary>
public static partial class LexicalParserFactory
{
    private const string ParserDslSpecification = """
        // Define our keywords.
        _keywords: 'and', 'asIs', 'based', 'binary', 'bounded', 'bounders', 'by', 'comments',
                   'containing', 'decimal', 'defaults', 'digits', 'double', 'dsl', 'escape',
                   'excluding', 'extensible', 'greekLetters', 'greekLowers', 'greekUppers',
                   'hex', 'identifiers', 'including', 'integral', 'keywords', 'letters',
                   'line', 'lineEnd', 'lineEnds', 'lowerCase', 'multiChar', 'multiLine',
                   'no', 'not', 'numbers', 'numbers', 'octal', 'of', 'operators', 'or',
                   'predefined', 'quoted', 'raw', 'redact', 'repeat', 'report', 'separated',
                   'signs', 'single', 'standard', 'starting', 'strings', 'to', 'triple',
                   'upperCase', 'whitespace', 'with'
        _operators: comma

        // Common clauses.
        extraStringsIdsOrKeywordsClause:
        {
            comma >
            [ _string | _identifier | _keyword ] ?? 'Expecting a string or identifier here.'
        }{*}
        stringIdOrKeywordListClause:
        {
            [ _string | _identifier | _keyword ] ?? 'Expecting a string or identifier here.' >
            extraStringsIdsOrKeywordsClause
        }
        extraStringsOrKeywordsClause:
        {
            comma >
            [ _string | _keyword ] ?? 'Expecting a string or keyword here.'
        }{*}
        stringOrKeywordListClause:
        {
            [ _string | _keyword ] ?? 'Expecting a string or keyword here.' >
            extraStringsOrKeywordsClause
        }
        letterCaseStyleClause: [ lowerCase | upperCase | asIs ]{?}
        reportClause: [ report | redact ]{?}

        // Whitespace related clauses.
        whitespaceWithClause:
        {
            with > separated ?? 'Expecting "separated" to follow "with" here.' >
            lineEnds ?? 'Expecting "lineEnds" to follow "separated" here.'
        }{?}

        whitespaceClause:
        {
            whitespace > whitespaceWithClause > reportClause
        }

        // Bounder related clauses.
        ofClause:
        {
            of > _string ?? 'Expecting a string to follow "of" here.'
        }{?}

        boundersClause:
        {
            bounders > ofClause > reportClause
        }

        // Based number related clauses.
        numberBaseSuppressionClause:
        {
            no > [ hex | octal | binary ] ?? 'Expecting "hex", "octal" or "binary" to follow "no" here.'
        }{*}

        basedNumbersClause:
        {
            based > numbers ?? 'Expecting "numbers" to follow "based" here.' >
            numberBaseSuppressionClause > reportClause
        }

        // Operator related clauses.
        operatorsOrKeywordsModifierClause:
        {
            [ including | excluding ] > stringIdOrKeywordListClause
        }{?}

        sourcedOperatorsClause:
        {
            [ predefined | dsl ] > operators >
            operatorsOrKeywordsModifierClause >
            reportClause
        }
        operatorsClause:
        {
            operators > stringIdOrKeywordListClause > reportClause
        }

        // Identifier related clauses.
        identifierStartingClause:
        {
            starting > with ?? 'Expecting "with" to follow "start" here.' >
            stringOrKeywordListClause
        }{?}
        identifierContainsClause:
        {
            containing > stringOrKeywordListClause
        }{?}

        identifiersClause:
        {
            identifiers > identifierStartingClause >
            identifierContainsClause > letterCaseStyleClause >
            reportClause
        }

        // Keyword related clauses.
        sourcedKeywordsClause:
        {
            dsl > keywords > operatorsOrKeywordsModifierClause >
            letterCaseStyleClause > reportClause
        }
        keywordsClause:
        {
            keywords > stringIdOrKeywordListClause > letterCaseStyleClause > reportClause
        }

        // Number related clauses.
        numbersPrefixClause:
        {
            [ integral | decimal ] > numbers ?? 'Expecting "numbers" to follow "integral" or "decimal" here.'
        }
        numbersSuffixClause: { with > signs ?? 'Expecting "signs" to follow "with" here.' }{?}

        numbersClause:
        {
            [ numbersPrefixClause | numbers ] >
            numbersSuffixClause > reportClause
        }

        // String related clauses.
        quotedStringsClause:
        {
            quoted ?? 'Expecting "quoted" to follow "single", "double" or "triple" here.' >
            strings ?? 'Expecting "strings" to follow "quoted" here.'
        }
        singleQuotedStringsClause:
        {
            single > quotedStringsClause > multiChar{?}
        }
        doubleQuotedStringsClause:
        {
            double > quotedStringsClause
        }
        notMultiLineClause: { single > line ?? 'Expecting "line" to follow "single" here.' }{?}
        notExtensibleClause: { not > extensible ?? 'Expecting "extensible" to follow "not" here.' }{?}
        tripleQuotedStringsClause:
        {
            triple > quotedStringsClause > notMultiLineClause > notExtensibleClause 
        }
        genericStringsClause:
        {
            strings > bounded ?? 'Expecting "bounded" to follow "strings" here.' >
            by ?? 'Expecting "by" to follow "bounded" here.' >
            _string ?? 'Expecting a string to follow "by" here.' >
            multiLine{?}
        }
        stringsSpecificationClause:
        [
            singleQuotedStringsClause |
            doubleQuotedStringsClause | 
            tripleQuotedStringsClause |
            genericStringsClause
        ]
        repeatToEscapeClause:
        {
            repeat > to ?? 'Expecting "to" to follow "repeat" here.' >
            escape ?? 'Expecting "escape" to follow "to" here.' 
        }{?}

        stringsClause:
        {
            stringsSpecificationClause > raw{?} >
            repeatToEscapeClause >
            reportClause
        }

        // Comment related clauses.
        commentBounderClause:
        {
            _string ?? 'Expecting a string here.' >
            and ?? 'Expecting "and" here.' >
            [ _string | lineEnd ] ?? 'Expecting "lineEnd" or a string here.'
        }
        orBoundedByClause: { or > commentBounderClause }{*}

        standardCommentsClause:
        {
            standard > comments ?? 'Expecting "comments" to follow "standard" here.' >
            orBoundedByClause >
            reportClause
        }
        commentsClause:
        {
            comments > bounded ?? 'Expecting a "bounded by" clause here.' >
            by ?? 'Expecting "by" to follow "bounded" here.' >
            commentBounderClause >
            orBoundedByClause >
            reportClause
        }

        // Top-level clause.
        [
            whitespaceClause       => 'HandleWhitespaceClause' |
            basedNumbersClause     => 'HandleBasedNumbersClause' |
            boundersClause         => 'HandleBoundersClause' |
            sourcedOperatorsClause => 'HandleSourcedOperatorsClause' |
            operatorsClause        => 'HandleOperatorsClause' |
            identifiersClause      => 'HandleIdentifiersClause' |
            sourcedKeywordsClause  => 'HandleSourcedKeywordsClause' |
            keywordsClause         => 'HandleKeywordsClause' |
            numbersClause          => 'HandleNumbersClause' |
            stringsClause          => 'HandleStringsClause' |
            standardCommentsClause => 'HandleStandardCommentsClause' |
            commentsClause         => 'HandleCommentsClause'
        ] ?? 'Unsupported tokenizer type found.'
        """;

    private static readonly Dsl ParserDsl = LexicalDslFactory.CreateFrom(ParserDslSpecification);
}
