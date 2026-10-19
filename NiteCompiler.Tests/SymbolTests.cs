using System.Collections.Immutable;
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

	[Test]
	public async Task MembersAreSortedBySourceLocation(CancellationToken cancellationToken)
	{
		// The declaration layer emits all modules before all types, so insertion order
		// ([A, T, U]) differs from source order ([T, A, U]).
		SyntaxTree first = SyntaxTree.FromText("type T {}\nmodule A;");
		SyntaxTree second = SyntaxTree.FromText("type U {}");

		var compilation = NiteCompilation.Create(null, Options, [first, second]);

		ImmutableArray<Symbol> members = compilation.GlobalModule.GetMembers();

		await That(members.Length).IsEqualTo(3);
		await That(members[0].Name).IsEqualTo("T");
		await That(members[1].Name).IsEqualTo("A");
		await That(members[2].Name).IsEqualTo("U");

		ImmutableArray<NamedTypeSymbol> types = compilation.GlobalModule.GetTypeMembers();

		await That(types.Length).IsEqualTo(2);
		await That(types[0].Name).IsEqualTo("T");
		await That(types[1].Name).IsEqualTo("U");
	}
}