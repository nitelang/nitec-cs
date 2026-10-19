using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Text;

namespace NiteCompiler.Diagnostics;

public sealed class SourceLocation : Location
{
	public override LocationKind Kind => LocationKind.SourceFile;

	public override SyntaxTree SourceTree { get; }
	public override TextSpan SourceSpan { get; }

	public SourceLocation(SyntaxTree sourceTree, TextSpan sourceSpan)
	{
		SourceTree = sourceTree;
		SourceSpan = sourceSpan;
	}

	public SourceLocation(SyntaxNode node) : this(node.SyntaxTree, node.Span)
	{
	}
}