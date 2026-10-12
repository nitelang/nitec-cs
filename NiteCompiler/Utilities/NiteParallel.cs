namespace NiteCompiler.Utilities;

public static class NiteParallel
{
	public static void For(int fromInclusive, int toExclusive, Action<int> body, CancellationToken cancellationToken = default)
	{
		ParallelOptions options = new()
		{
			CancellationToken = cancellationToken,
		};

		Parallel.For(fromInclusive, toExclusive, options, body);
	}
}