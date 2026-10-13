using NiteCompiler.CodeAnalysis.Declarations;
using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Tests;

public class DeclarationTests
{
	[Test]
	public async Task BuildsNestedModulesAndTypes(CancellationToken cancellationToken)
	{
		var tree = SyntaxTree.FromText(
			"""
			module Outer;
			type Thing {
				value: i32;
				type Inner { }
			}
			""",
			cancellationToken: cancellationToken);

		var root = DeclarationTreeBuilder.ForTree(tree);

		await That(root.Members.Length).IsEqualTo(1);

		var outer = (SingleModuleDeclaration)root.Members[0];
		await That(outer.Name).IsEqualTo("Outer");
		await That(outer.Members.Length).IsEqualTo(1);

		var thing = (SingleTypeDeclaration)outer.Members[0];
		await That(thing.Name).IsEqualTo("Thing");
		// Field `value` is not a declaration; only the nested type is.
		await That(thing.Members.Length).IsEqualTo(1);
		await That(((SingleTypeDeclaration)thing.Members[0]).Name).IsEqualTo("Inner");
	}

	[Test]
	public async Task MergedRootIncludesAllTrees(CancellationToken cancellationToken)
	{
		var first = SyntaxTree.FromText("module A;", cancellationToken: cancellationToken);
		var second = SyntaxTree.FromText("module B;", cancellationToken: cancellationToken);

		var table = new DeclarationTable([first, second]);
		var merged = table.GetMergedRoot();

		await That(merged.Declarations.Length).IsEqualTo(2);
	}
}