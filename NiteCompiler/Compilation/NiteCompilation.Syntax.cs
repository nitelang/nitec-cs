using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Declarations;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Utilities;

namespace NiteCompiler.Compilation;

public partial class NiteCompilation
{
	/// <summary>
	/// The syntax trees (parsed from source code) that this compilation was created with.
	/// </summary>
	public ImmutableArray<SyntaxTree> SyntaxTrees => _syntaxAndDeclarations.CurrentState.SyntaxTrees;

	internal DeclarationTable Declarations => _syntaxAndDeclarations.CurrentState.DeclarationTable;

	internal MergedModuleDeclaration MergedRootDeclaration => Declarations.GetMergedRoot();

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

	/// <summary>
	/// Returns <see langword="true"/> if this compilation contains the specified tree; otherwise <see langword="false"/>.
	/// </summary>
	public bool ContainsSyntaxTree(SyntaxTree? syntaxTree)
	{
		return syntaxTree != null && _syntaxAndDeclarations.CurrentState.OrdinalMap.ContainsKey(syntaxTree);
	}

	internal int GetSyntaxTreeOrdinal(SyntaxTree tree)
	{
		Debug.Assert(ContainsSyntaxTree(tree));
		try
		{
			return _syntaxAndDeclarations.CurrentState.OrdinalMap[tree];
		}
		catch (KeyNotFoundException)
		{
			throw new KeyNotFoundException($"Syntax tree not found with file path: {tree.FilePath}");
		}
	}
}