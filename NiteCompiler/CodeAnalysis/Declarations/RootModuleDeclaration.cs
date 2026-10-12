using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;

namespace NiteCompiler.CodeAnalysis.Declarations;

internal sealed class RootModuleDeclaration : SingleModuleDeclaration
{
	public RootModuleDeclaration(SyntaxNode treeNode, ImmutableArray<SingleItemDeclaration> members)
		: base(string.Empty, treeNode, nameLocation: treeNode.GetLocation(), members, [])
	{
	}
}