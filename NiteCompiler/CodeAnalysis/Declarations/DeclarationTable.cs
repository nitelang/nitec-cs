using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.CodeAnalysis.Declarations;

internal sealed class DeclarationTable
{
	// TODO: Roslyn parity — roots are currently rebuilt from the raw syntax trees on demand (no Lazy<RootModuleDeclaration>,
	// no older/latest split, no Cache of merged root/type+module names, no source-location ordering).
	// see DeclarationTableBuilder for the pending builder.
	public static readonly DeclarationTable Empty = new(ImmutableArray<SyntaxTree>.Empty);

	public ImmutableArray<SyntaxTree> Trees { get; }
	private MergedModuleDeclaration? _lateinitRoot;

	public DeclarationTable(ImmutableArray<SyntaxTree> trees)
	{
		Trees = trees;
	}

	public DeclarationTableBuilder ToBuilder()
	{
		return DeclarationTableBuilder.GetInstance(this);
	}

	public MergedModuleDeclaration GetMergedRoot()
	{
		if (_lateinitRoot == null)
		{
			Interlocked.CompareExchange(ref _lateinitRoot, CalculateMergedRoot(), null);
		}
		return _lateinitRoot;
	}

	private MergedModuleDeclaration CalculateMergedRoot()
	{
		var builder = ImmutableArray.CreateBuilder<SingleModuleDeclaration>();

		foreach (var tree in Trees)
		{
			builder.Add(DeclarationTreeBuilder.ForTree(tree));
		}

		return MergedModuleDeclaration.Create(builder.ToImmutable());
	}
}