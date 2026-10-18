using System.Collections.Concurrent;
using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Compilation;
using NiteCompiler.Symbols;
using NiteCompiler.Utilities;

namespace NiteCompiler.CodeAnalysis.Binding;

internal sealed partial class BinderFactory
{
	private static readonly ObjectPool<Visitor> sharedBinderFactoryVisitorPool = new(static () => new Visitor(), 64);

	private readonly NiteCompilation _compilation;
	private readonly SyntaxTree _syntaxTree;
	private readonly RootBinder _rootBinder;
	private readonly ConcurrentDictionary<BinderCache, Binder> _binderCache;

	private readonly ObjectPool<Visitor> _binderFactoryVisitorPool;

	public BinderFactory(NiteCompilation compilation, SyntaxTree syntaxTree, ObjectPool<Visitor>? binderFactoryVisitorPool = null)
	{
		_compilation = compilation;
		_syntaxTree = syntaxTree;
		_binderCache = [];

		_binderFactoryVisitorPool = binderFactoryVisitorPool ?? sharedBinderFactoryVisitorPool;

		_rootBinder = new(compilation, syntaxTree);
	}

	public Binder GetBinder(SyntaxNode node, SyntaxNode? memberDeclaration = null, Symbol? member = null)
	{
		int position = node.Span.Start;

		return GetBinder(node, position, memberDeclaration, member);
	}

	public Binder GetBinder(SyntaxNode node, int position, SyntaxNode? memberDeclaration = null, Symbol? member = null)
	{
		Debug.Assert(node != null);

		Visitor visitor = GetBinderFactoryVisitor(position, memberDeclaration, member);
		Binder? result = visitor.Visit(node);
		Debug.Assert(result != null, $"result != null; node: {node.Kind}");
		ClearBinderFactoryVisitor(visitor);

		return result;
	}

	private Visitor GetBinderFactoryVisitor(int position, SyntaxNode? memberDeclaration, Symbol? member)
	{
		Visitor visitor = _binderFactoryVisitorPool.Allocate();
		visitor.Initialize(factory: this, position, memberDeclaration, member);

		return visitor;
	}

	private void ClearBinderFactoryVisitor(Visitor visitor)
	{
		visitor.Clear();
		_binderFactoryVisitorPool.Free(visitor);
	}
}