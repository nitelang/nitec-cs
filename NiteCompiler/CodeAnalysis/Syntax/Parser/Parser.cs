using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

// This file contains only token processing, parsing is located in other files Parser.XXX.cs
/// <summary>
/// Nite language parser class.
/// </summary>
/// <remarks>
/// This class is <b>NOT</b> thread-safe.
/// </remarks>
internal sealed partial class Parser
{
	private readonly ImmutableArray<SyntaxToken> _tokens;
	private int _currentIndex = 0;
	private readonly int _maxIndex;

	public Parser(SyntaxTree tree, SourceText source, NiteParseOptions options, DiagnosticBag diagnostics)
	{
		Debug.Assert(tree != null);
		Debug.Assert(source != null);
		Debug.Assert(options != null);
		Debug.Assert(diagnostics != null);

		Lexer lexer = new(tree, source, options, diagnostics);
		var tokens = ArrayBuilder<SyntaxToken>.GetInstance();

		SyntaxToken token;
		do
		{
			token = lexer.Lex();
			tokens.Add(token);
		} while (token.Kind != SyntaxKind.EndOfFile);

		_tokens = tokens.ToImmutableAndFree();
		_maxIndex = _tokens.Length - 1;
	}

	private SyntaxToken Current
	{
		[DebuggerStepThrough]
		get => Peek(0);
	}

	[DebuggerStepThrough]
	private SyntaxToken Peek(int offset)
	{
		Debug.Assert(offset >= 0);
		return _tokens[int.Min(offset + _currentIndex, _maxIndex)];
	}

	[DebuggerStepThrough]
	private SyntaxToken PeekAndAdvance()
	{
		SyntaxToken current = Current;
		_currentIndex++;
		return current;
	}

	[DebuggerStepThrough]
	private void Advance()
	{
		_currentIndex++;
	}

	[DebuggerStepThrough]
	private SyntaxToken MatchToken(SyntaxKind kind)
	{
		Debug.Assert(kind.IsToken && kind.IsValid()); // We only have tokens here, requesting node kind is obviously wrong.

		if (Current.Kind == kind)
			return PeekAndAdvance();

		// _diagnostics.ReportExpectedToken(Current.Span.Contextualize(_syntaxTree), kind);
		return Current.WithKind(kind);
	}

	[DebuggerStepThrough]
	private bool IsPresentedAny(params ReadOnlySpan<SyntaxKind> kinds)
	{
		return kinds.Contains(Current.Kind);
	}

	private partial class Lexer; // in Lexer.cs
}