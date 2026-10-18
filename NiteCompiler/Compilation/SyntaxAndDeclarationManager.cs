using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Declarations;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Utilities;

namespace NiteCompiler.Compilation;

internal class SyntaxAndDeclarationManager
{
	public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

	internal SyntaxAndDeclarationManager(ImmutableArray<SyntaxTree> syntaxTrees, State? state)
	{
		SyntaxTrees = syntaxTrees;
		CurrentState = state;
	}

	internal State CurrentState
	{
		get
		{
			if (field == null)
			{
				Interlocked.CompareExchange(ref field, CreateState(SyntaxTrees), null);
			}

			return field;
		}
	}

	private static State CreateState(ImmutableArray<SyntaxTree> syntaxTrees)
	{
		ArrayBuilder<SyntaxTree> treesBuilder = ArrayBuilder<SyntaxTree>.GetInstance();
		PooledDictionary<SyntaxTree, int> ordinalMapBuilder = PooledDictionary<SyntaxTree, int>.GetInstance();
		PooledDictionary<SyntaxTree, Lazy<RootModuleDeclaration>> declMapBuilder = PooledDictionary<SyntaxTree, Lazy<RootModuleDeclaration>>.GetInstance();
		DeclarationTableBuilder declTableBuilder = DeclarationTable.Empty.ToBuilder();

		foreach (SyntaxTree syntaxTree in syntaxTrees)
		{
			AppendAllSyntaxTrees(treesBuilder, syntaxTree, ordinalMapBuilder, declMapBuilder, declTableBuilder);
		}

		ImmutableArray<SyntaxTree> trees = treesBuilder.ToImmutableAndFree();

		// TODO: Roslyn parity — feed DeclarationTableBuilder with Lazy<RootModuleDeclaration> roots and
		//       port its older/latest split + Cache (merged root, type/module names). For now the table
		//       builds the merged root straight from the trees, with no incremental caching.
		return new State(
			trees,
			ordinalMapBuilder.ToImmutableDictionaryAndFree(),
			declTableBuilder.ToDeclarationTableAndFree(),
			declMapBuilder.ToImmutableDictionaryAndFree());
	}

	private static void AppendAllSyntaxTrees(ArrayBuilder<SyntaxTree> treesBuilder, SyntaxTree tree,
		PooledDictionary<SyntaxTree, int> ordinalMapBuilder,
		PooledDictionary<SyntaxTree, Lazy<RootModuleDeclaration>> declMapBuilder,
		DeclarationTableBuilder declTableBuilder)
	{
		treesBuilder.Add(tree);
		ordinalMapBuilder.Add(tree, ordinalMapBuilder.Count);

		Lazy<RootModuleDeclaration> root = new(() => DeclarationTreeBuilder.ForTree(tree));
		declMapBuilder.Add(tree, root);
		declTableBuilder.AddRootDeclaration(root);
	}

	internal SyntaxAndDeclarationManager AddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		// TODO: incremental state — reuse OrdinalMap and apply Roslyn's DeclarationTable.Builder
		//       (older/latest + Cache) instead of rebuilding the state from scratch.
		ImmutableArray<SyntaxTree> newTrees = SyntaxTrees.AddRange(trees);
		return new SyntaxAndDeclarationManager(newTrees, state: null);
	}

	internal sealed class State
	{
		public readonly ImmutableArray<SyntaxTree> SyntaxTrees;
		public readonly ImmutableDictionary<SyntaxTree, int> OrdinalMap;
		public readonly DeclarationTable DeclarationTable;
		public readonly ImmutableDictionary<SyntaxTree, Lazy<RootModuleDeclaration>> RootModules;

		public State(ImmutableArray<SyntaxTree> syntaxTrees, ImmutableDictionary<SyntaxTree, int> ordinalMap,
			DeclarationTable declarationTable, ImmutableDictionary<SyntaxTree, Lazy<RootModuleDeclaration>> rootModules)
		{
			Debug.Assert(syntaxTrees.All(tree => syntaxTrees[ordinalMap[tree]] == tree));

			SyntaxTrees = syntaxTrees;
			OrdinalMap = ordinalMap;
			DeclarationTable = declarationTable;
			RootModules = rootModules;
		}
	}
}