using System.Collections.Concurrent;
using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Compilation;

public partial class NiteCompilation
{
	private ConcurrentDictionary<SyntaxTree, SemanticModel>? _semanticModels;

	/// <summary>
	/// Returns the <see cref="SemanticModel"/> for <paramref name="syntaxTree"/>.
	/// </summary>
	/// <param name="syntaxTree">A tree that belongs to this compilation.</param>
	/// <exception cref="ArgumentNullException">Provided <paramref name="syntaxTree"/> is null.</exception>
	/// <exception cref="ArgumentException">Provided <paramref name="syntaxTree"/> is not part of this compilation.</exception>
	public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
	{
		ArgumentNullException.ThrowIfNull(syntaxTree);

		if (!SyntaxTrees.Contains(syntaxTree))
		{
			throw new ArgumentException("Syntax tree is not part of the compilation.", nameof(syntaxTree));
		}

		if (_semanticModels is null)
		{
			Interlocked.CompareExchange(ref _semanticModels, [], null);
		}

		return _semanticModels.GetOrAdd(syntaxTree, static (tree, compilation) => new SyntaxTreeSemanticModel(compilation, tree), this);
	}
}