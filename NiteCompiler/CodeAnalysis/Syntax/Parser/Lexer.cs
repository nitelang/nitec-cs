using System.Collections.Immutable;
using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal sealed partial class Lexer
{
	public SyntaxTree SyntaxTree { get; }
	public NiteParseOptions Options { get; }
	public DiagnosticBag Diagnostics { get; }
	private SlidingWindow _window;

	public Lexer(SyntaxTree syntaxTree, SourceText source, NiteParseOptions options, DiagnosticBag diagnostics)
	{
		SyntaxTree = syntaxTree;
		Options = options;
		Diagnostics = diagnostics;
		_window = new SlidingWindow(source);
	}

	private ResetPoint GetResetPoint()
	{
		return new ResetPoint(_window.Position);
	}

	private void Reset(ResetPoint rp)
	{
		_window.Reset(rp.Position);
	}

	internal ref struct TokenInfo
	{
		public SyntaxKind Kind;
	}

	public SyntaxToken Lex()
	{
		TokenInfo info = default;

		var trivia = ArrayBuilder<SyntaxTrivia>.GetInstance();

		ReadTrivia(true, trivia);
		var leading = trivia.ToImmutableAndClear();

		_window.Start();
		ReadToken(ref info);
		TextSpan span = _window.LexemeSpan;

		ReadTrivia(false, trivia);
		var trailing = trivia.ToImmutableAndFree();

		return new SyntaxToken(info.Kind, _window.LexemeSpan, leading, trailing, SyntaxTree);
	}

	private void ReadToken(ref TokenInfo info)
	{
		if (_window.IsAtTheEnd)
		{
			info.Kind = SyntaxKind.EndOfFile;
			return;
		}

		switch (_window.Current)
		{

			default:
				//ReadIdentifier(ref info);

				if (_window.Width == 0)
				{
					_window.Advance();
					info.Kind = SyntaxKind.BadToken;
				}

				break;
		}
	}
}