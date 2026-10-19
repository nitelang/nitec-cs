using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.Compilation;
using NiteCompiler.Utilities;

namespace NiteCompiler.Symbols;

internal sealed class MergedModuleSymbol : ModuleSymbol
{
	public ImmutableArray<ModuleSymbol> Modules { get; }
	public override string Name => Modules[0].Name;
	public override Symbol? ContainingSymbol => Modules[0].ContainingSymbol as ModuleSymbol;

	public MergedModuleSymbol(ImmutableArray<ModuleSymbol> modules)
	{
		Debug.Assert(modules.Length > 0);

		Modules = modules;
	}

	// TODO: Cache members

	public override ImmutableArray<Symbol> GetMembersUnordered()
	{
		var builder = ArrayBuilder<Symbol>.GetInstance();

		foreach (ModuleSymbol module in Modules)
		{
			builder.AddRange(module.GetMembersUnordered());
		}

		return builder.ToImmutableAndFree();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		var builder = ArrayBuilder<Symbol>.GetInstance();

		foreach (ModuleSymbol module in Modules)
		{
			builder.AddRange(module.GetMembers());
		}

		builder.Sort(LexicalOrderSymbolComparer.Instance);

		return builder.ToImmutableAndFree();
	}

	public override ImmutableArray<Symbol> GetMembers(ReadOnlyMemory<char> name)
	{
		var builder = ArrayBuilder<Symbol>.GetInstance();

		foreach (ModuleSymbol module in Modules)
		{
			builder.AddRange(module.GetMembers(name));
		}

		builder.Sort(LexicalOrderSymbolComparer.Instance);

		return builder.ToImmutableAndFree();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		var builder = ArrayBuilder<NamedTypeSymbol>.GetInstance();

		foreach (ModuleSymbol module in Modules)
		{
			builder.AddRange(module.GetTypeMembers());
		}

		return builder.ToImmutableAndFree();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		var builder = ArrayBuilder<NamedTypeSymbol>.GetInstance();

		foreach (ModuleSymbol module in Modules)
		{
			builder.AddRange(module.GetTypeMembers());
		}

		builder.Sort(LexicalOrderSymbolComparer.Instance);

		return builder.ToImmutableAndFree();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int? arity)
	{
		var builder = ArrayBuilder<NamedTypeSymbol>.GetInstance();

		foreach (ModuleSymbol module in Modules)
		{
			builder.AddRange(module.GetTypeMembers(name, arity));
		}

		builder.Sort(LexicalOrderSymbolComparer.Instance);

		return builder.ToImmutableAndFree();
	}

	internal override void ForceComplete(Predicate<Symbol>? filter, CancellationToken cancellationToken = default)
	{
		foreach (ModuleSymbol module in Modules)
		{
			cancellationToken.ThrowIfCancellationRequested();
			module.ForceComplete(filter, cancellationToken);
		}
	}
}