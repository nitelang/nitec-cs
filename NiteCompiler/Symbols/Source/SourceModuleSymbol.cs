using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Declarations;
using NiteCompiler.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.Symbols.Source;

internal sealed class SourceModuleSymbol : ModuleSymbol
{
	public MergedModuleDeclaration MergedDeclaration { get; }
	public override SourceLibrarySymbol ContainingLibrary { get; }
	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public SourceModuleSymbol(SourceLibrarySymbol containingLibrary, SourceModuleSymbol? containingModule,
		MergedModuleDeclaration mergedDeclaration, PooledDiagnosticBag diagnostics)
	{
		ContainingLibrary = containingLibrary;
		ContainingSymbol = (Symbol?)containingModule ?? containingLibrary;
		MergedDeclaration = mergedDeclaration;
		Name = mergedDeclaration.Name;

		foreach (var singleDeclaration in mergedDeclaration.Declarations)
		{
			diagnostics.AddRange(singleDeclaration.Diagnostics);
		}
	}

	public override ImmutableArray<Symbol> GetMembersUnordered()
	{
		throw new NotImplementedException();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		throw new NotImplementedException();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		throw new NotImplementedException();
	}

	public override ImmutableArray<TypeSymbol> GetTypeMembers()
	{
		throw new NotImplementedException();
	}

	public override ImmutableArray<TypeSymbol> GetTypeMembers(string name, int? arity)
	{
		throw new NotImplementedException();
	}

	private CompletionPart _state;
	internal override void ForceComplete(Predicate<Symbol>? filter, CancellationToken cancellationToken = default)
	{
		if (!filter.Passes(this)) return;

		throw new NotImplementedException();
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}
}