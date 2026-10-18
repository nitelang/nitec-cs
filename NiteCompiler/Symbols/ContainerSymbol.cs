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
	public abstract ImmutableArray<Symbol> GetMembers(string name);

	public abstract ImmutableArray<TypeSymbol> GetTypeMembers();

	public abstract ImmutableArray<TypeSymbol> GetTypeMembers(string name, int? arity);

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