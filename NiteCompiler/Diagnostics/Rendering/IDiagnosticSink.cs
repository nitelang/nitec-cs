namespace NiteCompiler.Diagnostics.Rendering;

public interface IDiagnosticSink
{
	void Print(string text);
	void Flush();
}