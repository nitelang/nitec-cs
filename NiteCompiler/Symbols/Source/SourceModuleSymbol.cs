using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Declarations;
using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.Symbols.Source;

internal sealed class SourceModuleSymbol : ModuleSymbol
{
	public MergedModuleDeclaration MergedDeclaration { get; }
	public override SourceLibrarySymbol ContainingLibrary { get; }
	public override Symbol ContainingSymbol { get; }

	public override string Name => MergedDeclaration.Name;

	public SourceModuleSymbol(SourceLibrarySymbol containingLibrary, SourceModuleSymbol? containingModule,
		MergedModuleDeclaration mergedDeclaration, PooledDiagnosticBag diagnostics)
	{
		ContainingLibrary = containingLibrary;
		ContainingSymbol = (Symbol?)containingModule ?? containingLibrary;
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
				ImmutableInterlocked.InterlockedCompareExchange(ref field, MergedDeclaration.NameLocations, default);
			}

			return field;
		}
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>? _nameToMembersMap;
	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>>? _nameToTypeMembersMap;
	private volatile bool _membersIsOrdered;
	private ImmutableArray<Symbol> _members;
	private ImmutableArray<NamedTypeSymbol> _typeMembersUnordered;

	/// <summary>
	/// Member symbols are created exactly once, here; every other query derives from this map so
	/// repeated lookups always see the same symbol instances.
	/// </summary>
	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> GetNameToMembersMap()
	{
		if (_nameToMembersMap == null)
		{
			var diagnostics = PooledDiagnosticBag.GetInstance();

			// NOTE: the following is not cancellable. Once we've set the members, we must make sure
			// we're in a consistent state.
			if (Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(diagnostics), null) == null)
			{
				AddDeclarationDiagnostics(diagnostics);
			}

			diagnostics.Free();
		}

		return _nameToMembersMap!;
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> MakeNameToMembersMap(PooledDiagnosticBag diagnostics)
	{
		ArrayBuilder<Symbol> members = ArrayBuilder<Symbol>.GetInstance();

		foreach (MergedItemDeclaration memberDeclaration in MergedDeclaration.Members)
		{
			switch (memberDeclaration)
			{
				case MergedModuleDeclaration moduleDeclaration:
					members.Add(new SourceModuleSymbol(ContainingLibrary, this, moduleDeclaration, diagnostics));
					break;
				case MergedTypeDeclaration typeDeclaration:
					members.Add(new SourceNamedTypeSymbol(this, typeDeclaration, diagnostics));
					break;
			}
		}

		// shortcut: duplicate-name path rebuilds the array per member; fine while the declaration
		// layer merges partial declarations so names are unique — switch to a builder-based
		// multi-value map when function overloads introduce real duplicates.
		Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> map = new(members.Count, ReadOnlyCharMemoryComparer.Instance);

		foreach (Symbol member in members.ToImmutableAndFree())
		{
			AddToMultiValueMap(map, member.Name, member);
		}

		return map;
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> GetNameToTypeMembersMap()
	{
		if (_nameToTypeMembersMap == null)
		{
			// Derived from the members map so type-member queries see the same instances.
			Interlocked.CompareExchange(ref _nameToTypeMembersMap, MakeNameToTypeMembersMap(GetNameToMembersMap()), null);
		}

		return _nameToTypeMembersMap!;
	}

	private static Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> MakeNameToTypeMembersMap(
		Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersMap)
	{
		Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> typeMap = new(membersMap.Count, ReadOnlyCharMemoryComparer.Instance);

		foreach (ImmutableArray<Symbol> members in membersMap.Values)
		{
			foreach (Symbol member in members)
			{
				if (member is NamedTypeSymbol type)
				{
					AddToMultiValueMap(typeMap, type.Name, type);
				}
			}
		}

		return typeMap;
	}

	private static void AddToMultiValueMap<T>(Dictionary<ReadOnlyMemory<char>, ImmutableArray<T>> map, string name, T symbol)
		where T : Symbol
	{
		ReadOnlyMemory<char> key = name.AsMemory();

		if (map.TryGetValue(key, out ImmutableArray<T> existing))
		{
			map[key] = existing.Add(symbol);
		}
		else
		{
			map[key] = [symbol];
		}
	}

	public override ImmutableArray<Symbol> GetMembersUnordered()
	{
		ImmutableArray<Symbol> result = _members;

		if (result.IsDefault)
		{
			ImmutableArray<Symbol> members = GetNameToMembersMap().Flatten();
			ImmutableInterlocked.InterlockedInitialize(ref _members, members);
			result = _members;
		}

		return result;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (_membersIsOrdered)
		{
			return _members;
		}

		ImmutableArray<Symbol> members = GetMembersUnordered();

		if (members.Length >= 2)
		{
			members = members.Sort(LexicalOrderSymbolComparer.Instance);
			ImmutableInterlocked.InterlockedExchange(ref _members, members);
		}
		_membersIsOrdered = true;

		return members;
	}

	public override ImmutableArray<Symbol> GetMembers(ReadOnlyMemory<char> name)
	{
		if (GetNameToMembersMap().TryGetValue(name, out ImmutableArray<Symbol> members))
		{
			return members;
		}

		return [];
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		if (_typeMembersUnordered.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> members = GetNameToTypeMembersMap().Flatten();
			ImmutableInterlocked.InterlockedInitialize(ref _typeMembersUnordered, members);
		}

		return _typeMembersUnordered;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return GetNameToTypeMembersMap().Flatten(LexicalOrderSymbolComparer.Instance);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int? arity)
	{
		if (GetNameToTypeMembersMap().TryGetValue(name, out ImmutableArray<NamedTypeSymbol> members))
		{
			ArrayBuilder<NamedTypeSymbol> result = ArrayBuilder<NamedTypeSymbol>.GetInstance();

			foreach (NamedTypeSymbol member in members)
			{
				if (arity == null || arity == member.Arity)
				{
					result.Add(member);
				}
			}

			return result.ToImmutableAndFree();
		}

		return [];
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