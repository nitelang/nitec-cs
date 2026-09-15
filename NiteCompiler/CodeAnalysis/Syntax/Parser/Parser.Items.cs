using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Syntax;

internal partial class Parser
{
	public RootSyntax ParseRoot()
	{
		ArrayBuilder<ExpressionSyntax> expressions = ArrayBuilder<ExpressionSyntax>.GetInstance();
		while (Current.Kind != SyntaxKind.EndOfFile)
		{
			expressions.Add(ParseExpression());
		}

		SyntaxToken endOfFile = MatchToken(SyntaxKind.EndOfFile);

		return new RootSyntax(_tree, expressions.ToSyntaxListAndFree(), endOfFile);
	}
}