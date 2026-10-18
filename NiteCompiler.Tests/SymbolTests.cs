using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Compilation;
using NiteCompiler.Symbols;

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

		await That(compilation.GlobalModule.GetTypeMembers("Xyz", 0)).IsNotEmpty();
	}

	[Test]
	public async Task ModuleMember(CancellationToken cancellationToken)
	{
		var compilation = FromText(
			"""
			module A;
			type T {}
			""");

		ModuleSymbol global = compilation.GlobalModule;
		ModuleSymbol? a = global.GetNestedModule("A");

		await That(a).IsNotNull();
		await That(a!.GetMembers()).IsNotEmpty();
		await That(a.GetMembers("T")).IsNotEmpty();
		await That(a.GetMembers("Nope")).IsEmpty();
		await That(a.GetTypeMembers()).IsNotEmpty();
		await That(a.GetTypeMembers("T", arity: null)).IsNotEmpty();
		await That(a.GetTypeMembers("T", arity: 0)).IsNotEmpty();
		await That(a.GetTypeMembers("T", arity: 1)).IsEmpty();
		await That(a.GetMembers("T")[0]).IsSameReferenceAs(a.GetTypeMembers("T", arity: 0)[0]);
	}
}