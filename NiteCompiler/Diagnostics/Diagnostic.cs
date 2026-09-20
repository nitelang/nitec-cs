namespace NiteCompiler.Diagnostics;

public sealed class Diagnostic
{
	public DiagnosticDescriptor Descriptor { get; }
	public string Message { get; }

	public Diagnostic(DiagnosticDescriptor descriptor, string message)
	{
		Descriptor = descriptor;
		Message = message;
	}
}