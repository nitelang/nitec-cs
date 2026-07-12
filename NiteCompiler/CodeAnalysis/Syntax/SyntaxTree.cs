namespace NiteCompiler.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
	public static SyntaxTree FromText(string text)
	{
		throw new NotImplementedException();
	}

	public static SyntaxTree FromFile(FileInfo file)
	{
		throw new NotImplementedException();
	}

	internal SyntaxNode? LookupForParent(SyntaxNode node)
	{
		throw new NotImplementedException();
	}
}