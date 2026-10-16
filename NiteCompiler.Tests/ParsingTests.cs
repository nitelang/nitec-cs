global using static TUnit.Assertions.Assert;

using NiteCompiler.CodeAnalysis;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;

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
	[Arguments("1", (uint)1, typeof(uint), SpecialType.None)]
	[Arguments("1u8", (uint)1, typeof(uint), SpecialType.UInt8)]
	[Arguments("0xFF", (uint)255, typeof(uint), SpecialType.None)]
	[Arguments("1'000", (uint)1000, typeof(uint), SpecialType.None)]
	[Arguments("2147483648", (uint)2147483648, typeof(uint), SpecialType.None)]
	[Arguments("2147483648i32", (uint)2147483648, typeof(uint), SpecialType.SInt32)]
	[Arguments("4294967296", (ulong)4294967296, typeof(ulong), SpecialType.None)]
	[Arguments("4294967296i64", (ulong)4294967296, typeof(ulong), SpecialType.SInt64)]
	[Arguments("1.5f16", 1.5f, typeof(Half), SpecialType.Float16)]
	[Arguments("1.5f32", 1.5f, typeof(float), SpecialType.Float32)]
	[Arguments("1.5", 1.5f, typeof(float), SpecialType.None)]
	[Arguments("1.5f64", 1.5, typeof(double), SpecialType.Float64)]
	[Arguments("1.5e2", 150f, typeof(float), SpecialType.None)]
	[Arguments("1.5e2f64", 150.0, typeof(double), SpecialType.Float64)]
	[Arguments("1'234.5", 1234.5f, typeof(float), SpecialType.None)]
	[Arguments("5f32", 5f, typeof(float), SpecialType.Float32)]
	public async Task ParseNumberLiteralExpression(string literal, object value, Type type, SpecialType explicitType, CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText($$"""
		                                 public main() {
		                                     let x = {{literal}};
		                                 }
		                                 """, cancellationToken: cancellationToken);

		FunctionDeclarationSyntax? function;
		LetStatementSyntax? letStatement;
		LiteralExpressionSyntax? literalExpression;
		await That(function = tree.Root.Items[0] as FunctionDeclarationSyntax).IsNotNull();
		await That(letStatement = (function!.BodyStatement as BlockStatementSyntax)?.Statements[0] as LetStatementSyntax).IsNotNull();
		await That(literalExpression = (letStatement!.Equals!.Expression) as LiteralExpressionSyntax).IsNotNull();
		// attribute arguments cannot hold a Half, so the f16 case supplies a float
		object expected = type == typeof(Half) ? (Half)(float)value : value;
		await That(literalExpression!.Literal.LiteralValue).IsOfType(type)
			.And.IsEqualTo(expected);
		await That(literalExpression.Literal.ExplicitType).IsEqualTo(explicitType);
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseTooLargeIntegralLiteral(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   let x = 340282366920938463463374607431768211456;
		                               }
		                               """, cancellationToken: cancellationToken);

		var diagnostics = tree.GetDiagnostics(cancellationToken).ToList();
		await That(diagnostics).Count().IsEqualTo(1);
		await That(diagnostics).IsNotEmpty()
			.And.HasSingleItem(d => d.Descriptor == DiagnosticDescriptors.NumericLiteralTooLarge);
	}

	[Test]
	public async Task ParseTooLargeFloatingLiteral(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   let x = 1e100f16;
		                               }
		                               """, cancellationToken: cancellationToken);

		await That(tree.GetDiagnostics(cancellationToken)).IsNotEmpty()
			.And.HasSingleItem(d => d.Descriptor == DiagnosticDescriptors.NumericLiteralTooLarge);
	}

	[Test]
	public async Task ParseUInt128IntegralLiteralFallback(CancellationToken cancellationToken)
	{
		// 2^64, one above ulong.MaxValue: the fast path must refuse and the UInt128 fallback must accept it.
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   let x = 18446744073709551616;
		                               }
		                               """, cancellationToken: cancellationToken);

		var function = (FunctionDeclarationSyntax)tree.Root.Items[0]!;
		var block = (BlockStatementSyntax)function.BodyStatement!;
		var letStatement = (LetStatementSyntax)block.Statements[0];
		var literal = (LiteralExpressionSyntax)letStatement.Equals!.Expression!;

		await That(literal.Literal.LiteralValue).IsOfType(typeof(UInt128))
			.And.IsEqualTo((UInt128)ulong.MaxValue + 1);
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
	public async Task ParseMissingType(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public type Vec3 {
		                                   public x: ;
		                               }
		                               """, cancellationToken: cancellationToken);

		var declaration = (TypeDeclarationSyntax)tree.Root.Items[0]!;
		var field = (FieldDeclarationSyntax)declaration.Members[0]!;

		await That(((IdentifierNameSyntax)field.Type.Type).Identifier.IsMissing).IsTrue();

		var diagnostics = tree.GetDiagnostics(cancellationToken).ToList();
		await That(diagnostics).Count().IsEqualTo(1);
		await That(diagnostics[0].Descriptor.Id).IsEqualTo("expected-type");
	}

	[Test]
	public async Task ParseInvalidTypeToken(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public type Vec3 {
		                                   public x: 42;
		                               }
		                               """, cancellationToken: cancellationToken);

		await That(tree.Root.Items[0]).IsTypeOf<TypeDeclarationSyntax>();
		await That(tree.GetDiagnostics(cancellationToken).Any(d => d.Descriptor.Id == "expected-type")).IsTrue();
	}

	[Test]
	public async Task ParseMissingNameAfterPathSeparator(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public type Vec3 {
		                                   public x: a::;
		                               }
		                               """, cancellationToken: cancellationToken);

		var declaration = (TypeDeclarationSyntax)tree.Root.Items[0]!;
		var field = (FieldDeclarationSyntax)declaration.Members[0]!;

		await That(((IdentifierNameSyntax)((PathNameSyntax)field.Type.Type).Right).Identifier.IsMissing).IsTrue();
		await That(tree.GetDiagnostics(cancellationToken).Any(d => d.Descriptor.Id == "expected-identifier")).IsTrue();
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
	public async Task ParseParamsWithTrailingComma(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main(x: i32, y: Ananas,) {
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

	[Test]
	public async Task ParseIf(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   if 1 {
		                                       return 2;
		                                   }
		                               }
		                               """, cancellationToken: cancellationToken);

		FunctionDeclarationSyntax? function;
		IfStatementSyntax? ifStatement;
		await That(function = tree.Root.Items[0] as FunctionDeclarationSyntax).IsNotNull();
		await That(ifStatement = (function!.BodyStatement as BlockStatementSyntax)?.Statements[0] as IfStatementSyntax).IsNotNull();
		await That(ifStatement!.IfKeyword.Kind).IsEqualTo(SyntaxKind.IfKeyword);
		await That(ifStatement.Condition.Kind).IsEqualTo(SyntaxKind.NumericLiteralExpression);
		await That(ifStatement.Then).IsTypeOf<BlockStatementSyntax>();
		await That(ifStatement.ElseClause).IsNull();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseIfElse(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   if 1 {
		                                   } else {
		                                   }
		                               }
		                               """, cancellationToken: cancellationToken);

		var function = (FunctionDeclarationSyntax)tree.Root.Items[0]!;
		var ifStatement = (IfStatementSyntax)((BlockStatementSyntax)function.BodyStatement!).Statements[0];

		await That(ifStatement.ElseClause).IsNotNull();
		await That(ifStatement.ElseClause!.ElseKeyword.Kind).IsEqualTo(SyntaxKind.ElseKeyword);
		await That(ifStatement.ElseClause.Statement).IsTypeOf<BlockStatementSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseElseIf(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   if 1 {
		                                   } else if 2 {
		                                   }
		                               }
		                               """, cancellationToken: cancellationToken);

		var function = (FunctionDeclarationSyntax)tree.Root.Items[0]!;
		var ifStatement = (IfStatementSyntax)((BlockStatementSyntax)function.BodyStatement!).Statements[0];

		await That(ifStatement.ElseClause).IsNotNull();
		var elseIf = ifStatement.ElseClause!.Statement as IfStatementSyntax;
		await That(elseIf).IsNotNull();
		await That(((LiteralExpressionSyntax)elseIf!.Condition).Literal.Text).IsEqualTo("2");
		await That(elseIf.ElseClause).IsNull();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseNestedIf(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   if true {
		                                       if false {
		                                       }
		                                   }
		                               }
		                               """, cancellationToken: cancellationToken);

		var function = (FunctionDeclarationSyntax)tree.Root.Items[0]!;
		var outer = (IfStatementSyntax)((BlockStatementSyntax)function.BodyStatement!).Statements[0];
		var inner = ((BlockStatementSyntax)outer.Then).Statements[0];

		await That(inner).IsTypeOf<IfStatementSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsEmpty();
	}

	[Test]
	public async Task ParseIfWithoutBlock(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText("""
		                               public main() {
		                                   if 1 return 2;
		                               }
		                               """, cancellationToken: cancellationToken);

		var function = (FunctionDeclarationSyntax)tree.Root.Items[0]!;
		var ifStatement = (IfStatementSyntax)((BlockStatementSyntax)function.BodyStatement!).Statements[0];

		await That(ifStatement.Then).IsTypeOf<ReturnStatementSyntax>();
		await That(tree.GetDiagnostics(cancellationToken)).IsNotEmpty()
			.And.HasSingleItem(d => d.Descriptor == DiagnosticDescriptors.ExpectedBlock);
	}
}