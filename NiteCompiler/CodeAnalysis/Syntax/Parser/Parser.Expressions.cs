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
			throw new NotImplementedException();
		}

		ExpressionSyntax ParsePostFixExpression(ExpressionSyntax expression)
		{
			throw new NotImplementedException();
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

	private ExpressionSyntax? TryExpandExpression(ExpressionSyntax leftOperand, Precedence precedence)
	{
		throw new NotImplementedException();
	}
}