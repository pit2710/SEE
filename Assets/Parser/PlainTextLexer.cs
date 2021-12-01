//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from PlainTextLexer.g4 by ANTLR 4.9.2

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
public partial class PlainTextLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		PSEUDOWORD=1, WORD=2, NEWLINES=3, WHITESPACES=4, NUMBER=5, LETTERS=6, 
		SIGNS=7, SPECIAL=8;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"PSEUDOWORD", "WORD", "NEWLINES", "WHITESPACES", "NUMBER", "LETTERS", 
		"SIGNS", "SPECIAL", "OTHER", "WHITESPACE", "NEWLINE", "LETTER", "SIGN", 
		"DIGIT"
	};


	public PlainTextLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public PlainTextLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
	};
	private static readonly string[] _SymbolicNames = {
		null, "PSEUDOWORD", "WORD", "NEWLINES", "WHITESPACES", "NUMBER", "LETTERS", 
		"SIGNS", "SPECIAL"
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

	public override string GrammarFileName { get { return "PlainTextLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static PlainTextLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '\n', 'i', '\b', '\x1', '\x4', '\x2', '\t', '\x2', '\x4', 
		'\x3', '\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', '\x5', 
		'\x4', '\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', '\t', 
		'\b', '\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x4', '\v', '\t', 
		'\v', '\x4', '\f', '\t', '\f', '\x4', '\r', '\t', '\r', '\x4', '\xE', 
		'\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x3', '\x2', '\x3', '\x2', '\a', 
		'\x2', '\"', '\n', '\x2', '\f', '\x2', '\xE', '\x2', '%', '\v', '\x2', 
		'\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x6', '\x2', 
		'+', '\n', '\x2', '\r', '\x2', '\xE', '\x2', ',', '\x3', '\x3', '\a', 
		'\x3', '\x30', '\n', '\x3', '\f', '\x3', '\xE', '\x3', '\x33', '\v', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x6', '\x3', '\x38', '\n', 
		'\x3', '\r', '\x3', '\xE', '\x3', '\x39', '\x3', '\x4', '\x6', '\x4', 
		'=', '\n', '\x4', '\r', '\x4', '\xE', '\x4', '>', '\x3', '\x5', '\x6', 
		'\x5', '\x42', '\n', '\x5', '\r', '\x5', '\xE', '\x5', '\x43', '\x3', 
		'\x6', '\x6', '\x6', 'G', '\n', '\x6', '\r', '\x6', '\xE', '\x6', 'H', 
		'\x3', '\a', '\x6', '\a', 'L', '\n', '\a', '\r', '\a', '\xE', '\a', 'M', 
		'\x3', '\b', '\x6', '\b', 'Q', '\n', '\b', '\r', '\b', '\xE', '\b', 'R', 
		'\x3', '\t', '\x6', '\t', 'V', '\n', '\t', '\r', '\t', '\xE', '\t', 'W', 
		'\x3', '\n', '\x3', '\n', '\x3', '\v', '\x3', '\v', '\x3', '\f', '\x3', 
		'\f', '\x3', '\f', '\x5', '\f', '\x61', '\n', '\f', '\x3', '\r', '\x3', 
		'\r', '\x3', '\xE', '\x5', '\xE', '\x66', '\n', '\xE', '\x3', '\xF', '\x3', 
		'\xF', '\x2', '\x2', '\x10', '\x3', '\x3', '\x5', '\x4', '\a', '\x5', 
		'\t', '\x6', '\v', '\a', '\r', '\b', '\xF', '\t', '\x11', '\n', '\x13', 
		'\x2', '\x15', '\x2', '\x17', '\x2', '\x19', '\x2', '\x1B', '\x2', '\x1D', 
		'\x2', '\x3', '\x2', '\a', '\b', '\x2', '\v', '\f', '\xE', '\xF', '\"', 
		'\x41', '\x43', '\x80', '\xC2', '\x1F9', '\x2012', '\x2017', '\x5', '\x2', 
		'\v', '\v', '\xE', '\xE', '\"', '\"', '\x4', '\x2', '\f', '\f', '\xF', 
		'\xF', '\x5', '\x2', '\x43', '\\', '\x63', '|', '\xC2', '\x1F9', '\a', 
		'\x2', '#', '\x31', '<', '\x41', ']', '\x62', '}', '\x80', '\x2012', '\x2017', 
		'\x2', 'q', '\x2', '\x3', '\x3', '\x2', '\x2', '\x2', '\x2', '\x5', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\a', '\x3', '\x2', '\x2', '\x2', '\x2', '\t', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\v', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\r', '\x3', '\x2', '\x2', '\x2', '\x2', '\xF', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x11', '\x3', '\x2', '\x2', '\x2', '\x3', '#', '\x3', '\x2', '\x2', 
		'\x2', '\x5', '\x31', '\x3', '\x2', '\x2', '\x2', '\a', '<', '\x3', '\x2', 
		'\x2', '\x2', '\t', '\x41', '\x3', '\x2', '\x2', '\x2', '\v', '\x46', 
		'\x3', '\x2', '\x2', '\x2', '\r', 'K', '\x3', '\x2', '\x2', '\x2', '\xF', 
		'P', '\x3', '\x2', '\x2', '\x2', '\x11', 'U', '\x3', '\x2', '\x2', '\x2', 
		'\x13', 'Y', '\x3', '\x2', '\x2', '\x2', '\x15', '[', '\x3', '\x2', '\x2', 
		'\x2', '\x17', '`', '\x3', '\x2', '\x2', '\x2', '\x19', '\x62', '\x3', 
		'\x2', '\x2', '\x2', '\x1B', '\x65', '\x3', '\x2', '\x2', '\x2', '\x1D', 
		'g', '\x3', '\x2', '\x2', '\x2', '\x1F', '\"', '\x5', '\v', '\x6', '\x2', 
		' ', '\"', '\x5', '\r', '\a', '\x2', '!', '\x1F', '\x3', '\x2', '\x2', 
		'\x2', '!', ' ', '\x3', '\x2', '\x2', '\x2', '\"', '%', '\x3', '\x2', 
		'\x2', '\x2', '#', '!', '\x3', '\x2', '\x2', '\x2', '#', '$', '\x3', '\x2', 
		'\x2', '\x2', '$', '&', '\x3', '\x2', '\x2', '\x2', '%', '#', '\x3', '\x2', 
		'\x2', '\x2', '&', '*', '\x5', '\x13', '\n', '\x2', '\'', '+', '\x5', 
		'\r', '\a', '\x2', '(', '+', '\x5', '\v', '\x6', '\x2', ')', '+', '\x5', 
		'\x11', '\t', '\x2', '*', '\'', '\x3', '\x2', '\x2', '\x2', '*', '(', 
		'\x3', '\x2', '\x2', '\x2', '*', ')', '\x3', '\x2', '\x2', '\x2', '+', 
		',', '\x3', '\x2', '\x2', '\x2', ',', '*', '\x3', '\x2', '\x2', '\x2', 
		',', '-', '\x3', '\x2', '\x2', '\x2', '-', '\x4', '\x3', '\x2', '\x2', 
		'\x2', '.', '\x30', '\x5', '\v', '\x6', '\x2', '/', '.', '\x3', '\x2', 
		'\x2', '\x2', '\x30', '\x33', '\x3', '\x2', '\x2', '\x2', '\x31', '/', 
		'\x3', '\x2', '\x2', '\x2', '\x31', '\x32', '\x3', '\x2', '\x2', '\x2', 
		'\x32', '\x34', '\x3', '\x2', '\x2', '\x2', '\x33', '\x31', '\x3', '\x2', 
		'\x2', '\x2', '\x34', '\x37', '\x5', '\x19', '\r', '\x2', '\x35', '\x38', 
		'\x5', '\r', '\a', '\x2', '\x36', '\x38', '\x5', '\v', '\x6', '\x2', '\x37', 
		'\x35', '\x3', '\x2', '\x2', '\x2', '\x37', '\x36', '\x3', '\x2', '\x2', 
		'\x2', '\x38', '\x39', '\x3', '\x2', '\x2', '\x2', '\x39', '\x37', '\x3', 
		'\x2', '\x2', '\x2', '\x39', ':', '\x3', '\x2', '\x2', '\x2', ':', '\x6', 
		'\x3', '\x2', '\x2', '\x2', ';', '=', '\x5', '\x17', '\f', '\x2', '<', 
		';', '\x3', '\x2', '\x2', '\x2', '=', '>', '\x3', '\x2', '\x2', '\x2', 
		'>', '<', '\x3', '\x2', '\x2', '\x2', '>', '?', '\x3', '\x2', '\x2', '\x2', 
		'?', '\b', '\x3', '\x2', '\x2', '\x2', '@', '\x42', '\x5', '\x15', '\v', 
		'\x2', '\x41', '@', '\x3', '\x2', '\x2', '\x2', '\x42', '\x43', '\x3', 
		'\x2', '\x2', '\x2', '\x43', '\x41', '\x3', '\x2', '\x2', '\x2', '\x43', 
		'\x44', '\x3', '\x2', '\x2', '\x2', '\x44', '\n', '\x3', '\x2', '\x2', 
		'\x2', '\x45', 'G', '\x5', '\x1D', '\xF', '\x2', '\x46', '\x45', '\x3', 
		'\x2', '\x2', '\x2', 'G', 'H', '\x3', '\x2', '\x2', '\x2', 'H', '\x46', 
		'\x3', '\x2', '\x2', '\x2', 'H', 'I', '\x3', '\x2', '\x2', '\x2', 'I', 
		'\f', '\x3', '\x2', '\x2', '\x2', 'J', 'L', '\x5', '\x19', '\r', '\x2', 
		'K', 'J', '\x3', '\x2', '\x2', '\x2', 'L', 'M', '\x3', '\x2', '\x2', '\x2', 
		'M', 'K', '\x3', '\x2', '\x2', '\x2', 'M', 'N', '\x3', '\x2', '\x2', '\x2', 
		'N', '\xE', '\x3', '\x2', '\x2', '\x2', 'O', 'Q', '\x5', '\x1B', '\xE', 
		'\x2', 'P', 'O', '\x3', '\x2', '\x2', '\x2', 'Q', 'R', '\x3', '\x2', '\x2', 
		'\x2', 'R', 'P', '\x3', '\x2', '\x2', '\x2', 'R', 'S', '\x3', '\x2', '\x2', 
		'\x2', 'S', '\x10', '\x3', '\x2', '\x2', '\x2', 'T', 'V', '\x5', '\x13', 
		'\n', '\x2', 'U', 'T', '\x3', '\x2', '\x2', '\x2', 'V', 'W', '\x3', '\x2', 
		'\x2', '\x2', 'W', 'U', '\x3', '\x2', '\x2', '\x2', 'W', 'X', '\x3', '\x2', 
		'\x2', '\x2', 'X', '\x12', '\x3', '\x2', '\x2', '\x2', 'Y', 'Z', '\n', 
		'\x2', '\x2', '\x2', 'Z', '\x14', '\x3', '\x2', '\x2', '\x2', '[', '\\', 
		'\t', '\x3', '\x2', '\x2', '\\', '\x16', '\x3', '\x2', '\x2', '\x2', ']', 
		'^', '\a', '\xF', '\x2', '\x2', '^', '\x61', '\a', '\f', '\x2', '\x2', 
		'_', '\x61', '\t', '\x4', '\x2', '\x2', '`', ']', '\x3', '\x2', '\x2', 
		'\x2', '`', '_', '\x3', '\x2', '\x2', '\x2', '\x61', '\x18', '\x3', '\x2', 
		'\x2', '\x2', '\x62', '\x63', '\t', '\x5', '\x2', '\x2', '\x63', '\x1A', 
		'\x3', '\x2', '\x2', '\x2', '\x64', '\x66', '\t', '\x6', '\x2', '\x2', 
		'\x65', '\x64', '\x3', '\x2', '\x2', '\x2', '\x66', '\x1C', '\x3', '\x2', 
		'\x2', '\x2', 'g', 'h', '\x4', '\x32', ';', '\x2', 'h', '\x1E', '\x3', 
		'\x2', '\x2', '\x2', '\x12', '\x2', '!', '#', '*', ',', '\x31', '\x37', 
		'\x39', '>', '\x43', 'H', 'M', 'R', 'W', '`', '\x65', '\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
