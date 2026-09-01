// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiteCompiler.CodeAnalysis.Pooling;

// HashSet that can be recycled via an object pool
// NOTE: these HashSets always have the default comparer.
internal sealed class PooledHashSet<T> : HashSet<T>
{
	private readonly ObjectPool<PooledHashSet<T>> _pool;

	private PooledHashSet(ObjectPool<PooledHashSet<T>> pool, IEqualityComparer<T> equalityComparer) :
		base(equalityComparer)
	{
		_pool = pool;
	}

	public void Free()
	{
		this.Clear();
		_pool?.Free(this);
	}

	// global pool
	private static readonly ObjectPool<PooledHashSet<T>> PoolInstance = CreatePool(EqualityComparer<T>.Default);

	// if someone needs to create a pool;
	public static ObjectPool<PooledHashSet<T>> CreatePool(IEqualityComparer<T> equalityComparer)
	{
		ObjectPool<PooledHashSet<T>>? pool = null;
		pool = new ObjectPool<PooledHashSet<T>>(() => new PooledHashSet<T>(pool!, equalityComparer), 128);
		return pool;
	}

	public static PooledHashSet<T> GetInstance(
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