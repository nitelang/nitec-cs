using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;
using NiteCompiler.Text;

namespace NiteCompiler.Compilation;

public partial class NiteCompilation
{
	private bool FilterAndAppendDiagnostics(DiagnosticBag accumulator, DiagnosticBag incoming, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	private bool FilterAndAppendDiagnostics(DiagnosticBag accumulator, IEnumerable<Diagnostic> incoming,
		HashSet<string>? exclude, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
		/*
		bool hasError = false;
		bool reportSuppressedDiagnostics = true;//Options.ReportSuppressedDiagnostics;

		foreach (Diagnostic diagnostic in incoming)
		{
			if (exclude?.Contains(diagnostic.Descriptor.Id) == true)
			{
				continue;
			}

			var filtered = diagnostic;//Options.FilterDiagnostic(diagnostic, cancellationToken);
			if (filtered == null || (!reportSuppressedDiagnostics && filtered.IsSuppressed))
			{
				continue;
			}
			else if (filtered.IsUnsuppressableError())
			{
				hasError = true;
			}

			accumulator.Add(filtered);
		}

		return !hasError;*/
	}

	private void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, DiagnosticBag diagnostics, Predicate<object>? symbolFilter, CancellationToken cancellationToken = default)
	{
		BindingDiagnosticBag builder = BindingDiagnosticBag.GetInstance();
		Debug.Assert(builder.Diagnostics is { });

		GetDiagnosticsWithoutSeverityFiltering(stage, includeEarlierStages, builder, symbolFilter, cancellationToken);

		// Before returning diagnostics, we filter warnings to respect compiler flags
		FilterAndAppendDiagnostics(diagnostics, builder.Diagnostics, cancellationToken);
		builder.Free();
	}

	private void GetDiagnosticsWithoutSeverityFiltering(CompilationStage stage, bool includeEarlierStages,
		BindingDiagnosticBag builder, Predicate<object>? symbolFilter, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	private ImmutableArray<Diagnostic> GetDiagnosticsForMethodBodiesInTree(SyntaxTree tree, TextSpan? span,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	private struct MethodBodyDiagnostics
	{
		public SyntaxTree Tree { get; }
		public TextSpan? Span { get; }
		public ImmutableArray<Diagnostic> Diagnostics { get; }

		public MethodBodyDiagnostics(SyntaxTree tree, TextSpan? span, ImmutableArray<Diagnostic> diagnostics)
		{
			Tree = tree;
			Span = span;
			Diagnostics = diagnostics;
		}
	}
}