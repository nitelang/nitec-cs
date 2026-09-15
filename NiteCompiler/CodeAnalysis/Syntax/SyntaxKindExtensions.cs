namespace NiteCompiler.CodeAnalysis.Syntax;

public static partial class SyntaxKindExtensions
{
	extension(SyntaxKind kind)
	{
		public bool IsRightAssociative => kind.IsAssignmentExpressionOperatorToken();
	}
}