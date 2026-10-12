using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Text;

namespace NiteCompiler.Diagnostics;

public sealed class SourceLocation : Location
{
	public override LocationKind Kind => LocationKind.SourceFile;

	private readonly SyntaxTree _syntaxTree;
	private readonly TextSpan _span;

	public SourceLocation(SyntaxTree syntaxTree, TextSpan span)
	{
		_syntaxTree = syntaxTree;
		_span = span;
	}

	public SourceLocation(SyntaxNode node) : this(node.SyntaxTree, node.Span)
	{
	}
}