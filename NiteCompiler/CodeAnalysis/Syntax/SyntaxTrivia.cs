using System.Diagnostics;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

public readonly struct SyntaxTrivia
{
	public SyntaxTree SyntaxTree { get; }
	public SyntaxKind Kind { get; }
	public TextSpan Span { get; }

	internal SyntaxTrivia(SyntaxKind kind, int position, int width, SyntaxTree syntaxTree)
	{
		Debug.Assert(kind.IsTrivia);
		Debug.Assert(position >= 0);
		Debug.Assert(width >= 0);
		SyntaxTree = syntaxTree;
		Kind = kind;
		Span = new TextSpan(position, width);
	}
}