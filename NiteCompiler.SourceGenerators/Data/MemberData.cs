namespace NiteCompiler.SourceGenerators.Data;

internal enum NullSafety
{
	NotNull,
	Nullable,
}

internal sealed record MemberData
{
	public string Name { get; set; } = string.Empty;
	public object Type { get; set; } = string.Empty;
	public NullSafety NullSafety { get; set; } = NullSafety.NotNull;
}