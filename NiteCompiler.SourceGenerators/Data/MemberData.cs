namespace NiteCompiler.SourceGenerators.Data;

internal sealed record MemberData
{
    public string Name { get; set; } = string.Empty;
    public object Type { get; set; } = string.Empty;
}
