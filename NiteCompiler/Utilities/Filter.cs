namespace NiteCompiler.Utilities;

public static class FilterExtensions
{
	extension<T>(Predicate<T>? filter)
	{
		public bool Passes(T value)
		{
			return filter == null || filter(value);
		}
	}
}