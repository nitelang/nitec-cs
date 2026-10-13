using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Declarations;

internal sealed class DeclarationTreeBuilder : SyntaxVisitor<SingleItemDeclaration>
{
	public static RootModuleDeclaration ForTree(SyntaxTree syntaxTree)
	{
		DeclarationTreeBuilder builder = new();
		return (RootModuleDeclaration)builder.Visit(syntaxTree.Root)!;
	}

	public override SingleItemDeclaration VisitRoot(RootSyntax node)
	{
		var members = VisitModuleMembers(node, node.Items);

		return new RootModuleDeclaration(node, members);
	}

	public override SingleItemDeclaration VisitModuleDeclaration(ModuleDeclarationSyntax node)
	{
		var members = VisitModuleMembers(node, node.Members);

		return new SingleModuleDeclaration(
			GetName(node.Name),
			node,
			node.Name.GetLocation(),
			members,
			diagnostics: []);
	}

	public override SingleItemDeclaration VisitTypeDeclaration(TypeDeclarationSyntax node)
	{
		// TODO: lifetime arity/arity once generics exist; modifiers once the parser produces them;
		// MemberNames for name queries (Roslyn SingleTypeDeclaration.MemberNames).
		var members = VisitTypeMembers(node, node.Members);

		return new SingleTypeDeclaration(
			GetName(node.Name),
			lifetimeArity: 0,
			arity: 0,
			GetAccessibility(node),
			DeclarationModifiers.None,
			node,
			node.Name.GetLocation(),
			members,
			diagnostics: []);
	}

	private ImmutableArray<SingleItemDeclaration> VisitModuleMembers(SyntaxNode node, SyntaxList<ItemSyntax> members)
	{
		if (members.Count == 0)
		{
			return [];
		}

		var memberBuilder = ArrayBuilder<SingleItemDeclaration>.GetInstance();
		foreach (var member in members)
		{
			SingleItemDeclaration? item = Visit(member);

			memberBuilder.AddIfNotNull(item);
		}

		return memberBuilder.ToImmutableAndFree();
	}

	private ImmutableArray<SingleItemDeclaration> VisitTypeMembers(SyntaxNode node, SyntaxList<MemberSyntax>? members)
	{
		if (members == null || members.Value.Count == 0)
		{
			return [];
		}

		var memberBuilder = ArrayBuilder<SingleItemDeclaration>.GetInstance();
		foreach (var member in members)
		{
			SingleItemDeclaration? item = Visit(member);

			memberBuilder.AddIfNotNull(item);
		}

		return memberBuilder.ToImmutableAndFree();
	}

	private static DeclarationAccessibility GetAccessibility(MemberSyntax member)
	{
		return member.AccessibilityModifierToken?.Kind switch
		{
			SyntaxKind.FileKeyword => DeclarationAccessibility.File,
			SyntaxKind.PublicKeyword => DeclarationAccessibility.Public,
			_ => DeclarationAccessibility.None
		};
	}

	private static string GetName(NameSyntax name)
	{
		return name switch
		{
			PlainNameSyntax plain => GetName(plain),
			PathNameSyntax path => GetName(path.Right),
			_ => string.Empty
		};
	}

	[ToRemoval("PlainNameSyntax should provide identifier value via getter or field")]
	private static string GetName(PlainNameSyntax name)
	{
		return name is IdentifierNameSyntax identifier ? identifier.Identifier.Text : string.Empty;
	}
}