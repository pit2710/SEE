//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from CSharpCommentsGrammar.g4 by ANTLR 4.9.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.2")]
[System.CLSCompliant(false)]
public partial class CSharpCommentsGrammarLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, WS=7, Comment=8, PARAM=9, 
		TEXT=10, SHORT_COMMENT=11, EQUALS=12, LineComment=13, Classname=14, TEXT_SKIP=15, 
		CURLY_BRACKET_OPEN=16, CURLY_BRACKET_CLOSE=17, CLASS_LINK=18, PARAMREF=19;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "WS", "Comment", "PARAM", 
		"TEXT", "SHORT_COMMENT", "EQUALS", "LineComment", "Classname", "TEXT_SKIP", 
		"CURLY_BRACKET_OPEN", "CURLY_BRACKET_CLOSE", "CLASS_LINK", "PARAMREF"
	};


	public CSharpCommentsGrammarLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public CSharpCommentsGrammarLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'/// <summary>'", "'/// </summary>'", "'/// <returns>'", "'/// </returns>'", 
		"'</returns>'", "'public class'", null, null, null, null, "'//'", "'='", 
		null, null, null, "'{'", "'}'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, "WS", "Comment", "PARAM", "TEXT", 
		"SHORT_COMMENT", "EQUALS", "LineComment", "Classname", "TEXT_SKIP", "CURLY_BRACKET_OPEN", 
		"CURLY_BRACKET_CLOSE", "CLASS_LINK", "PARAMREF"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "CSharpCommentsGrammar.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static CSharpCommentsGrammarLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '\x15', '\x112', '\b', '\x1', '\x4', '\x2', '\t', '\x2', 
		'\x4', '\x3', '\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', 
		'\x5', '\x4', '\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', 
		'\t', '\b', '\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x4', '\v', 
		'\t', '\v', '\x4', '\f', '\t', '\f', '\x4', '\r', '\t', '\r', '\x4', '\xE', 
		'\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x4', '\x10', '\t', '\x10', '\x4', 
		'\x11', '\t', '\x11', '\x4', '\x12', '\t', '\x12', '\x4', '\x13', '\t', 
		'\x13', '\x4', '\x14', '\t', '\x14', '\x3', '\x2', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\x6', '\x3', '\x6', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', 
		'\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\b', '\x6', '\b', 
		'}', '\n', '\b', '\r', '\b', '\xE', '\b', '~', '\x3', '\b', '\x3', '\b', 
		'\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', 
		'\t', '\x3', '\t', '\x3', '\t', '\a', '\t', '\x8B', '\n', '\t', '\f', 
		'\t', '\xE', '\t', '\x8E', '\v', '\t', '\x3', '\t', '\x3', '\t', '\x6', 
		'\t', '\x92', '\n', '\t', '\r', '\t', '\xE', '\t', '\x93', '\x5', '\t', 
		'\x96', '\n', '\t', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', 
		'\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', 
		'\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', 
		'\a', '\n', '\xA7', '\n', '\n', '\f', '\n', '\xE', '\n', '\xAA', '\v', 
		'\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\a', '\n', 
		'\xB0', '\n', '\n', '\f', '\n', '\xE', '\n', '\xB3', '\v', '\n', '\x3', 
		'\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', 
		'\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\v', '\x3', '\v', '\x6', 
		'\v', '\xC0', '\n', '\v', '\r', '\v', '\xE', '\v', '\xC1', '\x3', '\f', 
		'\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\r', '\x3', 
		'\r', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', '\a', '\xE', 
		'\xCF', '\n', '\xE', '\f', '\xE', '\xE', '\xE', '\xD2', '\v', '\xE', '\x3', 
		'\xE', '\x3', '\xE', '\x3', '\xE', '\x3', '\xF', '\x3', '\xF', '\a', '\xF', 
		'\xD9', '\n', '\xF', '\f', '\xF', '\xE', '\xF', '\xDC', '\v', '\xF', '\x3', 
		'\x10', '\x3', '\x10', '\x6', '\x10', '\xE0', '\n', '\x10', '\r', '\x10', 
		'\xE', '\x10', '\xE1', '\x3', '\x10', '\x3', '\x10', '\x3', '\x11', '\x3', 
		'\x11', '\x3', '\x11', '\x3', '\x11', '\x3', '\x12', '\x3', '\x12', '\x3', 
		'\x13', '\x3', '\x13', '\x3', '\x13', '\x3', '\x13', '\x3', '\x13', '\x3', 
		'\x13', '\x3', '\x13', '\x3', '\x13', '\x3', '\x13', '\x3', '\x13', '\x3', 
		'\x13', '\x3', '\x13', '\x3', '\x13', '\x3', '\x13', '\x3', '\x13', '\x3', 
		'\x13', '\x3', '\x13', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', 
		'\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', 
		'\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', 
		'\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', 
		'\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\xD0', '\x2', 
		'\x15', '\x3', '\x3', '\x5', '\x4', '\a', '\x5', '\t', '\x6', '\v', '\a', 
		'\r', '\b', '\xF', '\t', '\x11', '\n', '\x13', '\v', '\x15', '\f', '\x17', 
		'\r', '\x19', '\xE', '\x1B', '\xF', '\x1D', '\x10', '\x1F', '\x11', '!', 
		'\x12', '#', '\x13', '%', '\x14', '\'', '\x15', '\x3', '\x2', '\f', '\x5', 
		'\x2', '\v', '\f', '\xF', '\xF', '\"', '\"', '\x5', '\x2', '\f', '\f', 
		'\xF', '\xF', '>', '>', '\b', '\x2', '#', '%', '/', ';', '?', '?', '\x43', 
		'\\', '\x61', '\x61', '\x63', '|', '\n', '\x2', '#', '%', '*', '+', '/', 
		';', '=', '=', '?', '?', '\x43', '\\', '\x61', '\x61', '\x63', '|', '\r', 
		'\x2', '$', '%', '(', '(', '*', '+', '-', '-', '/', ';', '=', '=', '?', 
		'?', '\x43', '\\', '\x61', '\x61', '\x63', '|', '~', '~', '\f', '\x2', 
		'%', '%', '(', '(', '*', '+', '/', ';', '=', '=', '?', '?', '\x43', '\\', 
		'\x61', '\x61', '\x63', '|', '~', '~', '\x5', '\x2', '\x43', '\\', '\x61', 
		'\x61', '\x63', '|', '\x6', '\x2', '\x32', ';', '\x43', '\\', '\x61', 
		'\x61', '\x63', '|', '\a', '\x2', '#', '%', '/', ';', '\x43', '\\', '\x61', 
		'\x61', '\x63', '|', '\n', '\x2', '#', '#', '%', '%', '*', '+', '/', ';', 
		'=', '=', '\x43', '\\', '\x61', '\x61', '\x63', '|', '\x2', '\x11B', '\x2', 
		'\x3', '\x3', '\x2', '\x2', '\x2', '\x2', '\x5', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\a', '\x3', '\x2', '\x2', '\x2', '\x2', '\t', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\v', '\x3', '\x2', '\x2', '\x2', '\x2', '\r', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\xF', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x11', '\x3', '\x2', '\x2', '\x2', '\x2', '\x13', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x15', '\x3', '\x2', '\x2', '\x2', '\x2', '\x17', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x19', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x1B', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1D', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x1F', '\x3', '\x2', '\x2', '\x2', '\x2', '!', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '#', '\x3', '\x2', '\x2', '\x2', '\x2', '%', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\'', '\x3', '\x2', '\x2', '\x2', '\x3', ')', 
		'\x3', '\x2', '\x2', '\x2', '\x5', '\x37', '\x3', '\x2', '\x2', '\x2', 
		'\a', '\x46', '\x3', '\x2', '\x2', '\x2', '\t', 'T', '\x3', '\x2', '\x2', 
		'\x2', '\v', '\x63', '\x3', '\x2', '\x2', '\x2', '\r', 'n', '\x3', '\x2', 
		'\x2', '\x2', '\xF', '|', '\x3', '\x2', '\x2', '\x2', '\x11', '\x95', 
		'\x3', '\x2', '\x2', '\x2', '\x13', '\x97', '\x3', '\x2', '\x2', '\x2', 
		'\x15', '\xBD', '\x3', '\x2', '\x2', '\x2', '\x17', '\xC3', '\x3', '\x2', 
		'\x2', '\x2', '\x19', '\xC8', '\x3', '\x2', '\x2', '\x2', '\x1B', '\xCA', 
		'\x3', '\x2', '\x2', '\x2', '\x1D', '\xD6', '\x3', '\x2', '\x2', '\x2', 
		'\x1F', '\xDD', '\x3', '\x2', '\x2', '\x2', '!', '\xE5', '\x3', '\x2', 
		'\x2', '\x2', '#', '\xE9', '\x3', '\x2', '\x2', '\x2', '%', '\xEB', '\x3', 
		'\x2', '\x2', '\x2', '\'', '\xFC', '\x3', '\x2', '\x2', '\x2', ')', '*', 
		'\a', '\x31', '\x2', '\x2', '*', '+', '\a', '\x31', '\x2', '\x2', '+', 
		',', '\a', '\x31', '\x2', '\x2', ',', '-', '\a', '\"', '\x2', '\x2', '-', 
		'.', '\a', '>', '\x2', '\x2', '.', '/', '\a', 'u', '\x2', '\x2', '/', 
		'\x30', '\a', 'w', '\x2', '\x2', '\x30', '\x31', '\a', 'o', '\x2', '\x2', 
		'\x31', '\x32', '\a', 'o', '\x2', '\x2', '\x32', '\x33', '\a', '\x63', 
		'\x2', '\x2', '\x33', '\x34', '\a', 't', '\x2', '\x2', '\x34', '\x35', 
		'\a', '{', '\x2', '\x2', '\x35', '\x36', '\a', '@', '\x2', '\x2', '\x36', 
		'\x4', '\x3', '\x2', '\x2', '\x2', '\x37', '\x38', '\a', '\x31', '\x2', 
		'\x2', '\x38', '\x39', '\a', '\x31', '\x2', '\x2', '\x39', ':', '\a', 
		'\x31', '\x2', '\x2', ':', ';', '\a', '\"', '\x2', '\x2', ';', '<', '\a', 
		'>', '\x2', '\x2', '<', '=', '\a', '\x31', '\x2', '\x2', '=', '>', '\a', 
		'u', '\x2', '\x2', '>', '?', '\a', 'w', '\x2', '\x2', '?', '@', '\a', 
		'o', '\x2', '\x2', '@', '\x41', '\a', 'o', '\x2', '\x2', '\x41', '\x42', 
		'\a', '\x63', '\x2', '\x2', '\x42', '\x43', '\a', 't', '\x2', '\x2', '\x43', 
		'\x44', '\a', '{', '\x2', '\x2', '\x44', '\x45', '\a', '@', '\x2', '\x2', 
		'\x45', '\x6', '\x3', '\x2', '\x2', '\x2', '\x46', 'G', '\a', '\x31', 
		'\x2', '\x2', 'G', 'H', '\a', '\x31', '\x2', '\x2', 'H', 'I', '\a', '\x31', 
		'\x2', '\x2', 'I', 'J', '\a', '\"', '\x2', '\x2', 'J', 'K', '\a', '>', 
		'\x2', '\x2', 'K', 'L', '\a', 't', '\x2', '\x2', 'L', 'M', '\a', 'g', 
		'\x2', '\x2', 'M', 'N', '\a', 'v', '\x2', '\x2', 'N', 'O', '\a', 'w', 
		'\x2', '\x2', 'O', 'P', '\a', 't', '\x2', '\x2', 'P', 'Q', '\a', 'p', 
		'\x2', '\x2', 'Q', 'R', '\a', 'u', '\x2', '\x2', 'R', 'S', '\a', '@', 
		'\x2', '\x2', 'S', '\b', '\x3', '\x2', '\x2', '\x2', 'T', 'U', '\a', '\x31', 
		'\x2', '\x2', 'U', 'V', '\a', '\x31', '\x2', '\x2', 'V', 'W', '\a', '\x31', 
		'\x2', '\x2', 'W', 'X', '\a', '\"', '\x2', '\x2', 'X', 'Y', '\a', '>', 
		'\x2', '\x2', 'Y', 'Z', '\a', '\x31', '\x2', '\x2', 'Z', '[', '\a', 't', 
		'\x2', '\x2', '[', '\\', '\a', 'g', '\x2', '\x2', '\\', ']', '\a', 'v', 
		'\x2', '\x2', ']', '^', '\a', 'w', '\x2', '\x2', '^', '_', '\a', 't', 
		'\x2', '\x2', '_', '`', '\a', 'p', '\x2', '\x2', '`', '\x61', '\a', 'u', 
		'\x2', '\x2', '\x61', '\x62', '\a', '@', '\x2', '\x2', '\x62', '\n', '\x3', 
		'\x2', '\x2', '\x2', '\x63', '\x64', '\a', '>', '\x2', '\x2', '\x64', 
		'\x65', '\a', '\x31', '\x2', '\x2', '\x65', '\x66', '\a', 't', '\x2', 
		'\x2', '\x66', 'g', '\a', 'g', '\x2', '\x2', 'g', 'h', '\a', 'v', '\x2', 
		'\x2', 'h', 'i', '\a', 'w', '\x2', '\x2', 'i', 'j', '\a', 't', '\x2', 
		'\x2', 'j', 'k', '\a', 'p', '\x2', '\x2', 'k', 'l', '\a', 'u', '\x2', 
		'\x2', 'l', 'm', '\a', '@', '\x2', '\x2', 'm', '\f', '\x3', '\x2', '\x2', 
		'\x2', 'n', 'o', '\a', 'r', '\x2', '\x2', 'o', 'p', '\a', 'w', '\x2', 
		'\x2', 'p', 'q', '\a', '\x64', '\x2', '\x2', 'q', 'r', '\a', 'n', '\x2', 
		'\x2', 'r', 's', '\a', 'k', '\x2', '\x2', 's', 't', '\a', '\x65', '\x2', 
		'\x2', 't', 'u', '\a', '\"', '\x2', '\x2', 'u', 'v', '\a', '\x65', '\x2', 
		'\x2', 'v', 'w', '\a', 'n', '\x2', '\x2', 'w', 'x', '\a', '\x63', '\x2', 
		'\x2', 'x', 'y', '\a', 'u', '\x2', '\x2', 'y', 'z', '\a', 'u', '\x2', 
		'\x2', 'z', '\xE', '\x3', '\x2', '\x2', '\x2', '{', '}', '\t', '\x2', 
		'\x2', '\x2', '|', '{', '\x3', '\x2', '\x2', '\x2', '}', '~', '\x3', '\x2', 
		'\x2', '\x2', '~', '|', '\x3', '\x2', '\x2', '\x2', '~', '\x7F', '\x3', 
		'\x2', '\x2', '\x2', '\x7F', '\x80', '\x3', '\x2', '\x2', '\x2', '\x80', 
		'\x81', '\b', '\b', '\x2', '\x2', '\x81', '\x10', '\x3', '\x2', '\x2', 
		'\x2', '\x82', '\x83', '\a', '\x31', '\x2', '\x2', '\x83', '\x84', '\a', 
		'\x31', '\x2', '\x2', '\x84', '\x96', '\a', '\x31', '\x2', '\x2', '\x85', 
		'\x86', '\a', '\x31', '\x2', '\x2', '\x86', '\x87', '\a', '\x31', '\x2', 
		'\x2', '\x87', '\x88', '\a', '\x31', '\x2', '\x2', '\x88', '\x8C', '\x3', 
		'\x2', '\x2', '\x2', '\x89', '\x8B', '\n', '\x3', '\x2', '\x2', '\x8A', 
		'\x89', '\x3', '\x2', '\x2', '\x2', '\x8B', '\x8E', '\x3', '\x2', '\x2', 
		'\x2', '\x8C', '\x8A', '\x3', '\x2', '\x2', '\x2', '\x8C', '\x8D', '\x3', 
		'\x2', '\x2', '\x2', '\x8D', '\x8F', '\x3', '\x2', '\x2', '\x2', '\x8E', 
		'\x8C', '\x3', '\x2', '\x2', '\x2', '\x8F', '\x91', '\t', '\x4', '\x2', 
		'\x2', '\x90', '\x92', '\t', '\x5', '\x2', '\x2', '\x91', '\x90', '\x3', 
		'\x2', '\x2', '\x2', '\x92', '\x93', '\x3', '\x2', '\x2', '\x2', '\x93', 
		'\x91', '\x3', '\x2', '\x2', '\x2', '\x93', '\x94', '\x3', '\x2', '\x2', 
		'\x2', '\x94', '\x96', '\x3', '\x2', '\x2', '\x2', '\x95', '\x82', '\x3', 
		'\x2', '\x2', '\x2', '\x95', '\x85', '\x3', '\x2', '\x2', '\x2', '\x96', 
		'\x12', '\x3', '\x2', '\x2', '\x2', '\x97', '\x98', '\a', '>', '\x2', 
		'\x2', '\x98', '\x99', '\a', 'r', '\x2', '\x2', '\x99', '\x9A', '\a', 
		'\x63', '\x2', '\x2', '\x9A', '\x9B', '\a', 't', '\x2', '\x2', '\x9B', 
		'\x9C', '\a', '\x63', '\x2', '\x2', '\x9C', '\x9D', '\a', 'o', '\x2', 
		'\x2', '\x9D', '\x9E', '\a', '\"', '\x2', '\x2', '\x9E', '\x9F', '\a', 
		'p', '\x2', '\x2', '\x9F', '\xA0', '\a', '\x63', '\x2', '\x2', '\xA0', 
		'\xA1', '\a', 'o', '\x2', '\x2', '\xA1', '\xA2', '\a', 'g', '\x2', '\x2', 
		'\xA2', '\xA3', '\a', '?', '\x2', '\x2', '\xA3', '\xA4', '\a', '$', '\x2', 
		'\x2', '\xA4', '\xA8', '\x3', '\x2', '\x2', '\x2', '\xA5', '\xA7', '\x5', 
		'\x15', '\v', '\x2', '\xA6', '\xA5', '\x3', '\x2', '\x2', '\x2', '\xA7', 
		'\xAA', '\x3', '\x2', '\x2', '\x2', '\xA8', '\xA6', '\x3', '\x2', '\x2', 
		'\x2', '\xA8', '\xA9', '\x3', '\x2', '\x2', '\x2', '\xA9', '\xAB', '\x3', 
		'\x2', '\x2', '\x2', '\xAA', '\xA8', '\x3', '\x2', '\x2', '\x2', '\xAB', 
		'\xAC', '\a', '$', '\x2', '\x2', '\xAC', '\xAD', '\a', '@', '\x2', '\x2', 
		'\xAD', '\xB1', '\x3', '\x2', '\x2', '\x2', '\xAE', '\xB0', '\x5', '\x15', 
		'\v', '\x2', '\xAF', '\xAE', '\x3', '\x2', '\x2', '\x2', '\xB0', '\xB3', 
		'\x3', '\x2', '\x2', '\x2', '\xB1', '\xAF', '\x3', '\x2', '\x2', '\x2', 
		'\xB1', '\xB2', '\x3', '\x2', '\x2', '\x2', '\xB2', '\xB4', '\x3', '\x2', 
		'\x2', '\x2', '\xB3', '\xB1', '\x3', '\x2', '\x2', '\x2', '\xB4', '\xB5', 
		'\a', '>', '\x2', '\x2', '\xB5', '\xB6', '\a', '\x31', '\x2', '\x2', '\xB6', 
		'\xB7', '\a', 'r', '\x2', '\x2', '\xB7', '\xB8', '\a', '\x63', '\x2', 
		'\x2', '\xB8', '\xB9', '\a', 't', '\x2', '\x2', '\xB9', '\xBA', '\a', 
		'\x63', '\x2', '\x2', '\xBA', '\xBB', '\a', 'o', '\x2', '\x2', '\xBB', 
		'\xBC', '\a', '@', '\x2', '\x2', '\xBC', '\x14', '\x3', '\x2', '\x2', 
		'\x2', '\xBD', '\xBF', '\t', '\x6', '\x2', '\x2', '\xBE', '\xC0', '\t', 
		'\a', '\x2', '\x2', '\xBF', '\xBE', '\x3', '\x2', '\x2', '\x2', '\xC0', 
		'\xC1', '\x3', '\x2', '\x2', '\x2', '\xC1', '\xBF', '\x3', '\x2', '\x2', 
		'\x2', '\xC1', '\xC2', '\x3', '\x2', '\x2', '\x2', '\xC2', '\x16', '\x3', 
		'\x2', '\x2', '\x2', '\xC3', '\xC4', '\a', '\x31', '\x2', '\x2', '\xC4', 
		'\xC5', '\a', '\x31', '\x2', '\x2', '\xC5', '\xC6', '\x3', '\x2', '\x2', 
		'\x2', '\xC6', '\xC7', '\b', '\f', '\x2', '\x2', '\xC7', '\x18', '\x3', 
		'\x2', '\x2', '\x2', '\xC8', '\xC9', '\a', '?', '\x2', '\x2', '\xC9', 
		'\x1A', '\x3', '\x2', '\x2', '\x2', '\xCA', '\xCB', '\a', '\x31', '\x2', 
		'\x2', '\xCB', '\xCC', '\a', ',', '\x2', '\x2', '\xCC', '\xD0', '\x3', 
		'\x2', '\x2', '\x2', '\xCD', '\xCF', '\v', '\x2', '\x2', '\x2', '\xCE', 
		'\xCD', '\x3', '\x2', '\x2', '\x2', '\xCF', '\xD2', '\x3', '\x2', '\x2', 
		'\x2', '\xD0', '\xD1', '\x3', '\x2', '\x2', '\x2', '\xD0', '\xCE', '\x3', 
		'\x2', '\x2', '\x2', '\xD1', '\xD3', '\x3', '\x2', '\x2', '\x2', '\xD2', 
		'\xD0', '\x3', '\x2', '\x2', '\x2', '\xD3', '\xD4', '\a', ',', '\x2', 
		'\x2', '\xD4', '\xD5', '\a', '\x31', '\x2', '\x2', '\xD5', '\x1C', '\x3', 
		'\x2', '\x2', '\x2', '\xD6', '\xDA', '\t', '\b', '\x2', '\x2', '\xD7', 
		'\xD9', '\t', '\t', '\x2', '\x2', '\xD8', '\xD7', '\x3', '\x2', '\x2', 
		'\x2', '\xD9', '\xDC', '\x3', '\x2', '\x2', '\x2', '\xDA', '\xD8', '\x3', 
		'\x2', '\x2', '\x2', '\xDA', '\xDB', '\x3', '\x2', '\x2', '\x2', '\xDB', 
		'\x1E', '\x3', '\x2', '\x2', '\x2', '\xDC', '\xDA', '\x3', '\x2', '\x2', 
		'\x2', '\xDD', '\xDF', '\t', '\n', '\x2', '\x2', '\xDE', '\xE0', '\t', 
		'\v', '\x2', '\x2', '\xDF', '\xDE', '\x3', '\x2', '\x2', '\x2', '\xE0', 
		'\xE1', '\x3', '\x2', '\x2', '\x2', '\xE1', '\xDF', '\x3', '\x2', '\x2', 
		'\x2', '\xE1', '\xE2', '\x3', '\x2', '\x2', '\x2', '\xE2', '\xE3', '\x3', 
		'\x2', '\x2', '\x2', '\xE3', '\xE4', '\b', '\x10', '\x2', '\x2', '\xE4', 
		' ', '\x3', '\x2', '\x2', '\x2', '\xE5', '\xE6', '\a', '}', '\x2', '\x2', 
		'\xE6', '\xE7', '\x3', '\x2', '\x2', '\x2', '\xE7', '\xE8', '\b', '\x11', 
		'\x2', '\x2', '\xE8', '\"', '\x3', '\x2', '\x2', '\x2', '\xE9', '\xEA', 
		'\a', '\x7F', '\x2', '\x2', '\xEA', '$', '\x3', '\x2', '\x2', '\x2', '\xEB', 
		'\xEC', '\a', '>', '\x2', '\x2', '\xEC', '\xED', '\a', 'u', '\x2', '\x2', 
		'\xED', '\xEE', '\a', 'g', '\x2', '\x2', '\xEE', '\xEF', '\a', 'g', '\x2', 
		'\x2', '\xEF', '\xF0', '\a', '\"', '\x2', '\x2', '\xF0', '\xF1', '\a', 
		'\x65', '\x2', '\x2', '\xF1', '\xF2', '\a', 't', '\x2', '\x2', '\xF2', 
		'\xF3', '\a', 'g', '\x2', '\x2', '\xF3', '\xF4', '\a', 'h', '\x2', '\x2', 
		'\xF4', '\xF5', '\a', '?', '\x2', '\x2', '\xF5', '\xF6', '\a', '$', '\x2', 
		'\x2', '\xF6', '\xF7', '\x3', '\x2', '\x2', '\x2', '\xF7', '\xF8', '\x5', 
		'\x15', '\v', '\x2', '\xF8', '\xF9', '\a', '$', '\x2', '\x2', '\xF9', 
		'\xFA', '\a', '\x31', '\x2', '\x2', '\xFA', '\xFB', '\a', '@', '\x2', 
		'\x2', '\xFB', '&', '\x3', '\x2', '\x2', '\x2', '\xFC', '\xFD', '\a', 
		'>', '\x2', '\x2', '\xFD', '\xFE', '\a', 'r', '\x2', '\x2', '\xFE', '\xFF', 
		'\a', '\x63', '\x2', '\x2', '\xFF', '\x100', '\a', 't', '\x2', '\x2', 
		'\x100', '\x101', '\a', '\x63', '\x2', '\x2', '\x101', '\x102', '\a', 
		'o', '\x2', '\x2', '\x102', '\x103', '\a', 't', '\x2', '\x2', '\x103', 
		'\x104', '\a', 'g', '\x2', '\x2', '\x104', '\x105', '\a', 'h', '\x2', 
		'\x2', '\x105', '\x106', '\a', '\"', '\x2', '\x2', '\x106', '\x107', '\a', 
		'p', '\x2', '\x2', '\x107', '\x108', '\a', '\x63', '\x2', '\x2', '\x108', 
		'\x109', '\a', 'o', '\x2', '\x2', '\x109', '\x10A', '\a', 'g', '\x2', 
		'\x2', '\x10A', '\x10B', '\a', '?', '\x2', '\x2', '\x10B', '\x10C', '\a', 
		'$', '\x2', '\x2', '\x10C', '\x10D', '\x3', '\x2', '\x2', '\x2', '\x10D', 
		'\x10E', '\x5', '\x15', '\v', '\x2', '\x10E', '\x10F', '\a', '$', '\x2', 
		'\x2', '\x10F', '\x110', '\a', '\x31', '\x2', '\x2', '\x110', '\x111', 
		'\a', '@', '\x2', '\x2', '\x111', '(', '\x3', '\x2', '\x2', '\x2', '\r', 
		'\x2', '~', '\x8C', '\x93', '\x95', '\xA8', '\xB1', '\xC1', '\xD0', '\xDA', 
		'\xE1', '\x3', '\b', '\x2', '\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
