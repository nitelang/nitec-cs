namespace NiteCompiler.CodeAnalysis.Syntax;

internal readonly ref struct ResetPoint(int position)
{
	public readonly int Position = position;
}