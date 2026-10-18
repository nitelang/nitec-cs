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
		_nodes = [.. nodes];
	}

	public int Count => _nodes.Length;

	public bool IsEmpty => Count == 0;

	public T this[int index] => _nodes[index];

	public static SyntaxList<T> CastUp<TDerived>(SyntaxList<TDerived> items)
		where TDerived : T
	{
		return new SyntaxList<T>(items._nodes.ToArray<T>());
	}

	public static SyntaxList<T> Empty { get; } = new([]);

	public ImmutableArray<T>.Enumerator GetEnumerator() => _nodes.GetEnumerator();

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_nodes).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)_nodes).GetEnumerator();
}