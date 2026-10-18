namespace NiteCompiler.Utilities;

internal sealed class ReadOnlyCharMemoryComparer : IEqualityComparer<ReadOnlyMemory<char>>
{
	public static readonly ReadOnlyCharMemoryComparer Instance = new();

	private ReadOnlyCharMemoryComparer() { }

	public static bool Equals(ReadOnlySpan<char> x, ReadOnlyMemory<char> y)
		=> x.SequenceEqual(y.Span);

	public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
		=> x.Span.SequenceEqual(y.Span);

	public int GetHashCode(ReadOnlyMemory<char> obj)
	{
		return string.GetHashCode(obj.Span);
	}
}