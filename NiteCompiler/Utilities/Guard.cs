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
}