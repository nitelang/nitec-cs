using System.Diagnostics;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

public readonly struct SyntaxTrivia
{
	public SyntaxTree SyntaxTree { get; }
	public SyntaxKind Kind { get; }
	public TextSpan Span { get; }

	internal SyntaxTrivia(SyntaxKind kind, TextSpan span, SyntaxTree syntaxTree)
	{
		Debug.Assert(kind.IsTrivia);
		SyntaxTree = syntaxTree;
		Kind = kind;
		Span = span;
	}
}