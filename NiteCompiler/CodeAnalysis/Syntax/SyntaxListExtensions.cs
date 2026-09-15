using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal static class SyntaxListExtensions
{
	extension<T>(ArrayBuilder<T> array)
		where T : SyntaxNode
	{
		public SyntaxList<T> ToSyntaxList()
		{
			return new(array.ToImmutable());
		}

		public SyntaxList<T> ToSyntaxListAndClear()
		{
			return new(array.ToImmutableAndClear());
		}

		public SyntaxList<T> ToSyntaxListAndFree()
		{
			return new(array.ToImmutableAndFree());
		}
	}

	extension<T>(ArrayBuilder<SyntaxNode> array)
		where T : SyntaxNode
	{
		public SeparatedSyntaxList<T> ToSeparatedSyntaxList()
		{
			return new(array.ToImmutable());
		}

		public SeparatedSyntaxList<T> ToSeparatedSyntaxListAndClear()
		{
			return new(array.ToImmutableAndClear());
		}

		public SeparatedSyntaxList<T> ToSeparatedSyntaxListAndFree()
		{
			return new(array.ToImmutableAndFree());
		}
	}
}