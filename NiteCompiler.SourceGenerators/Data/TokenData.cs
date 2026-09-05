namespace NiteCompiler.SourceGenerators.Data;

internal sealed record TokenData
{
	public string Name { get; set; } = string.Empty;
	public string? Text { get; set; }
	public bool Trivia { get; set; }
	public bool Contextual { get; set; }
	public Precedence? Precedence { get; set; }
}