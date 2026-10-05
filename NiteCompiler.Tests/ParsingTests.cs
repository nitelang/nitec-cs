using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;
using static TUnit.Assertions.Assert;

namespace NiteCompiler.Tests;

[Timeout(10_000)]
public class ParsingTests
{
	[Test]
	public async Task ZeroLengthFileTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText(string.Empty, cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}

	[Test]
	public async Task FileFilledWithSpacesTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText(" \n\r\n\t  \t\v", cancellationToken: cancellationToken);

		await That(tree.GetDiagnostics()).IsEmpty();
	}

	[Test]
	public async Task ShebangTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("#! usr/bin/python3", cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}

	[Test]
	public async Task RandomCharactersTest(CancellationToken cancellationToken)
	{
		Random rand = Random.Shared;

		int length = rand.Next(2048, 2048 * 12);
		char[] c = new char[length];

		unsafe
		{
			fixed (char* pC = c)
			{
				Span<byte> bytes = new(pC, length * sizeof(char));
				rand.NextBytes(bytes);
			}
		}
		string randomText = new(c);

		var tree = SyntaxTree.FromText(randomText, cancellationToken: cancellationToken);

		await That(tree).IsNotNull();
	}

	[Test]
	[Arguments("+")]
	[Arguments("-")]
	[Arguments("/")]
	[Arguments("*")]
	[Arguments("%")]
	[Arguments(">>")]
	[Arguments("<<")]
	[Arguments(">>>")]
	public async Task ExpressionsTest(string op, CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText($$"""
		                                public main() {
		                                    2 {{op}} 3;
		                                }
		                                """, cancellationToken: cancellationToken);

		FunctionDeclarationSyntax? function;
		await That(function = tree.Root.Items[0] as FunctionDeclarationSyntax).IsNotNull();
		await That((function?.BodyStatement as BlockStatementSyntax)?.Statements[0]).IsTypeOf<ExpressionStatementSyntax>();
	}

	[Test]
	public async Task ParseEmptyTypeTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("public type Vec3 {}", cancellationToken: cancellationToken);

		await That(tree.Root.Items[0]).IsTypeOf<TypeDeclarationSyntax>();
	}

	[Test]
	public async Task ParseTypeTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public type Vec3 {
		                                   public x: f32;
		                                   public y: f32;
		                                   public z: f32;
		                               }
		                               """, cancellationToken: cancellationToken);

		await That(tree.Root.Items[0]).IsNotNull().And.IsTypeOf<TypeDeclarationSyntax>();
	}

	[Test]
	public async Task ParseEmptyModuleTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("module test_name123;");

		await That(tree.Root.Items).Count().IsEqualTo(1);
		await That(tree.Root.Items[0]).IsTypeOf<ModuleDeclarationSyntax>();
	}

	[Test]
	public async Task ParseModuleTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               module test_name123;

		                               public main() {}
		                               """);

		await That(tree.Root.Items).Count().IsEqualTo(1);
		await That(tree.Root.Items[0]).IsTypeOf<ModuleDeclarationSyntax>();
	}

	[Test]
	public async Task ParseEmptyFunctionTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("public main() {}");

		await That(tree).IsNotNull();
		await That(tree.GetDiagnostics(cancellationToken)).All(t => t.Descriptor.Severity < DiagnosticSeverity.Warning);
	}

	[Test]
	public async Task ParseEmptyFunctionSemicolonTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("public main();");

		await That(tree).IsNotNull();
		await That(tree.GetDiagnostics(cancellationToken)).All(t => t.Descriptor.Severity < DiagnosticSeverity.Warning);
	}

	[Test]
	public async Task ParseLetTest(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   let x: i32 = 1;
		                                   let y = 1;
		                                   let z: i32;
		                               }
		                               """);

		await That(tree).IsNotNull();
		await That(tree.GetDiagnostics(cancellationToken)).All(t => t.Descriptor.Severity < DiagnosticSeverity.Warning);
	}
}