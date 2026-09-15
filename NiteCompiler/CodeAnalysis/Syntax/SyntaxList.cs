using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Syntax;

[DebuggerDisplay("Count = {Count}")]
public readonly struct SyntaxList<T> : IEnumerable<T> where T : SyntaxNode
{
	private readonly ImmutableArray<T> _nodes;

	public SyntaxList(ImmutableArray<T> nodes)
	{
		_nodes = nodes;
	}

	public SyntaxList(IEnumerable<T> nodes)
	{
		_nodes = nodes.ToImmutableArray();
	}

	public int Count => _nodes.Length;

	public T this[int index] => _nodes[index];

	public static SyntaxList<T> Empty { get; } = new([]);

	public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_nodes).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}