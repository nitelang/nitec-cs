using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Compilation;

namespace NiteCompiler.CodeAnalysis.Binding;

internal class RootBinder : Binder
{
	public RootBinder(NiteCompilation compilation, SyntaxTree syntaxTree) : base(compilation)
	{

	}

	public override Binder? GetBinder(SyntaxNode node) => null;
}