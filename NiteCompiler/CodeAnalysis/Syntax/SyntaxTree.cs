using NiteCompiler.Compilation;
using NiteCompiler.Diagnostics;
using NiteCompiler.Text;

namespace NiteCompiler.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
	public SourceText Text { get; }
	public RootSyntax Root { get; private set; } = null!;
	private DiagnosticBag Diagnostics { get; } = [];
	public string? FilePath { get; }

	private SyntaxTree(SourceText source, string? filePath)
	{
		Text = source;
		FilePath = filePath;
	}

	/// <summary>
	/// Creates <see cref="SyntaxTree"/> instance from provided text. Does not throw an exception on invalid syntax.
	/// </summary>
	/// <param name="text">The provided text.</param>
	/// <param name="filePath">The source of provided text.</param>
	/// <param name="options">Parse configuration.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A new syntax tree instance from provided text.</returns>
	public static SyntaxTree FromText(string text, string? filePath = null, NiteParseOptions? options = null, CancellationToken cancellationToken = default)
	{
		options ??= NiteParseOptions.Default;
		SourceText source = SourceText.FromText(text);
		SyntaxTree syntaxTree = new(source, filePath);

		Parser parser = new(syntaxTree, source, options, syntaxTree.Diagnostics, cancellationToken);

		RootSyntax root = parser.ParseRoot();
		syntaxTree.Root = root;

		return syntaxTree;
	}

	/// <summary>
	/// Creates <see cref="SyntaxTree"/> instance from provided file. Does not throw an exception on invalid syntax.
	/// </summary>
	/// <param name="file">The provided file.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A new syntax tree instance from content of provided file.</returns>
	/// <exception cref="FileNotFoundException">Cannot find the provided file.</exception>
	/// <exception cref="UnauthorizedAccessException">Operating system denied operation over the provided file.</exception>
	public static SyntaxTree FromFile(FileInfo file, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
	{
		return Diagnostics;
	}

	internal SyntaxNode? LookupForParent(SyntaxNode node)
	{
		throw new NotImplementedException();
	}
}