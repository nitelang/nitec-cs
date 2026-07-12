namespace NiteCompiler.CodeAnalysis.Pooling;

internal static class ArrayBuilderExtensions
{
	extension<T>(ArrayBuilder<T> builder)
		where T : struct
	{
		public void AddIfNotNull(T? value)
		{
			if (value != null)
			{
				builder.Add(value.Value);
			}
		}
	}

	extension<T>(ArrayBuilder<T> builder)
		where T : class
	{
		public void AddIfNotNull(T? value)
		{
			if (value != null)
			{
				builder.Add(value);
			}
		}
	}
}