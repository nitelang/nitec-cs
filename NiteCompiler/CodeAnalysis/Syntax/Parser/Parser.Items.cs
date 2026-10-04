using System.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	public RootSyntax ParseRoot()
	{
		ArrayBuilder<ItemSyntax> items = ArrayBuilder<ItemSyntax>.GetInstance();
		while (Current.Kind != SyntaxKind.EndOfFile)
		{
			items.Add(ParseItem());
		}

		SyntaxToken endOfFile = MatchToken(SyntaxKind.EndOfFile);

		return new RootSyntax(_tree, items.ToSyntaxListAndFree(), endOfFile);
	}

	private ItemSyntax ParseItem()
	{
		// TODO: attributes

		if (Current.Kind == SyntaxKind.ModuleKeyword)
		{
			return ParseModuleDeclaration();
		}

		return ParseMember();
	}

	private ModuleDeclarationSyntax ParseModuleDeclaration()
	{
		SyntaxToken module = PeekAndAdvance();
		Debug.Assert(module.Kind == SyntaxKind.ModuleKeyword);

		PlainNameSyntax name = ParsePlainName();

		SyntaxToken semicolon = MatchToken(SyntaxKind.Semicolon);
		// TODO: Maybe change to <MemberSyntax>, unless there will be an Item subclass, beside Module
		ArrayBuilder<ItemSyntax> members = ArrayBuilder<ItemSyntax>.GetInstance();
		while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.ModuleKeyword))
		{
			members.Add(ParseItem());
		}

		return new ModuleDeclarationSyntax(_tree, SyntaxList<AttributeListSyntax>.Empty, module, name, semicolon, members.ToSyntaxListAndFree());
	}

	private MemberSyntax ParseMember()
	{
		SyntaxToken? accessibility = MatchContextualOrNot(SyntaxKind.FileKeyword, SyntaxKind.PublicKeyword);

		if (Current.Kind == SyntaxKind.TypeKeyword)
		{
			SyntaxToken type = PeekAndAdvance();
			return ParseTypeDeclaration(accessibility, type);
		}
		else
		{
			return ParseFunctionDeclaration(accessibility);
		}
	}

	private TypeDeclarationSyntax ParseTypeDeclaration(SyntaxToken? accessibility, SyntaxToken type)
	{
		PlainNameSyntax name = ParsePlainName();
		SyntaxToken openBrace = MatchToken(SyntaxKind.OpenBrace);

		ArrayBuilder<MemberSyntax> members = ArrayBuilder<MemberSyntax>.GetInstance();
		while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.CloseBrace))
		{
			members.Add(ParseMember());
		}
		SyntaxToken closeBrace = MatchToken(SyntaxKind.CloseBrace);

		return new TypeDeclarationSyntax(_tree, SyntaxList<AttributeListSyntax>.Empty, accessibility, type, name, openBrace, members.ToSyntaxListAndFree(), closeBrace);
	}

	private MemberSyntax ParseFunctionDeclaration(SyntaxToken? accessibility)
	{
		throw new NotImplementedException();
	}
}