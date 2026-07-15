namespace NiteCompiler.Diagnostics;

public interface IDiagnosticBag
{
	void Add(Diagnostic diagnostic);
}