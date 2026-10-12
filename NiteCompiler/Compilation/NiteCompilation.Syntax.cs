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

		if (!trees.Any())
		{
			return this;
		}

		PooledHashSet<SyntaxTree> externalSyntaxTrees = PooledHashSet<SyntaxTree>.GetInstance();
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
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

				i++;
			}
		}
		finally
		{
			externalSyntaxTrees.Free();
		}


		return Update(syntaxAndDeclarations);
	}
}