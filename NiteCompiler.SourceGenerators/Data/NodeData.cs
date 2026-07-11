namespace NiteCompiler.SourceGenerators.Data;

internal sealed record NodeData
{
    public string Name { get; set; } = string.Empty;
    public string? Base { get; set; }
    public bool Abstract { get; set; }
    public bool OnlyTokensKinds { get; set; }
    public TokenData[] Kinds { get; set; } = [];
    public MemberData[] Members { get; set; } = [];
}