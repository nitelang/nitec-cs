using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace NiteCompiler.Diagnostics;

[DebuggerStepThrough]
public class DiagnosticBag : IEnumerable<Diagnostic>, IDiagnosticBag
{
	private readonly ConcurrentBag<Diagnostic> _diagnostics; // most of the reports are thread-unsafe

	public DiagnosticBag()
	{
		_diagnostics = [];
	}

	public DiagnosticBag(IEnumerable<Diagnostic> diagnostics)
	{
		_diagnostics = new ConcurrentBag<Diagnostic>(diagnostics);
	}

	public bool IsEmpty => _diagnostics.IsEmpty;

	public void Add(Diagnostic diagnostic)
	{
		_diagnostics.Add(diagnostic);
	}

	public void Clear()
	{
		_diagnostics.Clear();
	}

	public IEnumerator<Diagnostic> GetEnumerator()
	{
		return _diagnostics.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}