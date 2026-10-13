global using static TUnit.Assertions.Assert;

using NiteCompiler.CodeAnalysis.Syntax;

[assembly: Timeout(10_000)]
namespace NiteCompiler.Tests;

public class ParsingTests
{
	[Test]
	public async Task ParseEmptyFile(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText(string.Empty, cancellationToken: cancellationToken);

		await That(tree.Root.Items).IsEmpty();
	}

	[Test]
	public async Task ParseOnlySpaces(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText(" \n\r\n\t  \t\v", cancellationToken: cancellationToken);

		await That(tree.GetDiagnostics()).IsEmpty();
	}

	[Test]
	public async Task ParseShebang(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("#! usr/bin/python3", cancellationToken: cancellationToken);

		await That(tree.Root.Items).IsEmpty();
		await That(tree.Root.EndOfFileToken.LeadingTrivia[0].Kind).IsEqualTo(SyntaxKind.ShebangTrivia);
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseRandomCharacters(CancellationToken cancellationToken)
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

		await That(tree.Root.FullSpan.Length).IsEqualTo(randomText.Length);
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
	public async Task ParseBinaryExpression(string op, CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText($$"""
		                                public main() {
		                                    2 {{op}} 3;
		                                }
		                                """, cancellationToken: cancellationToken);

		FunctionDeclarationSyntax? function;
		await That(function = tree.Root.Items[0] as FunctionDeclarationSyntax).IsNotNull();
		await That((function?.BodyStatement as BlockStatementSyntax)?.Statements[0]).IsTypeOf<ExpressionStatementSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseEmptyType(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("public type Vec3 {}", cancellationToken: cancellationToken);

		await That(tree.Root.Items[0]).IsTypeOf<TypeDeclarationSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseType(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public type Vec3 {
		                                   public x: f32;
		                                   public y: f32;
		                                   public z: f32;
		                               }
		                               """, cancellationToken: cancellationToken);

		await That(tree.Root.Items[0]).IsNotNull().And.IsTypeOf<TypeDeclarationSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseEmptyModule(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("module test_name123;");

		await That(tree.Root.Items).Count().IsEqualTo(1);
		await That(tree.Root.Items[0]).IsTypeOf<ModuleDeclarationSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseModule(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               module test_name123;

		                               public main() {}
		                               """);

		await That(tree.Root.Items).Count().IsEqualTo(1);
		await That(tree.Root.Items[0]).IsTypeOf<ModuleDeclarationSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseEmptyFunction(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("public main() {}");

		await That(tree.Root.Items[0]).IsTypeOf<FunctionDeclarationSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseEmptyFunctionSemicolon(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("public main();");

		await That(tree.Root.Items[0]).IsTypeOf<FunctionDeclarationSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseLet(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   let x: i32 = 1;
		                                   let y = 1;
		                                   let z: i32;
		                               }
		                               """);

		FunctionDeclarationSyntax? function;
		await That(function = tree.Root.Items[0] as FunctionDeclarationSyntax).IsNotNull();

		BlockStatementSyntax? block = function?.BodyStatement as BlockStatementSyntax;
		await That(block?.Statements[0]).IsTypeOf<LetStatementSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseParams(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main(x: i32, y: Ananas) {
		                               }
		                               """);

		FunctionDeclarationSyntax? function;
		await That(function = tree.Root.Items[0] as FunctionDeclarationSyntax).IsNotNull();
		await That(function!.ParameterList.Parameters[0]).IsTypeOf<NamedParameterSyntax>();
		await That(function.ParameterList.Parameters[1]).IsTypeOf<NamedParameterSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ValidParent(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               module x;

		                               public type Foo {
		                                   public bar() -> i32 {
		                                       return 42;
		                                   }
		                               }
		                               """);

		await VerifyChild(tree.Root);

		async Task VerifyChild(SyntaxNode node)
		{
			foreach (SyntaxNode child in node.GetChildNodesAndTokens())
			{
				await That(child.Parent).IsEqualTo(node);
				await VerifyChild(child);
			}
		}

		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ChildWithinPosition(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("public main() {}", cancellationToken: cancellationToken);

		const int namePosition = 7;// sizeof("public ")

		await That(tree.Root.GetChildWithinPosition(namePosition)).IsTypeOf<FunctionDeclarationSyntax>();

		var function = (FunctionDeclarationSyntax)tree.Root.GetChildWithinPosition(namePosition)!;
		await That(function.GetChildWithinPosition(namePosition)).IsTypeOf<PlainNameSyntax>();

		var name = (PlainNameSyntax)function.GetChildWithinPosition(namePosition)!;
		var token = name.GetChildWithinPosition(namePosition);
		await That(token).IsTypeOf<SyntaxToken>();
		await That(((SyntaxToken)token!).Text).IsEqualTo("main");

		await That(tree.Root.GetChildWithinPosition(1000)).IsNull();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}
}