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

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="CSharpCommentsGrammarParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.2")]
[System.CLSCompliant(false)]
public interface ICSharpCommentsGrammarListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.classLink"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassLink([NotNull] CSharpCommentsGrammarParser.ClassLinkContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.classLink"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassLink([NotNull] CSharpCommentsGrammarParser.ClassLinkContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParameter([NotNull] CSharpCommentsGrammarParser.ParameterContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParameter([NotNull] CSharpCommentsGrammarParser.ParameterContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.paramref"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParamref([NotNull] CSharpCommentsGrammarParser.ParamrefContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.paramref"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParamref([NotNull] CSharpCommentsGrammarParser.ParamrefContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.summary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSummary([NotNull] CSharpCommentsGrammarParser.SummaryContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.summary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSummary([NotNull] CSharpCommentsGrammarParser.SummaryContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.returnContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReturnContent([NotNull] CSharpCommentsGrammarParser.ReturnContentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.returnContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReturnContent([NotNull] CSharpCommentsGrammarParser.ReturnContentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.return"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReturn([NotNull] CSharpCommentsGrammarParser.ReturnContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.return"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReturn([NotNull] CSharpCommentsGrammarParser.ReturnContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.comment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComment([NotNull] CSharpCommentsGrammarParser.CommentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.comment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComment([NotNull] CSharpCommentsGrammarParser.CommentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.comments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComments([NotNull] CSharpCommentsGrammarParser.CommentsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.comments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComments([NotNull] CSharpCommentsGrammarParser.CommentsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.line_comment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLine_comment([NotNull] CSharpCommentsGrammarParser.Line_commentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.line_comment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLine_comment([NotNull] CSharpCommentsGrammarParser.Line_commentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.methodSignature"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodSignature([NotNull] CSharpCommentsGrammarParser.MethodSignatureContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.methodSignature"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodSignature([NotNull] CSharpCommentsGrammarParser.MethodSignatureContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.methodContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodContent([NotNull] CSharpCommentsGrammarParser.MethodContentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.methodContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodContent([NotNull] CSharpCommentsGrammarParser.MethodContentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.methodDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodDeclaration([NotNull] CSharpCommentsGrammarParser.MethodDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.methodDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodDeclaration([NotNull] CSharpCommentsGrammarParser.MethodDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.scope"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterScope([NotNull] CSharpCommentsGrammarParser.ScopeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.scope"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitScope([NotNull] CSharpCommentsGrammarParser.ScopeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.classContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassContent([NotNull] CSharpCommentsGrammarParser.ClassContentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.classContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassContent([NotNull] CSharpCommentsGrammarParser.ClassContentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.classDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassDefinition([NotNull] CSharpCommentsGrammarParser.ClassDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.classDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassDefinition([NotNull] CSharpCommentsGrammarParser.ClassDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.usingClause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterUsingClause([NotNull] CSharpCommentsGrammarParser.UsingClauseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.usingClause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitUsingClause([NotNull] CSharpCommentsGrammarParser.UsingClauseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.namespaceDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNamespaceDeclaration([NotNull] CSharpCommentsGrammarParser.NamespaceDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.namespaceDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNamespaceDeclaration([NotNull] CSharpCommentsGrammarParser.NamespaceDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="CSharpCommentsGrammarParser.start"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStart([NotNull] CSharpCommentsGrammarParser.StartContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="CSharpCommentsGrammarParser.start"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStart([NotNull] CSharpCommentsGrammarParser.StartContext context);
}
