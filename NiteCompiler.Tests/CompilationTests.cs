using NiteCompiler.Compilation;

namespace NiteCompiler.Tests;

public class CompilationTests
{
	private static readonly NiteCompilationOptions Options = new();

	[Test]
	public async Task EmptyCompilation()
	{
		var compilation = NiteCompilation.Create(null, Options, []);

		await That(compilation).IsNotNull();
		await That(compilation.SyntaxTrees).IsEmpty();
	}
}