using System.Collections.Immutable;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Declarations;

internal sealed class DeclarationTableBuilder
{
	// TODO: wire SyntaxAndDeclarationManager.AddSyntaxTrees/RemoveSyntaxTrees through this builder
	// (remove old tree's Lazy<RootModuleDeclaration>, add the new one) instead of rebuilding the state from scratch.
	private static readonly ObjectPool<DeclarationTableBuilder> Pool = new(static () => new DeclarationTableBuilder());

	private DeclarationTable _table;
	private readonly List<Lazy<RootModuleDeclaration>> _addedLazyRootDeclarations;
	private readonly List<Lazy<RootModuleDeclaration>> _removedLazyRootDeclarations;

	private DeclarationTableBuilder()
	{
		_table = DeclarationTable.Empty;
		_addedLazyRootDeclarations = [];
		_removedLazyRootDeclarations = [];
	}

	public static DeclarationTableBuilder GetInstance(DeclarationTable table)
	{
		var builder = Pool.Allocate();

		builder._table = table;

		return builder;
	}

	public void AddRootDeclaration(Lazy<RootModuleDeclaration> rootDeclaration)
	{
		// We don't allow intermingled add/remove operations. Realize any pending removes first.
		RealizeRemoves();

		_addedLazyRootDeclarations.Add(rootDeclaration);
	}

	public void RemoveRootDeclaration(Lazy<RootModuleDeclaration> rootDeclaration)
	{
		// We don't allow intermingled add/remove operations. Realize any pending adds first.
		RealizeAdds();

		_removedLazyRootDeclarations.Add(rootDeclaration);
	}

	public DeclarationTable ToDeclarationTableAndFree()
	{
		RealizeAdds();
		RealizeRemoves();

		DeclarationTable table = _table;

		_table = DeclarationTable.Empty;
		Pool.Free(this);

		return table;
	}

	private void RealizeAdds()
	{
		if (_addedLazyRootDeclarations.Count == 0)
		{
			return;
		}

		var lastDeclaration = _addedLazyRootDeclarations[^1];
		if (_addedLazyRootDeclarations.Count == 1)
		{
			// We can only re-use the cache if we don't already have a 'latest' item for the decl table.
			if (_table.LatestLazyRootDeclaration == null)
			{
				_table = new DeclarationTable(_table.AllOlderRootDeclarations, lastDeclaration, _table.MergedCache);
			}
			else
			{
				// We already had a 'latest' item. This means we're hearing about a change to a
				// different tree. Add the old 'latest' to the 'older' collection and don't reuse the cache.
				_table = new DeclarationTable(_table.AllOlderRootDeclarations.Add(_table.LatestLazyRootDeclaration), lastDeclaration, cache: null);
			}
		}
		else
		{
			_addedLazyRootDeclarations.RemoveAt(_addedLazyRootDeclarations.Count - 1);

			if (_table.LatestLazyRootDeclaration != null)
			{
				_addedLazyRootDeclarations.Insert(0, _table.LatestLazyRootDeclaration);
			}

			var newOlderRootDeclarations = _table.AllOlderRootDeclarations.AddRange(_addedLazyRootDeclarations);

			_table = new DeclarationTable(newOlderRootDeclarations, lastDeclaration, cache: null);
		}

		_addedLazyRootDeclarations.Clear();
	}

	private void RealizeRemoves()
	{
		if (_removedLazyRootDeclarations.Count == 0)
		{
			return;
		}

		if (_removedLazyRootDeclarations.Count == 1)
		{
			var firstDeclaration = _removedLazyRootDeclarations[0];

			// We can only reuse the cache if we're removing the decl that was just added.
			if (_table.LatestLazyRootDeclaration == firstDeclaration)
			{
				_table = new DeclarationTable(_table.AllOlderRootDeclarations, latestLazyRootDeclaration: null, cache: _table.MergedCache);
			}
			else
			{
				// We're removing a different tree than the latest one added. We need to remove the
				// passed in root from our 'older' list. We also can't reuse the cache, but we can
				// keep around the 'latestLazyRootDeclaration'.
				_table = new DeclarationTable(_table.AllOlderRootDeclarations.Remove(firstDeclaration), _table.LatestLazyRootDeclaration, cache: null);
			}
		}
		else
		{
			bool isLatestRemoved = _table.LatestLazyRootDeclaration != null && _removedLazyRootDeclarations.Contains(_table.LatestLazyRootDeclaration);

			ImmutableArray<Lazy<RootModuleDeclaration>> newOlderRootDeclarations = _table.AllOlderRootDeclarations.RemoveRange(_removedLazyRootDeclarations);
			Lazy<RootModuleDeclaration>? newLatestLazyRootDeclaration = isLatestRemoved ? null : _table.LatestLazyRootDeclaration;

			_table = new DeclarationTable(newOlderRootDeclarations, newLatestLazyRootDeclaration, cache: null);
		}

		_removedLazyRootDeclarations.Clear();
	}
}