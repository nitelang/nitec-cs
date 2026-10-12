using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiteCompiler.Utilities;

internal sealed class PooledDictionary<K, V> : Dictionary<K, V>
	where K : notnull
{
	private readonly ObjectPool<PooledDictionary<K, V>> _pool;

	private PooledDictionary(ObjectPool<PooledDictionary<K, V>> pool, IEqualityComparer<K> keyComparer) : base(keyComparer)
	{
		_pool = pool;
	}

	public ImmutableDictionary<K, V> ToImmutableDictionaryAndFree()
	{
		ImmutableDictionary<K, V> result = this.ToImmutableDictionary(Comparer);
		Free();
		return result;
	}

	public ImmutableDictionary<K, V> ToImmutableDictionary() => this.ToImmutableDictionary(Comparer);

	public void Free()
	{
		Clear();
		_pool.Free(this);
	}

	// global pool
	private static readonly ObjectPool<PooledDictionary<K, V>> PoolInstance = CreatePool(EqualityComparer<K>.Default);

	public static ObjectPool<PooledDictionary<K, V>> CreatePool(IEqualityComparer<K> keyComparer)
	{
		ObjectPool<PooledDictionary<K, V>>? pool = null;
		pool = new ObjectPool<PooledDictionary<K, V>>(() => new PooledDictionary<K, V>(pool!, keyComparer), 128);
		return pool;
	}


	public static PooledDictionary<K, V> GetInstance(
#if DEBUG
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0
#endif
	)
	{
		var instance = PoolInstance.Allocate(
#if DEBUG
			filePath, lineNumber
#endif
		);
		Debug.Assert(instance.Count == 0);
		return instance;
	}
}