using System.Collections.Concurrent;
using System.Diagnostics;
using NiteCompiler.CodeAnalysis.Syntax;
using NiteCompiler.Compilation;
using NiteCompiler.Symbols;

namespace NiteCompiler.CodeAnalysis.Binding;

internal partial class BinderFactory
{
	private record struct BinderCache(SyntaxNode Node, NodeUsage Usage);

	internal enum NodeUsage
	{
		Normal = 0,
		FunctionTemplateParameters = 1 << 0,
		FunctionBody = 1 << 1,

		ModuleBody = 1 << 0,
	}

	internal sealed class Visitor : SyntaxVisitor<Binder>
	{
		private int _position;
		private SyntaxNode? _memberDeclaration;
		private Symbol? _member;
		private BinderFactory _factory = null!;

		public void Initialize(BinderFactory factory, int position, SyntaxNode? memberDeclaration, Symbol? member)
		{
			Debug.Assert((memberDeclaration == null) == (member == null));

			_factory = factory;
			_position = position;
			_memberDeclaration = memberDeclaration;
			_member = member;
		}

		public void Clear()
		{
			_factory = null!;
			_position = 0;
			_memberDeclaration = null;
			_member = null;
		}

		private NiteCompilation Compilation => _factory._compilation;
		private SyntaxTree SyntaxTree => _factory._syntaxTree;
		private RootBinder RootBinder => _factory._rootBinder;
		private ConcurrentDictionary<BinderCache, Binder> BinderCache => _factory._binderCache;
	}
}