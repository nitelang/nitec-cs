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
	private SlidingWindow Window { get; }

	public Lexer(SyntaxTree syntaxTree, SourceText source, NiteParseOptions options, DiagnosticBag diagnostics)
	{
		SyntaxTree = syntaxTree;
		Options = options;
		Diagnostics = diagnostics;
		Window = new SlidingWindow(source);
	}

	private ResetPoint GetResetPoint()
	{
		return new ResetPoint(Window.Position);
	}

	private void Reset(ResetPoint rp)
	{
		Window.Reset(rp.Position);
	}

	internal ref struct TokenInfo
	{
		public SyntaxKind Kind;
	}

	public SyntaxToken Lex()
	{
		TokenInfo info = default;

		ImmutableArray<SyntaxTrivia> leading, trailing;
		var trivia = ArrayBuilder<SyntaxTrivia>.GetInstance();

		ReadTrivia(true, trivia);
		leading = trivia.ToImmutableAndClear();

		Window.Start();
		ReadToken(ref info);
		TextSpan span = Window.LexemeSpan;

		ReadTrivia(false, trivia);
		trailing = trivia.ToImmutableAndFree();

		return new SyntaxToken(info.Kind, Window.LexemeSpan, leading, trailing, SyntaxTree);
	}

	private void ReadToken(ref TokenInfo info)
	{
		if (Window.IsAtTheEnd)
		{
			info.Kind = SyntaxKind.EndOfFile;
			return;
		}

		switch (Window.Current)
		{

			default:
				//ReadIdentifier(ref info);

				if (Window.Width == 0)
				{
					Window.Advance();
					info.Kind = SyntaxKind.BadToken;
				}

				break;
		}
	}
}