using System.Diagnostics;
using NiteCompiler.Diagnostics;
using NiteCompiler.Symbols;

namespace NiteCompiler.Compilation;

internal sealed class LexicalOrderSymbolComparer : IComparer<Symbol>
{
	public static readonly LexicalOrderSymbolComparer Instance = new();

	private LexicalOrderSymbolComparer() { }

	public int Compare(Symbol? x, Symbol? y)
	{
		if (x == y)
		{
			return 0;
		}

		if (x == null) return -1;
		if (y == null) return 1;
		Debug.Assert(ReferenceEquals(x.DeclaringCompilation, y.DeclaringCompilation));

		var xSortKey = x.GetLexicalSortKey();
		var ySortKey = y.GetLexicalSortKey();
		int comparison = LexicalSortKey.Compare(xSortKey, ySortKey);
		if (comparison != 0)
		{
			return comparison;
		}

		comparison = x.Kind - y.Kind;
		if (comparison != 0)
		{
			return comparison;
		}

		comparison = string.CompareOrdinal(x.Name, y.Name);
		Debug.Assert(comparison != 0);
		return comparison;
	}
}