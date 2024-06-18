﻿using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace SEE.Scanner
{
    /// <summary>
    /// Represents a language a <see cref="SEEToken.Type"/> is in.
    /// Symbolic names for the antlr lexer are specified here.
    /// </summary>
    public class TokenLanguage
    {
        /// <summary>
        /// Default number of spaces a tab is equivalent to.
        /// </summary>
        private const int defaultTabWidth = 4;

        /// <summary>
        /// Language-independent symbolic name for the end of file token.
        /// </summary>
        private const string eof = "EOF";

        /// <summary>
        /// File extensions which apply for the given language.
        /// May not intersect any other languages file extensions.
        /// </summary>
        public ISet<string> FileExtensions { get; }

        /// <summary>
        /// Name of the antlr lexer file the keywords were taken from.
        /// </summary>
        public string LexerFileName { get; }

        /// <summary>
        /// Number of spaces equivalent to a tab in this language.
        /// If not specified, this will be <see cref="defaultTabWidth"/>.
        /// </summary>
        public int TabWidth { get; }

        /// <summary>
        /// Symbolic names for comments of a language, including block, line, and documentation comments.
        /// </summary>
        public ISet<string> Comments { get; }

        /// <summary>
        /// Symbolic names for keywords of a language. This also includes boolean literals and null literals.
        /// </summary>
        public ISet<string> Keywords { get; }

        /// <summary>
        /// Symbolic names for branch keywords of a language.
        /// </summary>
        public ISet<string> BranchKeywords { get; }

        /// <summary>
        /// Symbolic names for number literals of a language. This includes integer literals, floating point literals, etc.
        /// </summary>
        public ISet<string> NumberLiterals { get; }

        /// <summary>
        /// Symbolic names for string literals of a language. Also includes character literals.
        /// </summary>
        public ISet<string> StringLiterals { get; }

        /// <summary>
        /// Symbolic names for separators and operators of a language.
        /// </summary>
        public ISet<string> Punctuation { get; }

        /// <summary>
        /// Symbolic names for identifiers in a language.
        /// </summary>
        public ISet<string> Identifiers { get; }

        /// <summary>
        /// Symbolic names for whitespace in a language, excluding newlines.
        /// </summary>
        public ISet<string> Whitespace { get; }

        /// <summary>
        /// Symbolic names for newlines in a language.
        /// </summary>
        public ISet<string> Newline { get; }

        #region Java Language

        /// <summary>
        /// Name of the Java antlr grammar lexer.
        /// </summary>
        private const string javaFileName = "Java9Lexer.g4";

        /// <summary>
        /// Set of java file extensions.
        /// </summary>
        private static readonly HashSet<string> javaExtensions = new()
        {
            "java"
        };

        /// <summary>
        /// Set of antlr type names for Java keywords excluding <see cref="javaBranchKeywords"/>.
        /// </summary>
        private static readonly HashSet<string> javaKeywords = new()
        {
            "ABSTRACT", "ASSERT", "BOOLEAN", "BREAK",  "BYTE", "CASE", "CATCH", "CHAR", "CLASS", "CONST", "CONTINUE",
            "DEFAULT", "DO", "DOUBLE", "ELSE", "ENUM", "EXPORTS", "EXTENDS", "FINAL", "FINALLY", "FLOAT",
            "GOTO", "IMPLEMENTS", "IMPORT", "INSTANCEOF", "INT", "INTERFACE", "LONG", "MODULE", "NATIVE", "NEW",
            "OPEN", "OPERNS", "PACKAGE", "PRIVATE", "PROTECTED", "PROVIDES", "PUBLIC", "REQUIRES", "RETURN", "SHORT",
            "STATIC", "STRICTFP", "SUPER", "SYNCHRONIZED", "THIS", "THROW", "THROWS", "TO", "TRANSIENT",
            "TRANSITIVE", "USES", "VOID", "VOLATILE", "WITH", "UNDER_SCORE",
            "BooleanLiteral", "NullLiteral"
        };

        /// <summary>
        /// Set of antlr type names for Java branch keywords.
        /// </summary>
        private static readonly HashSet<string> javaBranchKeywords = new()
        {
            "FOR", "IF", "SWITCH", "TRY", "WHILE"
        };

        /// <summary>
        /// Set of antlr type names for Java integer and floating point literals.
        /// </summary>
        private static readonly HashSet<string> javaNumbers = new() { "IntegerLiteral", "FloatingPointLiteral" };

        /// <summary>Set of antlr type names for Java character and string literals.</summary>
        private static readonly HashSet<string> javaStrings = new() { "CharacterLiteral", "StringLiteral" };

        /// <summary>Set of antlr type names for Java separators and operators.</summary>
        private static readonly HashSet<string> javaPunctuation = new()
        {
            "LPAREN", "RPAREN", "LBRACE",
            "RBRACE", "LBRACK", "RBRACK", "SEMI", "COMMA", "DOT", "ELLIPSIS", "AT", "COLONCOLON",
            "ASSIGN", "GT", "LT", "BANG", "TILDE", "QUESTION", "COLON", "ARROW", "EQUAL", "LE", "GE", "NOTEQUAL", "AND",
            "OR", "INC", "DEC", "ADD", "SUB", "MUL", "DIV", "BITAND", "BITOR", "CARET", "MOD",
            "ADD_ASSIGN", "SUB_ASSIGN", "MUL_ASSIGN", "DIV_ASSIGN", "AND_ASSIGN", "OR_ASSIGN", "XOR_ASSIGN",
            "MOD_ASSIGN", "LSHIFT_ASSIGN", "RSHIFT_ASSIGN", "URSHIFT_ASSIGN"
        };

        /// <summary>Set of antlr type names for Java identifiers.</summary>
        private static readonly HashSet<string> javaIdentifiers = new() { "Identifier" };

        /// <summary>
        /// Set of antlr type names for Java whitespace.
        /// </summary>
        private static readonly HashSet<string> javaWhitespace = new() { "WS" };

        /// <summary>
        /// Set of antlr type names for Java newlines.
        /// </summary>
        private static readonly HashSet<string> javaNewlines = new() { "NEWLINE" };

        /// <summary>
        /// Set of antlr type names for Java comments.
        /// </summary>
        private static readonly HashSet<string> javaComments = new() { "COMMENT", "LINE_COMMENT" };

        #endregion

        #region C# Language

        /// <summary>
        /// Name of the C# antlr grammar lexer.
        /// </summary>
        private const string cSharpFileName = "CSharpLexer.g4";

        /// <summary>
        /// Set of CSharp file extensions.
        /// </summary>
        private static readonly HashSet<string> cSharpExtensions = new()
        {
            "cs"
        };

        /// <summary>
        /// Set of antlr type names for CSharp keywords excluding <see cref="cSharpBranchKeywords"/>.
        /// </summary>
        private static readonly HashSet<string> cSharpKeywords = new()
        {
            // General keywords
            "ABSTRACT", "ADD", "ALIAS", "ARGLIST", "AS", "ASCENDING", "ASYNC", "AWAIT", "BASE", "BOOL", "BREAK", "BY",
            "BYTE", "CASE", "CATCH", "CHAR", "CHECKED", "CLASS", "CONST", "CONTINUE", "DECIMAL", "DEFAULT", "DELEGATE",
            "DESCENDING", "DO", "DOUBLE", "DYNAMIC", "ELSE", "ENUM", "EQUALS", "EVENT", "EXPLICIT", "EXTERN", "FALSE",
            "FINALLY", "FIXED", "FLOAT", "FROM", "GET", "GOTO", "GROUP", "IMPLICIT", "IN", "INT",
            "INTERFACE", "INTERNAL", "INTO", "IS", "JOIN", "LET", "LOCK", "LONG", "NAMEOF", "NAMESPACE", "NEW", "NULL_",
            "OBJECT", "ON", "OPERATOR", "ORDERBY", "OUT", "OVERRIDE", "PARAMS", "PARTIAL", "PRIVATE", "PROTECTED",
            "PUBLIC", "READONLY", "REF", "REMOVE", "RETURN", "SBYTE", "SEALED", "SELECT", "SET", "SHORT", "SIZEOF",
            "STACKALLOC", "STATIC", "STRING", "STRUCT", "THIS", "THROW", "TRUE", "TYPEOF", "UINT",
            "ULONG", "UNCHECKED", "UNMANAGED", "UNSAFE", "USHORT", "USING", "VAR", "VIRTUAL", "VOID", "VOLATILE", "WHEN",
            "WHERE", "YIELD", "SHARP",
            // Directive keywords (anything within a directive is treated as a keyword, similar to IDEs
            "DIRECTIVE_TRUE", "DIRECTIVE_FALSE", "DEFINE", "UNDEF", "DIRECTIVE_IF",
            "ELIF", "DIRECTIVE_ELSE", "ENDIF", "LINE", "ERROR", "WARNING", "REGION", "ENDREGION", "PRAGMA", "NULLABLE",
            "DIRECTIVE_DEFAULT", "DIRECTIVE_HIDDEN", "DIRECTIVE_OPEN_PARENS", "DIRECTIVE_CLOSE_PARENS", "DIRECTIVE_BANG",
            "DIRECTIVE_OP_EQ", "DIRECTIVE_OP_NE", "DIRECTIVE_OP_AND", "DIRECTIVE_OP_OR", "CONDITIONAL_SYMBOL",
        };

        /// <summary>
        /// Set of antlr type names for CSharp branch keywords.
        /// </summary>
        private static readonly HashSet<string> cSharpBranchKeywords = new()
        {
            "FOR", "FOREACH", "IF", "SWITCH", "TRY", "WHILE"
        };

        /// <summary>
        /// Set of antlr type names for CSharp integer and floating point literals.
        /// </summary>
        private static readonly HashSet<string> cSharpNumbers = new()
        {
            "LITERAL_ACCESS", "INTEGER_LITERAL", "HEX_INTEGER_LITERAL", "BIN_INTEGER_LITERAL", "REAL_LITERAL", "DIGITS"
        };

        /// <summary>Set of antlr type names for CSharp character and string literals.</summary>
        private static readonly HashSet<string> cSharpStrings = new()
        {
            "CHARACTER_LITERAL", "REGULAR_STRING", "VERBATIUM_STRING", "INTERPOLATED_REGULAR_STRING_START",
            "INTERPOLATED_VERBATIUM_STRING_START", "VERBATIUM_DOUBLE_QUOTE_INSIDE",
            "DOUBLE_QUOTE_INSIDE", "REGULAR_STRING_INSIDE", "VERBATIUM_INSIDE_STRING"
        };

        /// <summary>Set of antlr type names for CSharp separators and operators.</summary>
        private static readonly HashSet<string> cSharpPunctuation = new()
        {
            "OPEN_BRACE", "CLOSE_BRACE", "CLOSE_BRACE_INSIDE", "OPEN_BRACKET",
            "CLOSE_BRACKET", "OPEN_PARENS", "CLOSE_PARENS", "DOT", "COMMA", "FORMAT_STRING", "COLON", "SEMICOLON", "PLUS", "MINUS", "STAR", "DIV",
            "PERCENT", "AMP", "BITWISE_OR", "CARET", "BANG", "TILDE", "ASSIGNMENT", "LT", "GT", "INTERR", "DOUBLE_COLON",
            "OP_COALESCING", "OP_INC", "OP_DEC", "OP_AND", "OP_OR", "OP_PTR", "OP_EQ", "OP_NE", "OP_LE", "OP_GE", "OP_ADD_ASSIGNMENT",
            "OP_SUB_ASSIGNMENT", "OP_MULT_ASSIGNMENT", "OP_DIV_ASSIGNMENT", "OP_MOD_ASSIGNMENT", "OP_AND_ASSIGNMENT", "OP_OR_ASSIGNMENT",
            "OP_XOR_ASSIGNMENT", "OP_LEFT_SHIFT", "OP_LEFT_SHIFT_ASSIGNMENT", "OP_COALESCING_ASSIGNMENT", "OP_RANGE",
            "DOUBLE_CURLY_INSIDE", "OPEN_BRACE_INSIDE", "REGULAR_CHAR_INSIDE"
        };

        /// <summary>Set of antlr type names for CSharp identifiers.</summary>
        private static readonly HashSet<string> cSharpIdentifiers = new()
        {
            "IDENTIFIER", "TEXT"
        };

        /// <summary>
        /// Set of antlr type names for CSharp whitespace.
        /// </summary>
        private static readonly HashSet<string> cSharpWhitespace = new()
        {
            "WHITESPACES", "DIRECTIVE_WHITESPACES"
        };

        /// <summary>
        /// Set of antlr type names for CSharp newlines.
        /// </summary>
        private static readonly HashSet<string> cSharpNewlines = new()
        {
            "NL", "TEXT_NEW_LINE", "DIRECTIVE_NEW_LINE"
        };

        /// <summary>
        /// Set of antlr type names for Java comments.
        /// </summary>
        private static readonly HashSet<string> cSharpComments = new()
        {
            "SINGLE_LINE_DOC_COMMENT", "DELIMITED_DOC_COMMENT", "SINGLE_LINE_COMMENT", "DELIMITED_COMMENT",
            "DIRECTIVE_SINGLE_LINE_COMMENT"
        };

        #endregion

        #region CPP Language

        /// <summary>
        /// Name of the antlr grammar lexer.
        /// </summary>
        private const string cppFileName = "CPP14Lexer.g4";

        /// <summary>
        /// Set of CPP file extensions.
        /// </summary>
        private static readonly HashSet<string> cppExtensions = new()
        {
            "cpp", "cxx", "hpp"
        };

        /// <summary>
        /// Set of antlr type names for CPP keywords excluding <see cref="cppBranchKeywords"/>.
        /// </summary>
        private static readonly HashSet<string> cppKeywords = new()
        {
            "Alignas", "Alignof", "Asm", "Auto", "Bool", "Break", "Case", "Catch", "Continue",
            "Char", "Char16", "Char32", "Class", "Const", "Constexpr", "Const_cast",
            "Decltype", "Default", "Delete", "Do", "Double", "Dynamic_cast", "Else",
            "Enum", "Explicit", "Export", "Extern", "False_", "Final", "Float",
            "Friend", "Goto", "Inline", "Int", "Long", "Mutable", "Namespace",
            "New", "Noexcept", "Nullptr", "Operator", "Override", "Private", "Protected",
            "Public", "Register", "Reinterpret_cast", "Return", "Short", "Signed",
            "Sizeof", "Static", "Static_assert", "Static_cast", "Struct",
            "Template", "This", "Thread_local", "Throw", "True_", "Typedef",
            "Typeid_", "Typename_", "Union", "Unsigned", "Using", "Virtual", "Void",
            "Volatile", "Wchar",
            "BooleanLiteral", "PointerLiteral", "UserDefinedLiteral",
            "MultiLineMacro", "Directive"
        };

        /// <summary>
        /// Set of antlr type names for CPP branch keywords.
        /// </summary>
        private static readonly HashSet<string> cppBranchKeywords = new()
        {
            "For", "If", "Switch", "Try", "While"
        };

        /// <summary>
        /// Set of antlr type names for CPP integer and floating point literals.
        /// </summary>
        private static readonly HashSet<string> cppNumbers = new()
        {
            "IntegerLiteral", "FloatingLiteral", "DecimalLiteral", "OctalLiteral", "HexadecimalLiteral",
            "BinaryLiteral", "Integersuffix", "UserDefinedIntegerLiteral", "UserDefinedFloatingLiteral"
        };

        /// <summary>Set of antlr type names for CPP character and string literals.</summary>
        private static readonly HashSet<string> cppStrings = new()
        {
            "StringLiteral", "CharacterLiteral", "UserDefinedStringLiteral", "UserDefinedCharacterLiteral"
        };

        /// <summary>Set of antlr type names for CPP separators and operators.</summary>
        private static readonly HashSet<string> cppPunctuation = new()
        {
            "LeftParen", "RightParen", "LeftBracket",
            "RightBracket", "LeftBrace", "RightBrace", "Plus", "Minus", "Star", "Div",
            "Mod", "Caret", "And", "Or", "Tilde", "Not", "Assign", "Less", "Greater",
            "PlusAssign", "MinusAssign", "StarAssign", "DivAssign", "ModAssign", "XorAssign",
            "AndAssign", "OrAssign", "LeftShiftAssign", "RightShiftAssign", "Equal",
            "NotEqual", "LessEqual", "GreaterEqual", "AndAnd", "OrOr", "PlusPlus",
            "MinusMinus", "Comma", "ArrowStar", "Arrow", "Question", "Colon", "Doublecolon",
            "Semi", "Dot", "DotStar", "Ellipsis"
        };

        /// <summary>Set of antlr type names for CPP identifiers.</summary>
        private static readonly HashSet<string> cppIdentifiers = new() { "Identifier" };

        /// <summary>
        /// Set of antlr type names for CPP whitespace.
        /// </summary>
        private static readonly HashSet<string> cppWhitespace = new() { "Whitespace" };

        /// <summary>
        /// Set of antlr type names for CPP newlines.
        /// </summary>
        private static readonly HashSet<string> cppNewlines = new() { "Newline" };

        /// <summary>
        /// Set of antlr type names for CPP comments.
        /// </summary>
        private static readonly HashSet<string> cppComments = new() { "BlockComment", "LineComment" };

        #endregion

        #region Plain Text "Language"

        /// <summary>
        /// Name of the antlr grammar lexer.
        /// </summary>
        private const string plainFileName = "PlainTextLexer.g4";

        /// <summary>
        /// Set of plain text file extensions.
        /// Note that this is a special case, since this is the lexer we'll use when nothing else is available.
        /// </summary>
        private static readonly HashSet<string> plainExtensions = new();

        /// <summary> Set of antlr type names for keywords. There are none here. </summary>
        private static readonly HashSet<string> plainKeywords = new();

        /// <summary> Set of antlr type names for branch keywords. There are none here. </summary>
        private static readonly HashSet<string> plainBranchKeywords = new();

        /// <summary> Set of antlr type names for numbers. </summary>
        private static readonly HashSet<string> plainNumbers = new();

        /// <summary>Set of antlr type names for character and string literals. There are none here.
        private static readonly HashSet<string> plainStrings = new();

        /// <summary>Set of antlr type names for punctuation.</summary>
        private static readonly HashSet<string> plainPunctuation = new();

        /// <summary>Set of antlr type names for identifiers, which in this case is for normal words.</summary>
        private static readonly HashSet<string> plainIdentifiers = new()
        {
            "WORD", "PSEUDOWORD", "LETTERS", "SIGNS", "SPECIAL", "NUMBERS"
        };

        /// <summary>Set of antlr type names for whitespace.</summary>
        private static readonly HashSet<string> plainWhitespace = new() { "WHITESPACES" };

        /// <summary> Set of antlr type names for newlines. </summary>
        private static readonly HashSet<string> plainNewlines = new() { "NEWLINES" };

        /// <summary> Set of antlr type names for comments. There are none here.</summary>
        private static readonly HashSet<string> plainComments = new();

        #endregion

        #region Static Types

        public enum TokenLanguageType
        {
            Plain, 
            Java, 
            CSharp,
            CPP
        }

        public static TokenLanguage GetTokenLanguageByType(TokenLanguageType type)
        {
            switch(type)
            {
                case TokenLanguageType.Plain:
                    return Plain;
                case TokenLanguageType.Java: 
                    return Java;
                case TokenLanguageType.CSharp: 
                    return CSharp;
                case TokenLanguageType.CPP:
                    return CPP;
            }
            throw new Exception("Unknown Token Language Type.");
        }

        /// <summary>
        /// A list of all token languages there are.
        /// </summary>
        public static readonly IList<TokenLanguage> AllTokenLanguages = new List<TokenLanguage>();

        /// <summary>
        /// Token Language for Java.
        /// </summary>
        public static readonly TokenLanguage Java = new(javaFileName, javaExtensions, javaKeywords, javaBranchKeywords, javaNumbers,
                                                        javaStrings, javaPunctuation, javaIdentifiers, javaWhitespace, javaNewlines, javaComments);

        /// <summary>
        /// Token Language for C#.
        /// </summary>
        public static readonly TokenLanguage CSharp = new(cSharpFileName, cSharpExtensions, cSharpKeywords, cSharpBranchKeywords, cSharpNumbers,
                                                          cSharpStrings, cSharpPunctuation, cSharpIdentifiers, cSharpWhitespace, cSharpNewlines, cSharpComments);

        /// <summary>
        /// Token Language for CPP.
        /// </summary>
        public static readonly TokenLanguage CPP = new(cppFileName, cppExtensions, cppKeywords, cppBranchKeywords, cppNumbers,
                                                       cppStrings, cppPunctuation, cppIdentifiers, cppWhitespace, cppNewlines, cppComments);

        /// <summary>
        /// Token language for plain text.
        /// </summary>
        public static readonly TokenLanguage Plain = new(plainFileName, plainExtensions, plainKeywords, plainBranchKeywords, plainNumbers,
                                                         plainStrings, plainPunctuation, plainIdentifiers, plainWhitespace, plainNewlines, plainComments);

        #endregion

        /// <summary>
        /// Constructor for the token language.
        /// </summary>
        /// <remarks>Should never be accessible from outside this class.</remarks>
        /// <param name="lexerFileName">Name of this lexer grammar</param>
        /// <param name="fileExtensions">List of file extensions for this language</param>
        /// <param name="keywords">Keywords of this language</param>
        /// <param name="numberLiterals">Number literals of this language</param>
        /// <param name="stringLiterals">String literals of this language</param>
        /// <param name="punctuation">Punctuation for this language</param>
        /// <param name="identifiers">Identifiers for this language</param>
        /// <param name="whitespace">Whitespace for this language</param>
        /// <param name="newline">Newlines for this language</param>
        /// <param name="comments">Comments for this language</param>
        /// <param name="branchKeywords">Branches for this language</param>
        /// <param name="tabWidth">Number of spaces a tab is equivalent to</param>
        private TokenLanguage(string lexerFileName, ISet<string> fileExtensions, ISet<string> keywords, ISet<string> branchKeywords,
                              ISet<string> numberLiterals, ISet<string> stringLiterals, ISet<string> punctuation,
                              ISet<string> identifiers, ISet<string> whitespace, ISet<string> newline,
                              ISet<string> comments, int tabWidth = defaultTabWidth)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (AllTokenLanguages.Any(x => x.LexerFileName.Equals(lexerFileName) || x.FileExtensions.Overlaps(fileExtensions)))
            {
                throw new ArgumentException("Lexer file name and file extensions must be unique per language!");
            }
            if (AnyOverlaps())
            {
                throw new ArgumentException("Symbolic names may not appear in more than one set each!");
            }
#endif
            LexerFileName = lexerFileName;
            FileExtensions = fileExtensions;
            Keywords = keywords;
            BranchKeywords = branchKeywords;
            NumberLiterals = numberLiterals;
            StringLiterals = stringLiterals;
            Punctuation = punctuation;
            Identifiers = identifiers;
            Whitespace = whitespace;
            Newline = newline;
            Comments = comments;
            TabWidth = tabWidth;

            AllTokenLanguages.Add(this);

            // Check whether any of the symbolic names are used twice
            bool AnyOverlaps()
            {
                return keywords.Intersect(numberLiterals).Intersect(stringLiterals).Intersect(punctuation)
                               .Intersect(identifiers).Intersect(whitespace).Intersect(newline)
                               .Intersect(comments).Intersect(branchKeywords).Any();
            }
        }

        /// <summary>
        /// Returns the matching token language for the given <paramref name="lexerFileName"/>.
        /// If no matching token language is found, an exception will be thrown.
        /// </summary>
        /// <param name="lexerFileName">File name of the antlr lexer. Can be found in <c>lexer.GrammarFileName</c></param>
        /// <returns>The matching token language</returns>
        /// <exception cref="ArgumentException">If the given <paramref name="lexerFileName"/> is not supported.</exception>
        public static TokenLanguage FromLexerFileName(string lexerFileName)
        {
            return AllTokenLanguages.SingleOrDefault(x => x.LexerFileName.Equals(lexerFileName))
                   ?? throw new ArgumentException($"The given {nameof(lexerFileName)} is not of a supported grammar. Supported grammars are "
                                                  + string.Join(", ", AllTokenLanguages.Select(x => x.LexerFileName)));
        }

        /// <summary>
        /// Returns the matching token language for the given <paramref name="extension"/>.
        /// If no matching token language is found, the <see cref="PlainTextLexer"/> will be used, unless
        /// <paramref name="throwOnUnkown"/> is true.
        /// </summary>
        /// <param name="extension">File extension for the language.</param>
        /// <param name="throwOnUnknown">
        /// Whether to throw an exception when an unknown file extension is encountered.
        /// If this is false, the <see cref="PlainTextLexer"/> will be used instead in such a case.
        /// </param>
        /// <returns>The matching token language.</returns>
        /// <exception cref="ArgumentException">
        /// If the given <paramref name="extension"/> is not supported and <paramref name="throwOnUnknown"/> is true.
        /// </exception>
        public static TokenLanguage FromFileExtension(string extension, bool throwOnUnknown = false)
        {
            TokenLanguage target = AllTokenLanguages.SingleOrDefault(x => x.FileExtensions.Contains(extension));
            if (target == null)
            {
                if (throwOnUnknown)
                {
                    throw new ArgumentException("The given filetype is not supported. Supported filetypes are "
                                                + string.Join(", ", AllTokenLanguages.SelectMany(x => x.FileExtensions)));
                }

                target = Plain;
            }

            return target;
        }

        /// <summary>
        /// Creates a new lexer matching the <see cref="LexerFileName"/> of this language.
        /// </summary>
        /// <param name="content">The string which shall be parsed by the lexer.</param>
        /// <returns>the new matching lexer</returns>
        /// <exception cref="InvalidOperationException">If no lexer is defined for this language.</exception>
        public Lexer CreateLexer(string content)
        {
            ICharStream input = CharStreams.fromString(content);
            return LexerFileName switch
            {
                javaFileName => new Java9Lexer(input),
                cSharpFileName => new CSharpLexer(input),
                cppFileName => new CPP14Lexer(input),
                plainFileName => new PlainTextLexer(input),
                _ => throw new InvalidOperationException("No lexer defined for this language yet.")
            };
        }

        /// <summary>
        /// Returns the type of token this is.
        /// The type of token will be represented by the name of the collection it is in.
        /// Returns <c>null</c> if the token is not any known type.
        /// </summary>
        /// <param name="token">a symbolic name from the antlr lexer for this language</param>
        /// <returns>name of the type the given <paramref name="token"/> is, or <c>null</c> if it isn't known.</returns>
        public string TypeName(string token)
        {
            // We go through each category and check whether it contains the token.
            // I know that this looks like it may be abstracted because the same thing is done on different objects
            // in succession, but due to the usage of nameof() a refactoring of this kind would break it.
            if (Keywords.Contains(token))
            {
                return nameof(Keywords);
            }
            if (BranchKeywords.Contains(token))
            {
                return nameof(BranchKeywords);
            }
            if (NumberLiterals.Contains(token))
            {
                return nameof(NumberLiterals);
            }
            if (StringLiterals.Contains(token))
            {
                return nameof(StringLiterals);
            }
            if (Punctuation.Contains(token))
            {
                return nameof(Punctuation);
            }
            if (Identifiers.Contains(token))
            {
                return nameof(Identifiers);
            }
            if (Comments.Contains(token))
            {
                return nameof(Comments);
            }
            if (Whitespace.Contains(token))
            {
                return nameof(Whitespace);
            }
            if (Newline.Contains(token))
            {
                return nameof(Newline);
            }
            return eof.Equals(token) ? nameof(eof) : null;
        }
    }
}
