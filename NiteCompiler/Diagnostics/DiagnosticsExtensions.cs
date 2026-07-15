using System.Collections.Immutable;

namespace NiteCompiler.Diagnostics;

public static class DiagnosticsExtensions
{
	extension(IEnumerable<Diagnostic> diagnostics)
	{
		public bool HasAnyError => throw new NotImplementedException();
	}

	extension(IDiagnosticBag bag)
	{
		public void AddRange(ImmutableArray<Diagnostic> diagnostics)
		{
			foreach (var diagnostic in diagnostics)
			{
				bag.Add(diagnostic);
			}
		}

		public void AddRange(ReadOnlySpan<Diagnostic> diagnostics)
		{
			foreach (var diagnostic in diagnostics)
			{
				bag.Add(diagnostic);
			}
		}

		public void AddRange(IEnumerable<Diagnostic> diagnostics)
		{
			foreach (var diagnostic in diagnostics)
			{
				bag.Add(diagnostic);
			}
		}
	}
}