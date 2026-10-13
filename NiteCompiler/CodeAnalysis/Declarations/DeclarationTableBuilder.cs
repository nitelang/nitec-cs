using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Declarations;

internal sealed class DeclarationTableBuilder
{
	private static readonly ObjectPool<DeclarationTableBuilder> Pool = new(static () => new DeclarationTableBuilder());

	private DeclarationTable _table;

	private DeclarationTableBuilder()
	{
		_table = DeclarationTable.Empty;
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
		Pool.Free(this);
		return table;
	}
}