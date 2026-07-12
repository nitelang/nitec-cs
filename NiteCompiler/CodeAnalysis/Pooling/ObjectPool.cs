using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiteCompiler.CodeAnalysis.Pooling;

/// <summary>
/// Generic implementation of the object pooling pattern with a predefined pool size limit.
/// The main purpose is that a limited number of frequently used objects can be kept in the
/// pool for further recycling.
/// </summary>
/// <remarks>
/// <para>
/// It is not the goal to keep all returned objects. The pool is not meant for storage.
/// If there is no space in the pool, extra returned objects will be dropped.
/// </para>
/// <para>
/// It is implied that if an object was obtained from the pool, the caller will return it
/// back within a relatively short time. Keeping checked-out objects for long durations is
/// okay, but reduces the usefulness of pooling. Just create your own instance instead.
/// </para>
/// <para>
/// Not returning objects to the pool is not detrimental to the pool's operation, but is a
/// bad practice. Rationale: if there is no intent to reuse the object, do not use the pool —
/// just use <see langword="new"/>.
/// </para>
/// </remarks>
internal class ObjectPool<T> where T : class
{
	[DebuggerDisplay("{Value,nq}")]
	private struct Element
	{
		internal T? Value;
	}

	// Storage for the pool objects. The first item is stored in a dedicated field because we
	// expect to be able to satisfy most requests from it.
	private T? _firstItem;
	private readonly Element[] _items;

	// factory is stored for the lifetime of the pool. We will call this only when pool needs to
	// expand. compared to "new T()", Func gives more flexibility to implementers and faster
	// than "new T()".
	private readonly Func<T> _factory;

	public readonly bool TrimOnFree;

#if DEBUG
	/// <summary>
	/// When false, this pool's objects are not tracked for leak detection.
	/// Used for pools where cross-thread usage causes false positive leak reports.
	/// </summary>
	private readonly bool _trackLeaks;

	/// <summary>
	/// Identifies this pool in leak reports.
	/// </summary>
	private readonly string _poolName;
#endif

	internal ObjectPool(Func<T> factory, bool trimOnFree = true, bool trackLeaks = true
#if DEBUG
		, [CallerFilePath] string filePath = ""
		, [CallerLineNumber] int lineNumber = 0
#endif
	)
		: this(factory, Environment.ProcessorCount * 2, trimOnFree, trackLeaks
#if DEBUG
			, filePath, lineNumber
#endif
		)
	{
	}

	internal ObjectPool(Func<T> factory, int size, bool trimOnFree = true, bool trackLeaks = true
#if DEBUG
		, [CallerFilePath] string filePath = ""
		, [CallerLineNumber] int lineNumber = 0
#endif
	)
	{
		Debug.Assert(size >= 1);
		_factory = factory;
		_items = new Element[size - 1];
		TrimOnFree = trimOnFree;
#if DEBUG
		_trackLeaks = trackLeaks;
		_poolName = Path.GetFileName(filePath) + ":" + lineNumber;
#endif
	}

	internal ObjectPool(Func<ObjectPool<T>, T> factory, int size
#if DEBUG
		, [CallerFilePath] string filePath = ""
		, [CallerLineNumber] int lineNumber = 0
#endif
	)
	{
		Debug.Assert(size >= 1);
		_factory = () => factory(this);
		_items = new Element[size - 1];
#if DEBUG
		_trackLeaks = true;
		_poolName = Path.GetFileName(filePath) + ":" + lineNumber;
#endif
	}

	private T CreateInstance()
	{
		var inst = _factory();
		return inst;
	}

	/// <summary>
	/// Produces an instance.
	/// </summary>
	/// <remarks>
	/// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
	/// Note that Free will try to store recycled objects close to the start thus statistically 
	/// reducing how far we will typically search.
	/// </remarks>
	internal T Allocate(
#if DEBUG
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0
#endif
	)
	{
		// PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
		// Note that the initial read is optimistically not synchronized. That is intentional. 
		// We will interlock only when we have a candidate. in a worst case we may miss some
		// recently returned objects. Not a big deal.
		var inst = _firstItem;
		if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
		{
			inst = AllocateSlow();
		}

#if DEBUG
		if (_trackLeaks)
			PoolTracker.OnAllocate(inst, _poolName, filePath, lineNumber);
#endif
		return inst;
	}

	private T AllocateSlow()
	{
		var items = _items;

		for (int i = 0; i < items.Length; i++)
		{
			// Note that the initial read is optimistically not synchronized. That is intentional. 
			// We will interlock only when we have a candidate. in a worst case we may miss some
			// recently returned objects. Not a big deal.
			var inst = items[i].Value;
			if (inst != null)
			{
				if (inst == Interlocked.CompareExchange(ref items[i].Value, null, inst))
				{
					return inst;
				}
			}
		}

		return CreateInstance();
	}

	/// <summary>
	/// Returns objects to the pool.
	/// </summary>
	/// <remarks>
	/// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
	/// Note that Free will try to store recycled objects close to the start thus statistically 
	/// reducing how far we will typically search in Allocate.
	/// </remarks>
	internal void Free(T obj)
	{
		Validate(obj);
		ForgetTrackedObject(obj);

		if (_firstItem == null)
		{
			// Intentionally not using interlocked here. 
			// In a worst case scenario two objects may be stored into same slot.
			// It is very unlikely to happen and will only mean that one of the objects will get collected.
			_firstItem = obj;
		}
		else
		{
			FreeSlow(obj);
		}
	}

	private void FreeSlow(T obj)
	{
		var items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Value == null)
			{
				// Intentionally not using interlocked here. 
				// In a worst case scenario two objects may be stored into same slot.
				// It is very unlikely to happen and will only mean that one of the objects will get collected.
				items[i].Value = obj;
				break;
			}
		}
	}

	/// <summary>
	/// Removes an object from leak tracking.  
	/// 
	/// This is called when an object is returned to the pool.  It may also be explicitly 
	/// called if an object allocated from the pool is intentionally not being returned
	/// to the pool.  This can be of use with pooled arrays if the consumer wants to 
	/// return a larger array to the pool than was originally allocated.
	/// </summary>
	[Conditional("DEBUG")]
	internal void ForgetTrackedObject(T old, T? replacement = null)
	{
#if DEBUG
		if (!_trackLeaks)
			return;

		PoolTracker.OnFree(old);

		if (replacement != null)
		{
			PoolTracker.OnAllocate(replacement, _poolName);
		}
#endif
	}

	[Conditional("DEBUG")]
	private void Validate(object obj)
	{
		Debug.Assert(obj != null, "freeing null?");

		Debug.Assert(_firstItem != obj, "freeing twice?");

		var items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			var value = items[i].Value;
			if (value == null)
			{
				return;
			}

			Debug.Assert(value != obj, "freeing twice?");
		}
	}
}