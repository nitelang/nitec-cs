using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;

namespace NiteCompiler.Compilation;

internal struct LexicalSortKey
{
	public int TreeOrdinal { get; }
	public int Position { get; }

	public static readonly LexicalSortKey NotInSource = new(-1, 0);
	public static readonly LexicalSortKey NotInitialized = new(-1, -1);

	private LexicalSortKey(int treeOrdinal, int position)
	{
		TreeOrdinal = treeOrdinal;
		Position = position;
	}

	private LexicalSortKey(SyntaxTree? tree, int position, NiteCompilation compilation)
	{
		TreeOrdinal = tree != null ? compilation.GetSyntaxTreeOrdinal(tree) : -1;
		Position = position;
	}

	public LexicalSortKey(SyntaxNode node, NiteCompilation compilation)
		: this(compilation.GetSyntaxTreeOrdinal(node.SyntaxTree), node.Span.Start)
	{ }

	public LexicalSortKey(SyntaxToken token, NiteCompilation compilation)
		: this(compilation.GetSyntaxTreeOrdinal(token.SyntaxTree), token.Span.Start)
	{ }

	public LexicalSortKey(Location location, NiteCompilation compilation)
		: this(location.SourceTree!, location.SourceSpan.Start, compilation)
	{ }

	public static int Compare(LexicalSortKey xSortKey, LexicalSortKey ySortKey)
	{
		if (xSortKey.TreeOrdinal != ySortKey.TreeOrdinal)
		{
			if (xSortKey.TreeOrdinal < 0)
			{
				return 1;
			}

			if (ySortKey.TreeOrdinal < 0)
			{
				return -1;
			}

			int comparison = xSortKey.TreeOrdinal - ySortKey.TreeOrdinal;
			Debug.Assert(comparison != 0);
			return comparison;
		}

		return xSortKey.Position - ySortKey.Position;
	}

	public static LexicalSortKey First(LexicalSortKey xSortKey, LexicalSortKey ySortKey)
	{
		return Compare(xSortKey, ySortKey) > 0 ? ySortKey : xSortKey;
	}
}