using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Declarations;
using NiteCompiler.Diagnostics;

namespace NiteCompiler.Symbols.Source;

internal sealed class SourceNamedTypeSymbol : NamedTypeSymbol
{
	public override Symbol ContainingSymbol { get; }
	public MergedTypeDeclaration MergedDeclaration { get; }

	public override string Name => MergedDeclaration.Name;
	public override int LifetimeArity => MergedDeclaration.LifetimeArity;
	public override int Arity => MergedDeclaration.Arity;

	public SourceNamedTypeSymbol(SourceModuleSymbol containingModule, MergedTypeDeclaration mergedDeclaration,
		PooledDiagnosticBag diagnostics)
	{
		ContainingSymbol = containingModule;
		MergedDeclaration = mergedDeclaration;

		foreach (var singleDeclaration in mergedDeclaration.Declarations)
		{
			diagnostics.AddRange(singleDeclaration.Diagnostics);
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if (field.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref field, ImmutableArray<Location>.CastUp(MergedDeclaration.NameLocations), default);
			}

			return field;
		}
	}
}