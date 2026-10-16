using NiteCompiler.Diagnostics;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	private StatementSyntax ParseStatement(bool parseAttributes = false)
	{
		SyntaxList<AttributeListSyntax> attributes = parseAttributes
			? ParseAttributeLists()
			: SyntaxList<AttributeListSyntax>.Empty;

		switch (Current.Kind)
		{
			case SyntaxKind.OpenBrace:
				return ParseBlock(attributes);
			case SyntaxKind.LetKeyword:
				SyntaxToken let = PeekThenAdvance();
				PlainNameSyntax name = ParsePlainName();
				TypeClauseSyntax? type = Current.Kind is SyntaxKind.Colon ? ParseTypeClause() : null;
				EqualsClauseSyntax? init = Current.Kind is SyntaxKind.Equals ? ParseEqualsClause() : null;

				return new LetStatementSyntax(_tree, attributes, let, name, type, init);
			case SyntaxKind.IfKeyword:
				return ParseIfStatement(attributes);
			case SyntaxKind.Semicolon:
				SyntaxToken semicolon = PeekThenAdvance();
				return new EmptyStatementSyntax(_tree, attributes, semicolon);
			case SyntaxKind.ReturnKeyword:
				SyntaxToken returnKeyword = PeekThenAdvance();

				ExpressionSyntax expression = ParseExpression();

				// TODO: Do not consume token on fail
				semicolon = MatchToken(SyntaxKind.Semicolon);

				return new ReturnStatementSyntax(_tree, attributes, returnKeyword, expression, semicolon);
			default:
				expression = ParseExpression();

				// TODO: Do not consume token on fail
				semicolon = MatchToken(SyntaxKind.Semicolon);

				return new ExpressionStatementSyntax(_tree, attributes, expression, semicolon);
		}
	}

	private BlockStatementSyntax ParseBlock(SyntaxList<AttributeListSyntax> attributes)
	{
		SyntaxToken openBrace = MatchToken(SyntaxKind.OpenBrace);

		ArrayBuilder<StatementSyntax> statements = ArrayBuilder<StatementSyntax>.GetInstance();
		while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.CloseBrace))
		{
			statements.Add(ParseStatement(parseAttributes: true));
		}
		SyntaxToken closeBrace = MatchToken(SyntaxKind.CloseBrace);

		return new BlockStatementSyntax(_tree, attributes, openBrace, statements.ToSyntaxListAndFree(), closeBrace);
	}

	private IfStatementSyntax ParseIfStatement(SyntaxList<AttributeListSyntax> attributes)
	{
		SyntaxToken ifKeyword = MatchToken(SyntaxKind.IfKeyword);

		ExpressionSyntax condition = ParseExpression();

		StatementSyntax thenStatement = ParseStatement();

		// if (expr) any_statement;
		// if expr { only_block_statement }
		// even though such syntax is forbidden in standard, we still better consume that for better UX
		if (condition.Kind != SyntaxKind.ParenthesizedExpression && thenStatement.Kind != SyntaxKind.BlockStatement)
		{
			_diagnostics.ReportExpectedBlock(thenStatement.GetLocation());
		}

		ElseClauseSyntax? elseClause = Current.Kind == SyntaxKind.ElseKeyword ? ParseElseClause() : null;

		return new IfStatementSyntax(_tree, attributes, ifKeyword, condition, thenStatement, elseClause);
	}

	private ElseClauseSyntax ParseElseClause()
	{
		SyntaxToken elseKeyword = MatchToken(SyntaxKind.ElseKeyword);

		StatementSyntax otherwiseStatement = ParseStatement();

		return new ElseClauseSyntax(_tree, elseKeyword, otherwiseStatement);
	}
}