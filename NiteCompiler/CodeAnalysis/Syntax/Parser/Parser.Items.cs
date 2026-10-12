using System.Diagnostics;
using NiteCompiler.Diagnostics;
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
		SyntaxToken module = PeekThenAdvance();
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

	private static bool IsMemberModifier(SyntaxKind kind)
	{
		return kind switch
		{
			_ => false
		};
	}

	private static bool IsContextualStop(SyntaxKind kind)
	{
		// curr,   next
		// unsized type - valid
		// unsized (    - invalid: unsized is not a contextual keyword, but name of a function
		// unsized ::   - invalid: part of path
		// unsized :    - invalid: this is just invalid, suppose user just forgot second colon, or accidentally put space between

		return kind switch
		{
			SyntaxKind.OpenBrace => true,
			SyntaxKind.OpenParen => true,
			SyntaxKind.OpenBracket => true,
			SyntaxKind.Colon => true,
			SyntaxKind.DoubleColon => true,
			SyntaxKind.EndOfFile => true,
			_ => false
		};
	}

	private MemberSyntax ParseMember()
	{
		SyntaxToken? accessibility = MatchContextualOrNot(SyntaxKind.FileKeyword, SyntaxKind.PublicKeyword);

		ArrayBuilder<SyntaxToken> modifiers = ArrayBuilder<SyntaxToken>.GetInstance();

		// TODO: Add modifier tests
		while (IsMemberModifier(Current.Kind) && (!Current.Kind.IsContextual || IsContextualStop(Peek(1).Kind)))
		{
			modifiers.Add(PeekThenAdvance());
		}

		if (Current.Kind == SyntaxKind.TypeKeyword)
		{
			SyntaxToken type = PeekThenAdvance();
			return ParseTypeDeclaration(accessibility, modifiers.ToSyntaxListAndFree(), type);
		}
		else
		{
			PlainNameSyntax name = ParsePlainName();

			if (Current.Kind == SyntaxKind.OpenParen)
			{
				return ParseFunctionDeclaration(accessibility, modifiers.ToSyntaxListAndFree(), name);
			}

			// else: field
			TypeClauseSyntax type = ParseTypeClause();

			SyntaxToken semicolon = MatchToken(SyntaxKind.Semicolon);

			return new FieldDeclarationSyntax(_tree, SyntaxList<AttributeListSyntax>.Empty, accessibility, modifiers.ToSyntaxListAndFree(), name, type, semicolon);
		}
	}

	private TypeDeclarationSyntax ParseTypeDeclaration(SyntaxToken? accessibility, SyntaxList<SyntaxToken> modifiers, SyntaxToken type)
	{
		PlainNameSyntax name = ParsePlainName();
		SyntaxToken openBrace = MatchToken(SyntaxKind.OpenBrace);

		ArrayBuilder<MemberSyntax> members = ArrayBuilder<MemberSyntax>.GetInstance();
		while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.CloseBrace))
		{
			members.Add(ParseMember());
		}
		SyntaxToken closeBrace = MatchToken(SyntaxKind.CloseBrace);

		return new TypeDeclarationSyntax(_tree, SyntaxList<AttributeListSyntax>.Empty, accessibility, modifiers, type, name, openBrace, members.ToSyntaxListAndFree(), closeBrace);
	}

	private FunctionDeclarationSyntax ParseFunctionDeclaration(SyntaxToken? accessibility, SyntaxList<SyntaxToken> modifiers, PlainNameSyntax name)
	{
		ParameterListSyntax parameters = ParseParameterList();

		ReturnTypeClauseSyntax? returnType = null;
		if (Current.Kind == SyntaxKind.Arrow)
		{
			returnType = ParseReturnTypeClause();
		}

		StatementSyntax body = ParseStatement();

		return new FunctionDeclarationSyntax(_tree, SyntaxList<AttributeListSyntax>.Empty, accessibility, modifiers, name, parameters, returnType, body);
	}

	#region Arguments & Parameters

	private ParameterSyntax ParseParameter()
	{
		PlainNameSyntax name = ParsePlainName();

		TypeClauseSyntax type = ParseTypeClause();

		return new NamedParameterSyntax(_tree, name, type);
	}

	private ParameterListSyntax ParseParameterList()
	{
		SyntaxToken openParen = MatchToken(SyntaxKind.OpenParen);
		ArrayBuilder<SyntaxNode> parameters = ArrayBuilder<SyntaxNode>.GetInstance();

		if (Current.Kind == SyntaxKind.Comma)
		{
			_diagnostics.ReportUnexpectedToken(Current.GetLocation(), Current.Kind);
			PeekThenAdvance(); // TODO: Merge with next token?
		}

		while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.CloseParen))
		{
			parameters.Add(ParseParameter());

			if (Current.Kind == SyntaxKind.Comma)
			{
				parameters.Add(PeekThenAdvance());
			}
			else
			{
				break;
			}
		}

		SyntaxToken closeParen = MatchToken(SyntaxKind.CloseParen);
		return new ParameterListSyntax(_tree, openParen, parameters.ToSeparatedSyntaxListAndFree<ParameterSyntax>(), closeParen);
	}

	private ArgumentSyntax ParseArgument()
	{
		ExplicitArgumentClauseSyntax? argumentName = null;
		if (Current.Kind is SyntaxKind.Identifier or SyntaxKind.EscapedIdentifier && Peek(1).Kind is SyntaxKind.Colon)
		{
			argumentName = ParseExplicitArgumentClause();
		}

		ExpressionSyntax expression = ParseExpression();

		return new ArgumentSyntax(_tree, argumentName, expression);
	}

	private ArgumentListSyntax ParseArgumentList()
	{
		SyntaxToken openParen = MatchToken(SyntaxKind.OpenParen);
		ArrayBuilder<SyntaxNode> parameters = ArrayBuilder<SyntaxNode>.GetInstance();

		if (Current.Kind == SyntaxKind.Comma)
		{
			_diagnostics.ReportUnexpectedToken(new SourceLocation(_tree, Current.Span), Current.Kind);
			PeekThenAdvance(); // TODO: Merge with next token?
		}

		while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.CloseParen))
		{
			parameters.Add(ParseArgument());

			if (Current.Kind == SyntaxKind.Comma)
			{
				parameters.Add(PeekThenAdvance());
			}
			else
			{
				break;
			}
		}

		SyntaxToken closeParen = MatchToken(SyntaxKind.CloseParen);
		return new ArgumentListSyntax(_tree, openParen, parameters.ToSeparatedSyntaxListAndFree<ArgumentSyntax>(), closeParen);
	}

	private ArgumentListSyntax ParseBracketedArgumentList()
	{
		SyntaxToken openParen = MatchToken(SyntaxKind.OpenBracket);
		ArrayBuilder<SyntaxNode> parameters = ArrayBuilder<SyntaxNode>.GetInstance();

		if (Current.Kind == SyntaxKind.Comma)
		{
			_diagnostics.ReportUnexpectedToken(Current.GetLocation(), Current.Kind);
			PeekThenAdvance(); // TODO: Append as trailing trivia
		}

		while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.CloseBracket))
		{
			parameters.Add(ParseArgument());

			if (Current.Kind == SyntaxKind.Comma)
			{
				parameters.Add(PeekThenAdvance());
			}
			else
			{
				break;
			}
		}

		SyntaxToken closeParen = MatchToken(SyntaxKind.CloseBracket);
		return new ArgumentListSyntax(_tree, openParen, parameters.ToSeparatedSyntaxListAndFree<ArgumentSyntax>(), closeParen);
	}

	#endregion

	#region Clauses

	private TypeClauseSyntax ParseTypeClause()
	{
		SyntaxToken colon = MatchToken(SyntaxKind.Colon);

		TypeSyntax type = ParseType();

		return new TypeClauseSyntax(_tree, colon, type);
	}

	private ReturnTypeClauseSyntax ParseReturnTypeClause()
	{
		SyntaxToken arrow = MatchToken(SyntaxKind.Arrow);

		TypeSyntax type = ParseType();

		return new ReturnTypeClauseSyntax(_tree, arrow, type);
	}

	private ExplicitArgumentClauseSyntax ParseExplicitArgumentClause()
	{
		PlainNameSyntax name = ParsePlainName();

		SyntaxToken colon = MatchToken(SyntaxKind.Colon);

		return new ExplicitArgumentClauseSyntax(_tree, name, colon);
	}

	private EqualsClauseSyntax ParseEqualsClause()
	{
		SyntaxToken equals = MatchToken(SyntaxKind.Equals);

		ExpressionSyntax expression = ParseExpression();

		return new EqualsClauseSyntax(_tree, equals, expression);
	}

	#endregion
}