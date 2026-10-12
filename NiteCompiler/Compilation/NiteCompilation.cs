using System.Collections.Immutable;
using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.Compilation;

public sealed partial class NiteCompilation : ICloneable
{
	private readonly string? _libraryName;
	private readonly NiteCompilationOptions _options;

	private readonly SyntaxAndDeclarationManager _syntaxAndDeclarations;

	public static NiteCompilation Create(string? libraryName, NiteCompilationOptions options, IEnumerable<SyntaxTree>? syntaxTrees)
	{
		Debug.Assert(options != null);

		NiteCompilation compilation = new(libraryName, options, new SyntaxAndDeclarationManager(ImmutableArray<SyntaxTree>.Empty, null));

		if (syntaxTrees != null)
		{
			compilation = compilation.AddSyntaxTrees(syntaxTrees);
		}

		return compilation;
	}

	private NiteCompilation(string? libraryName, NiteCompilationOptions options,
		SyntaxAndDeclarationManager syntaxAndDeclarations)
	{
		Debug.Assert(options != null);
		Debug.Assert(syntaxAndDeclarations != null);

		_libraryName = libraryName;
		_options = options;
		_syntaxAndDeclarations = syntaxAndDeclarations;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public NiteCompilation Clone()
	{
		return new NiteCompilation(
			_libraryName,
			_options,
			_syntaxAndDeclarations
			);
	}

	private NiteCompilation Update(SyntaxAndDeclarationManager syntaxAndDeclarations)
	{
		return new NiteCompilation(
			_libraryName,
			_options,
			syntaxAndDeclarations
		);
	}
}