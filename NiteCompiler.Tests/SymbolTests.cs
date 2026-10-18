using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Compilation;

namespace NiteCompiler.Tests;

public class SymbolTests
{
	private static readonly NiteCompilationOptions Options = new();

	private static NiteCompilation FromText(string text)
	{
		SyntaxTree tree = SyntaxTree.FromText(text);

		return NiteCompilation.Create(null, Options, [tree]);
	}

	[Test]
	public async Task TypeSymbolName(CancellationToken cancellationToken)
	{
		var compilation = FromText("type Xyz {}");

		// TODO: more lookup
		await That(compilation.GlobalModule.GetTypeMembers("Xyz", 0)).IsNotEmpty();
	}
}