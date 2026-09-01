namespace NiteCompiler.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
	/// <summary>
	/// Creates <see cref="SyntaxTree"/> instance from provided text. Does not throw an exception on invalid syntax.
	/// </summary>
	/// <param name="text">The provided text.</param>
	/// <returns>A new syntax tree instance from provided text.</returns>
	public static SyntaxTree FromText(string text)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Creates <see cref="SyntaxTree"/> instance from provided file. Does not throw an exception on invalid syntax.
	/// </summary>
	/// <param name="file">The provided file.</param>
	/// <returns>A new syntax tree instance from content of provided file.</returns>
	/// <exception cref="FileNotFoundException">Cannot find the provided file.</exception>
	/// <exception cref="UnauthorizedAccessException">Operating system denied operation over the provided file.</exception>
	public static SyntaxTree FromFile(FileInfo file)
	{
		throw new NotImplementedException();
	}

	internal SyntaxNode? LookupForParent(SyntaxNode node)
	{
		throw new NotImplementedException();
	}
}