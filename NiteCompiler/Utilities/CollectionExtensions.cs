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
}