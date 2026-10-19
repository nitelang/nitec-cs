using System.Collections.Immutable;
using NiteCompiler.Utilities;

namespace NiteCompiler.Symbols;

public abstract class ContainerSymbol : Symbol
{
	private protected ContainerSymbol() { }

	/// <summary>
	/// Returns array filled with symbol's members.
	/// </summary>
	/// <remarks>
	/// Same functionality as <see cref="GetMembers()"/>, but resulting array may not be sorted.
	/// </remarks>
	/// <returns>
	/// Immutable array of symbol's members.
	/// </returns>
	public abstract ImmutableArray<Symbol> GetMembersUnordered();

	/// <summary>
	/// Returns ordered array filled with symbol's members.
	/// </summary>
	/// <returns>
	/// Immutable array of ordered symbol's members.
	/// </returns>
	public abstract ImmutableArray<Symbol> GetMembers();

	/// <summary>
	/// Returns ordered array filled with symbol's members with provided <paramref name="name"/>.
	/// </summary>
	/// <returns>
	/// Immutable array of ordered symbol's members with provided <paramref name="name"/>.
	/// </returns>
	public abstract ImmutableArray<Symbol> GetMembers(ReadOnlyMemory<char> name);

	/// <inheritdoc cref="GetMembers(ReadOnlyMemory{char})" />
	public ImmutableArray<Symbol> GetMembers(string name) => GetMembers(name.AsMemory());

	public abstract ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered();

	public abstract ImmutableArray<NamedTypeSymbol> GetTypeMembers();

	public abstract ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int? arity);

	/// <inheritdoc cref="GetTypeMembers(ReadOnlyMemory{char}, int?)" />
	public ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int? arity) => GetTypeMembers(name.AsMemory(), arity);

	public virtual ImmutableArray<ModuleSymbol> GetNestedModules()
	{
		var members = GetMembers();

		if (members.IsDefaultOrEmpty)
		{
			return [];
		}

		var result = ArrayBuilder<ModuleSymbol>.GetInstance();

		foreach (var member in members)
		{
			if (member.Kind == SymbolKind.Module)
			{
				result.Add((ModuleSymbol)member);
			}
		}

		return result.ToImmutableAndFree();
	}

	public virtual ModuleSymbol? GetNestedModule(string name)
	{
		foreach (var member in GetMembers(name))
		{
			if (member.Kind == SymbolKind.Module)
			{
				return (ModuleSymbol)member;
			}
		}

		return null;
	}
}