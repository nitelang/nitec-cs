using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler;

public sealed record CompilationOptions
{
	public ParserOptions AcquireParseOptions()
	{
		return new();
	}
}