namespace NiteCompiler.Utilities;

public static class CollectionExtensions
{
	extension<T>(Stack<T> stack)
	{
		public T? PopOrDefault()
		{
			if (stack.TryPop(out T? result))
			{
				return result;
			}

			return default;
		}

		public T? PeekOrDefault()
		{
			if (stack.TryPeek(out T? result))
			{
				return result;
			}

			return default;
		}
	}

	extension<K, V>(Dictionary<K, V> dictionary)
		where K : notnull
	{
		public V GetOrAdd(K key, V value)
		{
			if (dictionary.TryGetValue(key, out var existingValue))
			{
				return existingValue;
			}
			else
			{
				dictionary.Add(key, value);
				return value;
			}
		}

		public V GetOrAdd(K key, Func<V> getValue)
		{
			if (dictionary.TryGetValue(key, out var existingValue))
			{
				return existingValue;
			}
			else
			{
				var value = getValue();
				dictionary.Add(key, value);
				return value;
			}
		}
	}
}