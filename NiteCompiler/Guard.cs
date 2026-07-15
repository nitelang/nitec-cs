using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NiteCompiler;

internal static class Guard
{
	public static void NotEmptyNorNull([NotNull] string? id, [CallerArgumentExpression(nameof(id))] string expression = null!)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentOutOfRangeException(expression);
		}
	}

	public static void ValueExists<T>(T value, [CallerArgumentExpression(nameof(value))] string expression = null!)
		where T : struct, Enum
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentException(null, expression);
		}
	}
}