using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

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

	private bool _declarationDiagnosticsFrozen = false;
	internal DiagnosticBag DeclarationDiagnostics
	{
		get
		{
			Debug.Assert(!_declarationDiagnosticsFrozen);
			if (field == null)
			{
				var diagnostics = new DiagnosticBag();
				Interlocked.CompareExchange(ref field, diagnostics, null);
			}

			return field;
		}
	}

	public ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = default)
	{
		return GetDiagnostics(CompilationStage.Parse, false, symbolFilter: null, cancellationToken: cancellationToken);
	}

	public ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default)
	{
		return GetDiagnostics(CompilationStage.Declare, false, symbolFilter: null, cancellationToken);
	}

	public ImmutableArray<Diagnostic> GetFunctionBodyDiagnostics(CancellationToken cancellationToken = default)
	{
		return GetDiagnostics(CompilationStage.Compile, false, symbolFilter: null, cancellationToken);
	}

	public ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
	{
		return GetDiagnostics(CompilationStage.Compile, true, symbolFilter: null, cancellationToken);
	}

	private ImmutableArray<Diagnostic> GetDiagnostics(CompilationStage stage, bool includeEarlierStages, Predicate<object>? symbolFilter, CancellationToken cancellationToken)
	{
		var diagnostics = PooledDiagnosticBag.GetInstance();
		GetDiagnostics(stage, includeEarlierStages, diagnostics, symbolFilter, cancellationToken);
		return diagnostics.ToImmutableAndFree();
	}

	private void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, PooledDiagnosticBag diagnostics, Predicate<object>? symbolFilter, CancellationToken cancellationToken = default)
	{
		PooledDiagnosticBag builder = PooledDiagnosticBag.GetInstance();
		Debug.Assert(builder.Diagnostics is not null);

		GetDiagnosticsWithoutSeverityFiltering(stage, includeEarlierStages, builder, symbolFilter, cancellationToken);

		// Before returning diagnostics, we filter warnings to respect compiler flags
		FilterAndAppendDiagnostics(diagnostics.Diagnostics, builder.Diagnostics, cancellationToken);
		builder.Free();
	}

	private void GetDiagnosticsWithoutSeverityFiltering(CompilationStage stage, bool includeEarlierStages,
		PooledDiagnosticBag builder, Predicate<object>? symbolFilter, CancellationToken cancellationToken = default)
	{
		if (stage.IsInclude(CompilationStage.Parse, includeEarlierStages))
		{
			var syntaxTrees = SyntaxTrees;
			if (_options.ConcurrentBuild)
			{
				NiteParallel.For(0, syntaxTrees.Length, i =>
				{
					SyntaxTree tree = syntaxTrees[i];
					builder.AddRange(tree.GetDiagnostics());
				}, cancellationToken);
			}
			else
			{
				foreach (SyntaxTree tree in syntaxTrees)
				{
					builder.AddRange(tree.GetDiagnostics(cancellationToken));
				}
			}
		}

		// TODO: decl and compile

		cancellationToken.ThrowIfCancellationRequested();

		if (stage.IsInclude(CompilationStage.Compile, includeEarlierStages))
		{
		}
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