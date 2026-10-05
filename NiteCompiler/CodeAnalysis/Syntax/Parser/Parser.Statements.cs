using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	private StatementSyntax ParseStatement()
	{
		// TODO: read attributes
		SyntaxList<AttributeListSyntax> attributes = SyntaxList<AttributeListSyntax>.Empty;

		switch (Current.Kind)
		{
			case SyntaxKind.OpenBrace:
				SyntaxToken openBrace = PeekAndAdvance();

				ArrayBuilder<StatementSyntax> statements = ArrayBuilder<StatementSyntax>.GetInstance();
				while (Current.Kind is not (SyntaxKind.EndOfFile or SyntaxKind.CloseBrace))
				{
					statements.Add(ParseStatement());
				}
				SyntaxToken closeBrace = MatchToken(SyntaxKind.CloseBrace);

				return new BlockStatementSyntax(_tree, attributes, openBrace, statements.ToSyntaxListAndFree(), closeBrace);
			case SyntaxKind.Semicolon:
				SyntaxToken semicolon = PeekAndAdvance();
				return new EmptyStatementSyntax(_tree, attributes, semicolon);
			case SyntaxKind.ReturnKeyword:
				SyntaxToken returnKeyword = PeekAndAdvance();

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
}