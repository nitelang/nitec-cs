using System.Diagnostics;
using NiteCompiler.Text;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	[Flags]
	private enum NameOptions
	{
		None = 0,
		InExpression = 1 << 0,
		InItemName = 1 << 1,
	}

	private ExpressionSyntax ParseExpression()
	{
		return ParseSubExpression(Precedence.Expression);
	}

	private ExpressionSyntax ParseSubExpression(Precedence precedence)
	{
		ExpressionSyntax result = Impl(precedence);

#if DEBUG
		// Check if acquired expression has precedence
		_ = result.Kind.GetPrecedence();
#endif

		return result;

		ExpressionSyntax Impl(Precedence implPrecedence)
		{
			return ParseExpressionContinued(ParsePrimaryOrUnaryExpression(implPrecedence), implPrecedence);
		}
	}

	private ExpressionSyntax ParsePrimaryOrUnaryExpression(Precedence precedence)
	{
		// primary expressions:
		// x, [...], x.y, x(...), x[...]
		return ParsePostFixExpression(ParsePrimaryExpressionWithoutPostfix(precedence));

		ExpressionSyntax ParsePrimaryExpressionWithoutPostfix(Precedence precedence)
		{
			SyntaxKind kind = Current.Kind;

			switch (kind)
			{
				case SyntaxKind.OpenParen:
					return ParseParenthesizedExpression();
				case SyntaxKind.NumericLiteral:
					SyntaxToken current = PeekAndAdvance();

					return new LiteralExpressionSyntax(SyntaxKind.NumericLiteralExpression, _tree, current);
				default:
					// TODO: error-prone
					// throw in here is just a temp stub, remove it ASAP!
					throw ExceptionUtilities.UnexpectedValue(kind);
			}
		}

		ExpressionSyntax ParsePostFixExpression(ExpressionSyntax expression)
		{
			while (true) // postfix
			{
				// This code is for future use
				SyntaxKind tokenKind = Current.Kind;
				/*if (tokenKind == SyntaxKind.OpenParen) // method invocation
				{
					expression = new InvocationExpressionSyntax(_syntaxTree, expression, ParseParenthesizedArgumentList());
				}
				else if (tokenKind == TokenKind.OpenBracket) // method invocation 2
				{
					expression = new IndexationExpressionSyntax(_syntaxTree, expression, ParseBracketedArgumentList());
				}
				else
				{
					return expression;
				}*/
				return expression;
			}
		}
	}

	private ExpressionSyntax ParseExpressionContinued(ExpressionSyntax unaryOrPrimaryExpression, Precedence precedence)
	{
		ExpressionSyntax currentExpression = unaryOrPrimaryExpression;

		while (TryExpandExpression(currentExpression, precedence) is { } expandedExpression)
		{
			currentExpression = expandedExpression;
		}

		return currentExpression;
	}

	private (SyntaxKind operatorTokenKind, SyntaxKind operatorExpressionKind) GetExpressionOperatorTokenKindAndExpressionKind()
	{
		// If the set of expression continuations is updated here, please review ParseStatementAttributeDeclarations
		// to see if it may need a similar look-ahead check to determine if something is a collection expression versus
		// an attribute.

		SyntaxToken token1 = Current;
		SyntaxKind token1Kind = token1.Kind;
		SyntaxToken token2 = Peek(1);

		// check for >>, >>=, >>> or >>>=
		//
		// In all those cases, update token1Kind to be the merged token kind.  It will then be handled by the code below.
		if (token1Kind == SyntaxKind.Greater
			&& (token2.Kind == SyntaxKind.Greater || token2.Kind == SyntaxKind.GreaterEquals)
			&& token1.IsBefore(token2)) // check to see if they really are adjacent
		{
			if (token2.Kind == SyntaxKind.Greater)
			{
				SyntaxToken token3 = Peek(2);
				if ((token3.Kind == SyntaxKind.Greater || token3.Kind == SyntaxKind.GreaterEquals)
					&& token2.IsBefore(token3)) // check to see if they really are adjacent
				{
					// >>>  or  >>>=
					token1Kind = token3.Kind == SyntaxKind.Greater
						? SyntaxKind.UnsignedRightShift
						: SyntaxKind.UnsignedRightShiftEquals;
				}
				else
				{
					// >>
					token1Kind = SyntaxKind.RightShift;
				}
			}
			else
			{
				// >>=
				token1Kind = SyntaxKind.RightShiftEquals;
			}
		}

		// TODO: Adapt this logic
		if (token1Kind.IsAssignmentExpressionOperatorToken())
		{
			return (token1Kind, token1Kind.ToAssignmentExpressionKind());
		}

		if (token1Kind.IsBinaryExpressionOperatorToken())
		{
			return (token1Kind, token1Kind.ToBinaryExpressionKind());
		}

		// something that doesn't expand the current expression we're looking at.  Bail out and see if we
		// can end with a conditional expression.
		return (SyntaxKind.None, SyntaxKind.None);
	}

	private SyntaxToken ConsumeExpressionOperatorToken(SyntaxKind operatorTokenKind)
	{
		if (operatorTokenKind == SyntaxKind.RightShift ||
			operatorTokenKind == SyntaxKind.RightShiftEquals)
		{
			// >> and >>=
			SyntaxToken token1 = PeekAndAdvance();
			SyntaxToken token2 = PeekAndAdvance();

			return SyntaxToken.Merge(token1, token2, operatorTokenKind);
		}

		if (operatorTokenKind == SyntaxKind.UnsignedRightShift ||
			operatorTokenKind == SyntaxKind.UnsignedRightShiftEquals)
		{
			// >>> and >>>=
			SyntaxToken token1 = PeekAndAdvance();
			_ = PeekAndAdvance();
			SyntaxToken token3 = PeekAndAdvance();

			return SyntaxToken.Merge(token1, token3, operatorTokenKind);
		}

		return PeekAndAdvance();
	}

	private ExpressionSyntax? TryExpandExpression(ExpressionSyntax leftOperand, Precedence precedence)
	{
		(SyntaxKind operatorTokenKind, SyntaxKind operatorExpressionKind) = GetExpressionOperatorTokenKindAndExpressionKind();

		if (operatorTokenKind == SyntaxKind.None)
			return null;

		Precedence newPrecedence = operatorExpressionKind.GetPrecedence();

		if (newPrecedence < precedence)
			return null;

		if ((newPrecedence == precedence) && !operatorExpressionKind.IsRightAssociative)
			return null;

		SyntaxToken operatorToken = ConsumeExpressionOperatorToken(operatorTokenKind);

		if (newPrecedence > operatorExpressionKind.GetPrecedence())
		{
			throw ExceptionUtilities.Unreachable();
		}

		if (operatorToken.Kind.IsAssignmentExpressionOperatorToken())
		{
			// return ParseAssignmentExpression(operatorExpressionKind, leftOperand, operatorToken);
		}
		if (operatorToken.Kind.IsBinaryExpressionOperatorToken())
		{
			return new BinaryExpressionSyntax(operatorExpressionKind, leftOperand.SyntaxTree, leftOperand, operatorToken, ParseSubExpression(newPrecedence));
		}

		// TODO: error-prone
		// This is also a valid path?
		throw ExceptionUtilities.Unreachable();
	}

	private ExpressionSyntax ParseParenthesizedExpression()
	{
		SyntaxToken openParen = MatchToken(SyntaxKind.OpenParen);

		ExpressionSyntax expression = ParseExpression();

		SyntaxToken closeParen = MatchToken(SyntaxKind.CloseParen);

		return new ParenthesizedExpressionSyntax(_tree, openParen, expression, closeParen);
	}
}