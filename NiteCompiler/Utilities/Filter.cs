namespace NiteCompiler.Utilities;

public static class Filter
{
	public static bool Passes<T>(Predicate<T>? filter, T value)
	{
		return filter == null || filter(value);
	}
}