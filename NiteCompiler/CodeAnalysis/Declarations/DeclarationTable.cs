using System.Collections.Immutable;
using NiteCompiler.CodeAnalysis.Syntax;

namespace NiteCompiler.CodeAnalysis.Declarations;

/// <summary>
/// A declaration table keeps track of root module declarations from parse trees. It is optimized
/// for one set of declarations that stays constant ('older') plus a single root declaration for the
/// currently edited file ('latest') which is added and removed repeatedly. The merged root of the
/// 'older' forest is cached and re-used as long as that forest does not change.
/// </summary>
/// <remarks>
/// Diagrammatically:
/// <code>
///                   MergedRoot
///                  /          \
///   old merged root            new merged root
///  /   |   |   |   \                \
/// old singles forest                 new single tree
/// </code>
/// </remarks>
internal sealed class DeclarationTable
{
	public static readonly DeclarationTable Empty = new([], latestLazyRootDeclaration: null, cache: null);

	// All root declarations, split so the unchanging 'older' forest can be cached separately
	// from the constantly changing 'latest' declaration. Insertion order = syntax tree order.
	internal ImmutableArray<Lazy<RootModuleDeclaration>> AllOlderRootDeclarations { get; }
	internal Lazy<RootModuleDeclaration>? LatestLazyRootDeclaration { get; }

	// Cache of values computed over AllOlderRootDeclarations only; re-used while that set is unchanged.
	internal Cache MergedCache { get; }

	// The lazily computed total merged declaration.
	private MergedModuleDeclaration? _mergedRoot;

	internal DeclarationTable(
		ImmutableArray<Lazy<RootModuleDeclaration>> allOlderRootDeclarations,
		Lazy<RootModuleDeclaration>? latestLazyRootDeclaration,
		Cache? cache)
	{
		AllOlderRootDeclarations = allOlderRootDeclarations;
		LatestLazyRootDeclaration = latestLazyRootDeclaration;
		MergedCache = cache ?? new Cache(this);
	}

	public DeclarationTable(ImmutableArray<SyntaxTree> trees)
		: this(CreateLazyRoots(trees), latestLazyRootDeclaration: null, cache: null)
	{
	}

	private static ImmutableArray<Lazy<RootModuleDeclaration>> CreateLazyRoots(ImmutableArray<SyntaxTree> trees)
	{
		var roots = ImmutableArray.CreateBuilder<Lazy<RootModuleDeclaration>>(trees.Length);

		foreach (SyntaxTree tree in trees)
		{
			roots.Add(new Lazy<RootModuleDeclaration>(() => DeclarationTreeBuilder.ForTree(tree)));
		}

		return roots.ToImmutable();
	}

	public DeclarationTableBuilder ToBuilder()
	{
		return DeclarationTableBuilder.GetInstance(this);
	}

	public MergedModuleDeclaration GetMergedRoot()
	{
		if (_mergedRoot == null)
		{
			Interlocked.CompareExchange(ref _mergedRoot, CalculateMergedRoot(), null);
		}
		return _mergedRoot;
	}

	private MergedModuleDeclaration CalculateMergedRoot()
	{
		MergedModuleDeclaration oldRoot = MergedCache.MergedRoot;

		if (LatestLazyRootDeclaration == null)
		{
			return oldRoot;
		}

		// shortcut: result order is older-forest order with 'latest' appended last, not syntax-tree
		// order; correct while trees are only ever appended in order — upgrade to sorting roots by
		// source location when RemoveSyntaxTrees/ReplaceSyntaxTree lets an edit move 'latest' out of last position.
		var builder = ImmutableArray.CreateBuilder<SingleModuleDeclaration>(oldRoot.Declarations.Length + 1);
		builder.AddRange(oldRoot.Declarations);
		builder.Add(LatestLazyRootDeclaration.Value);

		return MergedModuleDeclaration.Create(builder.ToImmutable());
	}

	/// <summary>
	/// Values cached over the 'older' forest: the merged root of all older declarations, with each
	/// lazy root realized only on first merge.
	/// </summary>
	internal sealed class Cache
	{
		private readonly DeclarationTable _table;
		private MergedModuleDeclaration? _mergedRoot;

		internal Cache(DeclarationTable table)
		{
			_table = table;
		}

		internal MergedModuleDeclaration MergedRoot
		{
			get
			{
				if (_mergedRoot is null)
				{
					ImmutableArray<Lazy<RootModuleDeclaration>> olderRoots = _table.AllOlderRootDeclarations;

					var roots = ImmutableArray.CreateBuilder<SingleModuleDeclaration>(olderRoots.Length);
					foreach (Lazy<RootModuleDeclaration> root in olderRoots)
					{
						roots.Add(root.Value);
					}

					Interlocked.CompareExchange(
						ref _mergedRoot,
						MergedModuleDeclaration.Create(roots.ToImmutable()),
						comparand: null);
				}

				return _mergedRoot;
			}
		}
	}
}