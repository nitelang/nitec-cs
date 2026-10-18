using NiteCompiler.Compilation;
using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Tests;

public class SemanticModelTests
{
	private static readonly NiteCompilationOptions Options = new();

	[Test]
	public async Task GetSemanticModelReturnsCachedModelForTree(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("module A;", cancellationToken: cancellationToken);
		var compilation = NiteCompilation.Create(null, Options, [tree]);

		var first = compilation.GetSemanticModel(tree);
		var second = compilation.GetSemanticModel(tree);

		await That(first.SyntaxTree).IsSameReferenceAs(tree);
		await That(second).IsSameReferenceAs(first);
		await That(first.Compilation).IsSameReferenceAs(compilation);
	}

	[Test]
	public async Task GetSemanticModelExposesParseDiagnostics(CancellationToken cancellationToken)
	{
		// Missing module terminator is a parse error.
		var tree = SyntaxTree.FromText("module A", cancellationToken: cancellationToken);
		var compilation = NiteCompilation.Create(null, Options, [tree]);

		var diagnostics = compilation.GetSemanticModel(tree).GetDiagnostics(cancellationToken);

		await That(diagnostics).IsNotEmpty();
	}

	[Test]
	public async Task GetSemanticModelRejectsForeignTree(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("module A;", cancellationToken: cancellationToken);
		var foreign = SyntaxTree.FromText("module B;", cancellationToken: cancellationToken);
		var compilation = NiteCompilation.Create(null, Options, [tree]);

		await That(() => compilation.GetSemanticModel(foreign)).Throws<ArgumentException>();
	}
}