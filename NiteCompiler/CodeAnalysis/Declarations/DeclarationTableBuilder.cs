using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Declarations;

internal sealed class DeclarationTableBuilder
{
	// TODO: not wired up yet — SyntaxAndDeclarationManager constructs DeclarationTable directly from the trees.
	// port Roslyn's AddRootDeclaration/RemoveRootDeclaration + RealizeAdds/RealizeRemoves (older/latest split and cache reuse) when incremental edits land.
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

	public void AddRootDeclaration(Lazy<RootModuleDeclaration> rootDeclaration)
	{
		RealizeRemoves();

		throw new NotImplementedException();
	}

	public void RemoveRootDeclaration(Lazy<RootModuleDeclaration> rootDeclaration)
	{
		RealizeAdds();

		throw new NotImplementedException();
	}

	private void RealizeRemoves()
	{
		throw new NotImplementedException();
	}

	private void RealizeAdds()
	{
		throw new NotImplementedException();
	}

	public static DeclarationTableBuilder GetInstance(DeclarationTable table)
	{
		var builder = Pool.Allocate();

		builder._table = table;

		return builder;
	}

	public DeclarationTable ToDeclarationTableAndFree()
	{
		DeclarationTable table = _table;

		_table = DeclarationTable.Empty;
		Pool.Free(this);

		return table;
	}
}