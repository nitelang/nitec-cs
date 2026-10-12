using System.Collections.Immutable;
using System.Diagnostics;
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
		//DeclarationTable declTableBuilder = DeclarationTable.Empty.ToBuilder();


		foreach (SyntaxTree syntaxTree in syntaxTrees)
		{
			AppendAllSyntaxTrees(treesBuilder, syntaxTree, ordinalMapBuilder);
		}

		return new State(syntaxTrees, ordinalMapBuilder.ToImmutableDictionaryAndFree(), null!);
	}

	private static void AppendAllSyntaxTrees(ArrayBuilder<SyntaxTree> treesBuilder, SyntaxTree tree, PooledDictionary<SyntaxTree, int> ordinalMapBuilder)
	{
		treesBuilder.Add(tree);
		ordinalMapBuilder.Add(tree, ordinalMapBuilder.Count);
	}

	internal sealed class State
	{
		public readonly ImmutableArray<SyntaxTree> SyntaxTrees;
		public readonly ImmutableDictionary<SyntaxTree, int> OrdinalMap;
		public readonly object DeclarationTable;

		public State(ImmutableArray<SyntaxTree> syntaxTrees, ImmutableDictionary<SyntaxTree, int> ordinalMap,
			object declarationTable)
		{
			Debug.Assert(syntaxTrees.All(tree => syntaxTrees[ordinalMap[tree]] == tree));

			SyntaxTrees = syntaxTrees;
			OrdinalMap = ordinalMap;
			DeclarationTable = declarationTable;

			throw new NotImplementedException();
		}
	}
}