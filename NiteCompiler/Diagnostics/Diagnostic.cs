namespace NiteCompiler.Diagnostics;

public sealed class Diagnostic
{
	public DiagnosticDescriptor Descriptor { get; }

	public Diagnostic(DiagnosticDescriptor descriptor)
	{
		Descriptor = descriptor;
	}
}