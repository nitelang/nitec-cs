using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

[DebuggerDisplay("Count = {Count}")]
public readonly struct SeparatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode
{
	private readonly ImmutableArray<SyntaxNode> _nodesAndSeparators;

	public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
	{
		_nodesAndSeparators = nodesAndSeparators;
	}

	public SeparatedSyntaxList(IEnumerable<SyntaxNode> nodesAndSeparators)
	{
		_nodesAndSeparators = [.. nodesAndSeparators];
	}

	public int Count => _nodesAndSeparators.Length == 0 ? 0 : (_nodesAndSeparators.Length + 1) / 2;

	public T this[int index] => (T)_nodesAndSeparators[index * 2];

	public SyntaxToken GetSeparator(int index) => (SyntaxToken)_nodesAndSeparators[index * 2 + 1];

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}