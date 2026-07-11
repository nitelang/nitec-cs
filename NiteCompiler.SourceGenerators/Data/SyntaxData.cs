namespace NiteCompiler.SourceGenerators.Data;

internal record SyntaxData
{
    public TokenData[] Tokens { get; set; } = [];
    public NodeData[] Nodes { get; set; } = [];
}