namespace NiteCompiler.Diagnostics;

public sealed class DiagnosticDescriptor
{
	public string Id { get; }

	public DiagnosticDescriptor(string id)
	{
		Guard.NotEmptyNorNull(Id = id);
	}
}