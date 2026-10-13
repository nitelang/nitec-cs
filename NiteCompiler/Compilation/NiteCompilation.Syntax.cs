using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Utilities;

namespace NiteCompiler.Compilation;

public partial class NiteCompilation
{
	/// <summary>
	/// The syntax trees (parsed from source code) that this compilation was created with.
	/// </summary>
	public ImmutableArray<SyntaxTree> SyntaxTrees => _syntaxAndDeclarations.CurrentState.SyntaxTrees;

	public NiteCompilation AddSyntaxTrees(params IEnumerable<SyntaxTree> trees)
	{
		if (trees == null)
		{
			throw new ArgumentNullException(nameof(trees));
		}

		List<SyntaxTree> treeList = [];
		PooledHashSet<SyntaxTree> externalSyntaxTrees = PooledHashSet<SyntaxTree>.GetInstance();
		int i = 0;
		try
		{
			foreach (SyntaxTree? tree in trees)
			{
				if (tree == null)
				{
					throw new ArgumentNullException($"{nameof(trees)}[{i}]");
				}

				if (!externalSyntaxTrees.Add(tree))
				{
					throw new ArgumentException("Syntax tree already present.", $"{nameof(trees)}[{i}]");
				}

				treeList.Add(tree);
				i++;
			}
		}
		finally
		{
			externalSyntaxTrees.Free();
		}

		if (treeList.Count == 0)
		{
			return this;
		}

		// TODO: RemoveSyntaxTrees/ReplaceSyntaxTree and reuse of the existing declaration state.
		return Update(_syntaxAndDeclarations.AddSyntaxTrees(treeList));
	}
}