using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NiteCompiler.Utilities;

internal static class Guard
{
	public static void NotEmptyNorNull([NotNull] string? id, [CallerArgumentExpression(nameof(id))] string expression = null!)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentOutOfRangeException(expression);
		}
	}

	public static void IsValid<T>(T value, [CallerArgumentExpression(nameof(value))] string expression = null!)
		where T : struct, Enum
	{
		if (!value.IsValid())
		{
			throw new InvalidEnumArgumentException(expression, (int)(object)value, typeof(T));
		}
	}
}