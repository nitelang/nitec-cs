using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiteCompiler.Utilities;

internal static class ExceptionUtilities
{
	public static Exception UnexpectedValue(object? o)
	{
		string output = $"Unexpected value '{o}' of type '{(o != null ? o.GetType().FullName : "<unknown>")}'";
		Debug.Assert(false, output);

		return new InvalidOperationException(output);
	}

	public static Exception Unreachable([CallerFilePath] string? path = null, [CallerLineNumber] int line = 0)
	{
		return new InvalidOperationException($"This program location is thought to be unreachable. File='{path}' Line={line}");
	}
}