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
		// TODO: Implement name unwrapping
		// module x::y::z; should produce x{y{z}} declaration, not single item!

		var members = VisitModuleMembers(node, node.Members);

		NameSyntax name = node.Name;
		SyntaxNode currentNode = node;

		while (name is PathNameSyntax path)
		{
			SingleModuleDeclaration module = new(
				name: name.UnqualifiedName.GetIdentifierValue(),
				syntax: currentNode,
				nameLocation: path.Right.GetLocation(),
				members: members,
				diagnostics: []
			);

			members = [module];

			currentNode = name = path.Left;
		}

		return new SingleModuleDeclaration(
			name: name.UnqualifiedName.GetIdentifierValue(), // ain't sure about UnqualifiedName.GetIdentifierValue() is what I want, will check later
			syntax: currentNode,
			nameLocation: name.GetLocation(),
			members: members,
			diagnostics: []
		);
	}

	public override SingleItemDeclaration VisitTypeDeclaration(TypeDeclarationSyntax node)
	{
		// TODO: lifetime arity/arity once generics exist; modifiers once the parser produces them;
		// MemberNames for name queries (Roslyn SingleTypeDeclaration.MemberNames).
		var members = VisitTypeMembers(node, node.Members);

		return new SingleTypeDeclaration(
			node.Name.GetIdentifierValue(),
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
}