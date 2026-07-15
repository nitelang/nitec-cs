using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Pooling;

namespace NiteCompiler.Diagnostics;

internal sealed class PooledDiagnosticBag : IDiagnosticBag
{
	private static readonly ObjectPool<PooledDiagnosticBag> Pool = new(static () => new PooledDiagnosticBag());

	private DiagnosticBag? _bag;

	public static PooledDiagnosticBag GetInstance()
	{
		return Pool.Allocate();
	}

	public void Free()
	{
		Pool.Free(this);

		_bag?.Clear();
	}

	public bool IsEmpty => _bag == null || _bag.IsEmpty;

	public DiagnosticBag Diagnostics
	{
		get
		{
			if (_bag == null)
			{
				Interlocked.CompareExchange(ref _bag, [], null);
			}

			return _bag;
		}
	}

	public void Add(Diagnostic diagnostic)
	{
		Diagnostics.Add(diagnostic);
	}

	/// <returns>A bag instance with at least one diagnostic, or <see langword="null"/>.</returns>
	public DiagnosticBag? ToBagAndFree()
	{
		var bag = _bag;
		_bag = null;
		Pool.Free(this);
		return bag;
	}

	public ImmutableArray<Diagnostic> ToImmutableAndFree()
	{
		var bag = _bag;
		_bag = null;
		Pool.Free(this);
		return bag?.ToImmutableArray() ?? [];
	}
}