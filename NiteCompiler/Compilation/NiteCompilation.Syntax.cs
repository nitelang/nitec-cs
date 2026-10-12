using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Utilities;

namespace NiteCompiler.Compilation;

public partial class NiteCompilation
{
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